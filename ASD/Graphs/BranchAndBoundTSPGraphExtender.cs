using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o rozwiązywanie problemu komiwojażera metodą podziału i ograniczeń
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class BranchAndBoundTSPGraphExtender
    {
        /// <summary>
        /// Znajduje rozwiązanie dokładne problemu komiwojażera metodą podziału i ograniczeń
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="multiThread">Informacja czy korzystać z jednowątkowej czy wielowątkowej wersji algorytmu</param>
        /// <returns>Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi) znalezionego cyklu i tablicy krawędzi tworzących ten cykl)</returns>
        /// <remarks>
        /// Metoda przeznaczona jest dla grafów z nieujemnymi wagami krawędzi.<para/>
        /// Uruchomiona dla grafu zawierającego krawędź o wadze ujemnej zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// W wielowątkowej wersji algorytmu liczba wątków dobierana jest automatycznie z zakresu od 1 do liczby sprzętowych wątków w procesorze.<para/>
        /// Mniejsza niż maksymalna dostępna liczba wątków może być korzystna ze względu na wymagania pamięciowe algorytmu (każdy wątek wymaga własnego zestawu wszystkich tablic roboczych).<para/>
        /// Elementy (krawędzie) umieszczone są w tablicy cycle w kolejności swojego następstwa w znalezionym cyklu Hamiltona.<para/>
        /// Jeśli w badanym grafie nie istnieje cykl Hamiltona metoda zwraca krotkę (NaN,null).<para/>
        /// Metodę można stosować dla grafów skierowanych i nieskierowanych.
        /// </remarks>
        /// <exception cref="ArgumentException">Gdy ruchomiona dla grafu zawierającego krawędź o wadze ujemnej</exception>
        /// <seealso cref="BranchAndBoundTSPGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) BranchAndBoundTSP(this Graph g, bool multiThread = false)
        {
            var verticesCount = g.VerticesCount;
            if (verticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);
            var tab = new double[verticesCount + 1, verticesCount + 1];
            var array2 = new bool[verticesCount];
            
            for (var i = 0; i < verticesCount; i++)
                for (var j = 0; j < verticesCount; j++)
                    tab[i, j] = double.PositiveInfinity;

            for (var i = 0; i < verticesCount; i++)
            {
                var min = double.PositiveInfinity;
                foreach (var edge in g.OutEdges(i))
                {
                    if (edge.Weight < 0.0)
                        throw new ArgumentException("Negative weights are not allowed");
                    tab[i, edge.To] = edge.Weight;
                    if (min > edge.Weight && i != edge.To) 
                        min = edge.Weight;
                }
                
                if (double.IsPositiveInfinity(min))
                    return (double.NaN, null);
                
                tab[i, i] = double.NaN;
                
                tab[verticesCount, i] = tab[i, verticesCount] = i;
                
                tab[verticesCount, verticesCount] += min;
                
                for (var j = 0; j < verticesCount; j++)
                {
                    tab[i, j] -= min;
                    if (tab[i, j] == 0.0) array2[j] = true;
                }
            }
            
            for (var i = 0; i < verticesCount; i++)
            {
                if (array2[i]) continue;
                
                var min = double.PositiveInfinity;
                for (var j = 0; j < verticesCount; j++)
                    if (min > tab[j, i])
                        min = tab[j, i];
                if (double.IsPositiveInfinity(min))
                    return (double.NaN, null);
                tab[verticesCount, verticesCount] += min;
                for (var j = 0; j < verticesCount; j++) tab[j, i] -= min;
            }
            
            var branchAndBoundHelper = new BranchAndBoundHelper(g, multiThread);
            var state = new State(tab, new Edge[g.VerticesCount]);

            if (multiThread)
                branchAndBoundHelper.BranchAndBoundMultiThread(state);
            else
                branchAndBoundHelper.BranchAndBoundSingleThread(state);
            
            return double.IsPositiveInfinity(branchAndBoundHelper.bestWeight) ? (double.NaN, null) : (branchAndBoundHelper.bestWeight, branchAndBoundHelper.GetCycle());
        }

        private struct State
        {
            internal State(double[,] tab, Edge[] edges)
            {
                this.tab = tab;
                this.edges = edges;
            }
            
            internal double[,] tab;
            internal Edge[] edges;
            
        }

        private sealed class BranchAndBoundHelper
        {
            internal double bestWeight;

            [ThreadStatic]
            private static double[][,] tabs;

            private Graph g;

            private Edge[] edges;

            private int processors;

            private ConcurrentStack<State> stack;

            private volatile int resourcesUsed;

            private readonly object mutex = new object();

            private Exception exception;
            
            internal BranchAndBoundHelper(Graph g, bool multiThread)
            {
                this.g = g;
                bestWeight = double.PositiveInfinity;
                if (multiThread)
                {
                    stack = new ConcurrentStack<State>();
                    return;
                }
                tabs = new double[g.VerticesCount + 1][,];
                for (var i = 2; i <= g.VerticesCount; i++)
                {
                    tabs[i] = new double[i, i];
                }
            }

            internal Edge[] GetCycle()
            {
                var cycle = new Edge[g.VerticesCount];
                var next = 0;
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    var j = 0;
                    while (edges[j].From != next) j++;
                    cycle[i] = edges[j];
                    next = edges[j].To;
                }
                return cycle;
            }

            internal void BranchAndBoundMultiThread(State state)
            {
                processors = Environment.ProcessorCount;
                stack.Push(state);
                resourcesUsed = processors + 1;
                var threads = new Thread[processors];
                for (var i = 0; i < threads.Length; i++)
                {
                    threads[i] = new Thread(SolveThread, 0);
                    threads[i].Start();
                }
                foreach (var thread in threads) thread.Join();
                if (exception == null) return;
                Console.WriteLine("Download more ram. Idk");
                throw exception;
            }

            private void SolveThread()
            {
                var flag = true;
                try
                {
                    State state = default;
                    var problemSizeThreshold = g.VerticesCount - processors;
                    if (problemSizeThreshold < 45) problemSizeThreshold = 45;
                    var resourcesUsedThreshold = 6 * processors;
                    tabs = new double[g.VerticesCount][,];
                    for (var i = 2; i < g.VerticesCount; i++) tabs[i] = new double[i, i];
                    while (true)
                    {
                        if (flag)
                        {
                            Interlocked.Decrement(ref resourcesUsed);
                            while (!stack.TryPop(out state))
                            {
                                if (resourcesUsed == 0)
                                    return;
                                Thread.Sleep(10);
                            }
                        }
                        flag = true;
                        var problemSize = state.tab.GetLength(0) - 1;
                        if (!(state.tab[problemSize, problemSize] < bestWeight)) continue;
                        if (problemSize == 2)
                            SolveElementaryProblem(state);
                        else
                        {
                            var (m, i, j) = FindBestLimit(state.tab);
                            if (!(m >= 0.0)) continue;
                            var nextState = SolveProblem(state, i, j, new double[problemSize, problemSize]);
                            if (state.tab[problemSize, problemSize] + m < bestWeight)
                            {
                                state.tab[i, j] = double.PositiveInfinity;
                                if (ProcessColumn(state.tab, i) && ProcessRow(state.tab, j))
                                    stack.Push(new State(state.tab, (Edge[]) state.edges.Clone()));
                                Interlocked.Increment(ref resourcesUsed);
                            }
                            if (problemSize >= problemSizeThreshold && resourcesUsed <= resourcesUsedThreshold)
                            {
                                state = nextState;
                                flag = false;
                            }
                            else
                                BranchAndBoundSingleThread(nextState);
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (exception == null) exception = ex;
                    Interlocked.Decrement(ref resourcesUsed);
                }
            }

            internal void BranchAndBoundSingleThread(State state)
            {
                var problemSize = state.tab.GetLength(0) - 1;
                while (true)
                {
                    if (state.tab[problemSize, problemSize] >= bestWeight)
                        return;
                    if (problemSize == 2)
                        break;
                    var (m, i, j) = FindBestLimit(state.tab);
                    if (m < 0.0)
                        return;
                    BranchAndBoundSingleThread(SolveProblem(state, i, j, tabs[problemSize]));
                    if (state.tab[problemSize, problemSize] + m >= bestWeight)
                        return;
                    state.tab[i, j] = double.PositiveInfinity;
                    if (ProcessColumn(state.tab, i)) ProcessRow(state.tab, j);
                }
                SolveElementaryProblem(state);
            }

            private State SolveProblem(State state, int ii, int jj, double[,] tab)
            {
                var problemSize = state.tab.GetLength(0) - 1;
                var naNColumns = new bool[problemSize - 1];
                var naNRows = new bool[problemSize - 1];
                var zeroColumns = new bool[problemSize - 1];
                var zeroRows = new bool[problemSize - 1];
                var m = 0;
                var i = 0;
                int j;
                while (m <= problemSize)
                {
                    if (m == ii)
                        i--;
                    else
                    {
                        var n = 0;
                        j = 0;
                        while (n <= problemSize)
                        {
                            if (n == jj)
                                j--;
                            else
                            {
                                tab[i, j] = state.tab[m, n];
                                if (m != problemSize && n != problemSize)
                                {
                                    if (tab[i, j] == 0.0)
                                    {
                                        zeroRows[j] = true;
                                        zeroColumns[i] = true;
                                    }
                                    if (tab[i, j].IsNaN())
                                    {
                                        naNRows[j] = true;
                                        naNColumns[i] = true;
                                    }
                                }
                            }
                            n++;
                            j++;
                        }
                    }
                    m++;
                    i++;
                }
                i = 0;
                while (i < problemSize - 1 && naNColumns[i]) i++;
                j = 0;
                while (j < problemSize - 1 && naNRows[j]) j++;
                if (tab[i, j] == 0.0)
                {
                    zeroRows[j] = false;
                    zeroColumns[i] = false;
                }
                tab[i, j] = double.NaN;
                var from = (int)state.tab[ii, problemSize];
                var to = (int)state.tab[problemSize, jj];
                state.edges[problemSize - 1] = new Edge(from, to, g.GetEdgeWeight(from, to));
                for (var c = 0; c < problemSize - 1; c++)
                    if (!zeroColumns[c])
                        ProcessColumn(tab, c);
                for (var r = 0; r < problemSize - 1; r++)
                    if (!zeroRows[r])
                        ProcessRow(tab, r);
                return new State(tab, state.edges);
            }

            private static (double m, int i, int j) FindBestLimit(double[,] tab)
            {
                var problemSize = tab.GetLength(0) - 1;
                var chosenColumns = new bool[problemSize];
                var chosenRows = new bool[problemSize];
                var lowestWeightsColumn = new double[problemSize];
                var lowestWeightsRow = new double[problemSize];
                var mm = -1.0;
                var jj = -1;
                var ii = -1;
                for (var i = 0; i < problemSize; i++)
                {
                    lowestWeightsRow[i] = double.PositiveInfinity;
                    lowestWeightsColumn[i] = float.PositiveInfinity;
                }
                for (var i = 0; i < problemSize; i++)
                {
                    for (var j = 0; j < problemSize; j++)
                    {
                        if (tab[i, j] == 0.0)
                        {
                            if (!chosenColumns[i])
                            {
                                for (var k = j + 1; k < problemSize; k++)
                                {
                                    if (!(lowestWeightsColumn[i] > tab[i, k])) continue;
                                    lowestWeightsColumn[i] = tab[i, k];
                                    if (lowestWeightsColumn[i] == 0.0)
                                        break;
                                }
                                chosenColumns[i] = true;
                            }
                            if (!chosenRows[j])
                            {
                                for (var k = i + 1; k < problemSize; k++)
                                {
                                    if (!(lowestWeightsRow[j] > tab[k, j])) continue;
                                    lowestWeightsRow[j] = tab[k, j];
                                    if (lowestWeightsRow[j] == 0.0)
                                        break;
                                }
                                chosenRows[j] = true;
                            }

                            if (!(mm < lowestWeightsColumn[i] + lowestWeightsRow[j])) continue;
                            mm = lowestWeightsColumn[i] + lowestWeightsRow[j];
                            ii = i;
                            jj = j;
                        }
                        else
                        {
                            var weight = tab[i, j];
                            if (lowestWeightsColumn[i] > weight) lowestWeightsColumn[i] = weight;
                            if (lowestWeightsRow[j] > weight) lowestWeightsRow[j] = weight;
                        }
                    }
                    if (!chosenColumns[i])
                        return (-1.0, -1, -1);
                }
                for (var i = 0; i < problemSize; i++)
                    if (!chosenRows[i])
                        return (-1.0, -1, -1);
                return (mm, ii, jj);
            }

            private static bool ProcessColumn(double[,] tab, int i)
            {
                var problemSize = tab.GetLength(0) - 1;
                var min = double.PositiveInfinity;
                for (var j = 0; j < problemSize; j++)
                {
                    if (!(min > tab[i, j])) continue;
                    min = tab[i, j];
                    if (min == 0.0)
                        break;
                }
                if (double.IsPositiveInfinity(min))
                {
                    tab[problemSize, problemSize] = double.PositiveInfinity;
                    return false;
                }

                if (min <= 0.0) return true;
                {
                    tab[problemSize, problemSize] += min;
                    for (var j = 0; j < problemSize; j++) 
                        tab[i, j] -= min;
                }
                return true;
            }

            private static bool ProcessRow(double[,] tab, int j)
            {
                var problemSize = tab.GetLength(0) - 1;
                var min = double.PositiveInfinity;
                for (var i = 0; i < problemSize; i++)
                {
                    if (!(min > tab[i, j])) continue;
                    min = tab[i, j];
                    if (min == 0.0)
                        break;
                }
                if (double.IsPositiveInfinity(min))
                {
                    tab[problemSize, problemSize] = double.PositiveInfinity;
                    return false;
                }

                if (min <= 0.0) return true;
                {
                    tab[problemSize, problemSize] += min;
                    for (var i = 0; i < problemSize; i++) 
                        tab[i, j] -= min;
                }
                return true;
            }

            private void SolveElementaryProblem(State state)
            {
                if (state.tab[2, 2] >= bestWeight)
                {
                    return;
                }
                if (!state.tab[0, 0].IsNaN())
                {
                    var from = (int)state.tab[0, 2];
                    var to = (int)state.tab[2, 0];
                    state.edges[0] = new Edge(from, to, g.GetEdgeWeight(from, to));
                    from = (int)state.tab[1, 2];
                    to = (int)state.tab[2, 1];
                    state.edges[1] = new Edge(from, to, g.GetEdgeWeight(from, to));
                }
                else
                {
                    var from = (int)state.tab[0, 2];
                    var to = (int)state.tab[2, 1];
                    state.edges[0] = new Edge(from, to, g.GetEdgeWeight(from, to));
                    from = (int)state.tab[1, 2];
                    to = (int)state.tab[2, 0];
                    state.edges[1] = new Edge(from, to, g.GetEdgeWeight(from, to));
                }
                lock (mutex)
                {
                    if (!(state.tab[2, 2] < bestWeight)) return;
                    bestWeight = state.tab[2, 2];
                    edges = (Edge[])state.edges.Clone();
                }
            }
        }
    }
}
