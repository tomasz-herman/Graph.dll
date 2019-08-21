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
            
            var class44 = new BreachAndBoundHelper(g, multiThread);
            var struct8 = new State(tab, new Edge[g.VerticesCount]);

            if (multiThread)
                class44.BreachAndBoundMultiThread(struct8);
            else
                class44.BreachAndBoundSingleThread(struct8);
            
            return double.IsPositiveInfinity(class44.weight) ? (double.NaN, null) : (class44.weight, class44.GetCycle());
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

        private sealed class BreachAndBoundHelper
        {
            internal double weight;

            [ThreadStatic]
            private static double[][,] tabs;

            private Graph g;

            private Edge[] edges;

            private int int_0;

            private ConcurrentStack<State> concurrentStack_0;

            private volatile int int_1;

            private readonly object mutex = new object();

            private Exception exceptionallyBadNameForAnException;
            
            internal BreachAndBoundHelper(Graph g, bool multiThread)
            {
                this.g = g;
                weight = double.PositiveInfinity;
                if (multiThread)
                {
                    concurrentStack_0 = new ConcurrentStack<State>();
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

            internal void BreachAndBoundMultiThread(State state0)
            {
                int_0 = Environment.ProcessorCount;
                concurrentStack_0.Push(state0);
                int_1 = int_0 + 1;
                Thread[] array = new Thread[int_0];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new Thread(method_2, 0);
                    array[i].Start();
                }
                for (int j = 0; j < array.Length; j++)
                {
                    array[j].Join();
                }
                if (exceptionallyBadNameForAnException != null)
                {
                    throw exceptionallyBadNameForAnException;
                }
            }

            private void method_2()
            {
                var flag = true;
                try
                {
                    State state = default;
                    int problemSize = g.VerticesCount;
                    int num2 = g.VerticesCount - int_0;
                    if (num2 < 45) num2 = 45;
                    int num3 = 6 * int_0;
                    tabs = new double[g.VerticesCount][,];
                    for (var i = 2; i < g.VerticesCount; i++) tabs[i] = new double[i, i];
                    while (true)
                    {
                        if (flag)
                        {
                            Interlocked.Decrement(ref int_1);
                            while (!concurrentStack_0.TryPop(out state))
                            {
                                if (int_1 == 0)
                                    return;
                                Thread.Sleep(10);
                            }
                        }
                        flag = true;
                        problemSize = state.tab.GetLength(0) - 1;
                        if (!(state.tab[problemSize, problemSize] < weight)) continue;
                        if (problemSize == 2)
                            SolveElementaryProblem(state);
                        else
                        {
                            ValueTuple<double, int, int> valueTuple = FindBestLimit(state.tab);
                            double item = valueTuple.Item1;
                            int item2 = valueTuple.Item2;
                            int item3 = valueTuple.Item3;
                            if (item >= 0.0)
                            {
                                State struct2 = SolveProblem(state, item2, item3, new double[problemSize, problemSize]);
                                if (state.tab[problemSize, problemSize] + item < weight)
                                {
                                    state.tab[item2, item3] = double.PositiveInfinity;
                                    if (smethod_0(state.tab, item2) && smethod_1(state.tab, item3))
                                    {
                                        concurrentStack_0.Push(new State(state.tab, (Edge[])state.edges.Clone()));
                                    }
                                    Interlocked.Increment(ref int_1);
                                }
                                if (problemSize >= num2 && int_1 <= num3)
                                {
                                    state = struct2;
                                    flag = false;
                                }
                                else
                                {
                                    BreachAndBoundSingleThread(struct2);
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    if (exceptionallyBadNameForAnException == null)
                    {
                        exceptionallyBadNameForAnException = ex;
                    }
                    Interlocked.Decrement(ref int_1);
                }
            }

            internal void BreachAndBoundSingleThread(State state)
            {
                var problemSize = state.tab.GetLength(0) - 1;
                while (true)
                {
                    if (state.tab[problemSize, problemSize] >= weight)
                        return;
                    if (problemSize == 2)
                        break;
                    var (m, i, j) = FindBestLimit(state.tab);
                    if (m < 0.0)
                        return;
                    BreachAndBoundSingleThread(SolveProblem(state, i, j, tabs[problemSize]));
                    if (state.tab[problemSize, problemSize] + m >= weight)
                        return;
                    state.tab[i, j] = double.PositiveInfinity;
                    if (smethod_0(state.tab, i)) smethod_1(state.tab, j);
                }
                SolveElementaryProblem(state);
            }

            private State SolveProblem(State state0, int int_2, int int_3, double[,] double_2)
            {
                int num = state0.tab.GetLength(0) - 1;
                bool[] array = new bool[num - 1];
                bool[] array2 = new bool[num - 1];
                bool[] array3 = new bool[num - 1];
                bool[] array4 = new bool[num - 1];
                int num2 = 0;
                int i = 0;
                int j = num2;
                int l;
                while (i <= num)
                {
                    if (i == int_2)
                    {
                        j--;
                    }
                    else
                    {
                        int num3 = 0;
                        int k = 0;
                        l = num3;
                        while (k <= num)
                        {
                            if (k == int_3)
                            {
                                l--;
                            }
                            else
                            {
                                double_2[j, l] = state0.tab[i, k];
                                if (i != num && k != num)
                                {
                                    if (double_2[j, l] == 0.0)
                                    {
                                        bool[] array5 = array3;
                                        int num4 = j;
                                        array4[l] = true;
                                        array5[num4] = true;
                                    }
                                    if (double_2[j, l].IsNaN())
                                    {
                                        bool[] array6 = array;
                                        int num5 = j;
                                        array2[l] = true;
                                        array6[num5] = true;
                                    }
                                }
                            }
                            k++;
                            l++;
                        }
                    }
                    i++;
                    j++;
                }
                j = 0;
                while (j < num - 1 && array[j])
                {
                    j++;
                }
                l = 0;
                while (l < num - 1 && array2[l])
                {
                    l++;
                }
                if (double_2[j, l] == 0.0)
                {
                    bool[] array7 = array3;
                    int num6 = j;
                    array4[l] = false;
                    array7[num6] = false;
                }
                double_2[j, l] = double.NaN;
                int from = (int)state0.tab[int_2, num];
                int to = (int)state0.tab[num, int_3];
                state0.edges[num - 1] = new Edge(from, to, g.GetEdgeWeight(from, to));
                for (j = 0; j < num - 1; j++)
                {
                    if (!array3[j])
                    {
                        smethod_0(double_2, j);
                    }
                }
                for (l = 0; l < num - 1; l++)
                {
                    if (!array4[l])
                    {
                        smethod_1(double_2, l);
                    }
                }
                return new State(double_2, state0.edges);
            }

            private static (double m, int i, int j) FindBestLimit(double[,] tab)
            {
                var problemSize = tab.GetLength(0) - 1;
                bool[] chosenColumns = new bool[problemSize];
                bool[] chosenRows = new bool[problemSize];
                double[] array3 = new double[problemSize];
                double[] array4 = new double[problemSize];
                double mm = -1.0;
                int jj = -1;
                int ii = -1;
                for (var i = 0; i < problemSize; i++)
                {
                    array4[i] = double.PositiveInfinity;
                    array3[i] = float.PositiveInfinity;
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
                                    if (!(array3[i] > tab[i, k])) continue;
                                    array3[i] = tab[i, k];
                                    if (array3[i] == 0.0)
                                        break;
                                }
                                chosenColumns[i] = true;
                            }
                            if (!chosenRows[j])
                            {
                                for (var k = i + 1; k < problemSize; k++)
                                {
                                    if (!(array4[j] > tab[k, j])) continue;
                                    array4[j] = tab[k, j];
                                    if (array4[j] == 0.0)
                                        break;
                                }
                                chosenRows[j] = true;
                            }

                            if (!(mm < array3[i] + array4[j])) continue;
                            mm = array3[i] + array4[j];
                            ii = i;
                            jj = j;
                        }
                        else
                        {
                            var num6 = tab[i, j];
                            if (array3[i] > num6) array3[i] = num6;
                            if (array4[j] > num6) array4[j] = num6;
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

            private static bool smethod_0(double[,] matrix, int i)
            {
                var length = matrix.GetLength(0) - 1;
                var min = double.PositiveInfinity;
                for (var j = 0; j < length; j++)
                {
                    if (!(min > matrix[i, j])) continue;
                    min = matrix[i, j];
                    if (min == 0.0)
                        break;
                }
                if (double.IsPositiveInfinity(min))
                {
                    matrix[length, length] = double.PositiveInfinity;
                    return false;
                }

                if (min <= 0.0) return true;
                {
                    matrix[length, length] += min;
                    for (var j = 0; j < length; j++) 
                        matrix[i, j] -= min;
                }
                return true;
            }

            private static bool smethod_1(double[,] matrix, int j)
            {
                var length = matrix.GetLength(0) - 1;
                var min = double.PositiveInfinity;
                for (int i = 0; i < length; i++)
                {
                    if (!(min > matrix[i, j])) continue;
                    min = matrix[i, j];
                    if (min == 0.0)
                        break;
                }
                if (double.IsPositiveInfinity(min))
                {
                    matrix[length, length] = double.PositiveInfinity;
                    return false;
                }

                if (min <= 0.0) return true;
                {
                    matrix[length, length] += min;
                    for (var i = 0; i < length; i++) 
                        matrix[i, j] -= min;
                }
                return true;
            }

            private void SolveElementaryProblem(State state)
            {
                if (state.tab[2, 2] >= weight)
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
                    if (!(state.tab[2, 2] < weight)) return;
                    weight = state.tab[2, 2];
                    edges = (Edge[])state.edges.Clone();
                }
            }
        }
    }
}
