using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o algorytmy wyznaczania maksymalnego przepływu w sieciach
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class MaxFlowGraphExtender
    {
        /// <summary>
        /// Wyznacza maksymalny przepływ metodą FordaFulkersona lub metodą Dinica
        /// </summary>
        /// <param name="g">Badany graf (sieć przepływowa)</param>
        /// <param name="source">Wierzchołek źródłowy</param>
        /// <param name="target">Wierzchołek docelowy</param>
        /// <param name="af">Metoda powiększania przepływu</param>
        /// <param name="matrixToHashTable">Czy optymalizować sposób reprezentacji grafu rezydualnego</param>
        /// <returns>
        /// Krotka (value, flow) składająca się z wartości maksymalnego przepływu
        /// i grafu opisującego ten przepływ
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Jeśli jako parametr af zostanie wybrane wyszukiwanie ścieżki powiększającej to metoda realizuje
        /// algorytm Forda-Fulkersona, jeśli jako parametr af zostanie wybrane wyszukiwanie przepływu blokującego
        /// to metoda realizuje algorytm Dinica.<para/>
        ///  Można oczywiście zdefiniować własną metodę powiększania przepływu zgodną
        /// z typem delegacyjnym <see cref="AugmentFlow"/>.<para/>
        /// Jeśli parametr matrixToHashTable ma wartość true oraz graf g jest typu <see cref="AdjacencyMatrixGraph"/>
        /// (czyli macierz sąsiedztwa) i ma nie więcej niż 10% krawędzi, to wykorzystywany przez algorytm graf
        /// rezydualny jest typu <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/>.
        /// W przeciwnym przypadku graf rezydualny jest takiego samego typu jak graf g.<para/>
        /// Natomiast jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to inne grafy robocze
        /// (ścieżka powiększająca, przepływ blokujący, graf warstwowy) są typu
        /// <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/> niezależnie
        /// od wartości parametru matrixToHashTable.<para/>
        /// Wynikowy przepływ maksymalny flow zawsze jest takiego samego typu jak graf g
        /// (niezależnie od wartości parametru matrixToHashTable.<para/>
        /// Jeśli po danej krawędzi nie płynie żaden przepływ,
        /// to nadal jest ona w wynikowym grafie flow (oczywiście z wagą 0).<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego lub grafu
        /// z ujemnymi wagami krawędzi zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Gdy parametry source i target są równe metoda również zgłasza wyjątek <see cref="ArgumentException"/>.
        /// </remarks>
        /// <seealso cref="MaxFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double value, Graph flow) FordFulkersonDinicMaxFlow(this Graph g, int source, int target, AugmentFlow af, bool matrixToHashTable = true)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");
            if (source == target)
                throw new ArgumentException("Source and target must be different");
            if (af == null)
                throw new ArgumentException("Flow increase method not specified");
            
            var flow = g.IsolatedVerticesGraph();

            var residual = matrixToHashTable
                ? g is AdjacencyMatrixGraph && 10 * g.EdgesCount < g.VerticesCount * g.VerticesCount
                    ? new AdjacencyListsGraph<HashTableAdjacencyList>(true, g.VerticesCount)
                    : g.IsolatedVerticesGraph()
                : g.IsolatedVerticesGraph();
            
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var edge in g.OutEdges(i))
                {
                    if (edge.Weight < 0.0)
                        throw new ArgumentException("Negative capacity weights are not allowed");
                    flow.AddEdge(edge.From, edge.To, 0.0);
                    if (edge.Weight > 0.0)
                        residual.AddEdge(edge.From, edge.To, Math.Min(edge.Weight, double.MaxValue));
                }

            var value = 0.0;
            while (true)
            {
                var (augmentingValue, augmentingFlow) = af(residual, source, target);
                if (augmentingValue == 0.0)
                    break;
                value += augmentingValue;
                for (var i = 0; i < augmentingFlow.VerticesCount; i++)
                {
                    foreach (var edge in augmentingFlow.OutEdges(i))
                    {
                        var weight = flow.GetEdgeWeight(edge.To, edge.From);
                        if (weight > 0.0)
                        {
                            weight = flow.ModifyEdgeWeight(edge.To, edge.From, -edge.Weight);
                            if (weight < 0.0)
                            {
                                flow.ModifyEdgeWeight(edge.To, edge.From, -weight);
                                flow.ModifyEdgeWeight(edge.From, edge.To, -weight);
                            }
                        }
                        else
                            flow.ModifyEdgeWeight(edge.From, edge.To, edge.Weight);
                        if (residual.ModifyEdgeWeight(edge.From, edge.To, -edge.Weight) == 0.0) 
                            residual.DelEdge(edge);
                        if (residual.ModifyEdgeWeight(edge.To, edge.From, edge.Weight).IsNaN()) 
                            residual.AddEdge(edge.To, edge.From, edge.Weight);
                    }
                }
            }
            return (value, flow);
        }
        
        /// <summary>
        /// Wyznacza najkrótszą ścieżkę powiekszającą
        /// </summary>
        /// <param name="g">Graf rezydualny</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="t">Wierzchołek docelowy</param>
        /// <returns>
        /// Krotka (augmentingValue, augmentingFlow) składająca się z przepustowości wyznaczonej ścieżki
        /// i grafu opisującego tą ścieżkę
        /// </returns>
        /// <remarks>
        /// Jeśli ścieżka powiększająca nie istnieje, to zwracana jest krotka (0.0,null).<para/>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczona ścieżka powiększająca p
        /// jest typu <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/>,
        /// w przeciwnym przypadku ścieżka p jest takiego samego typu jak graf g.
        /// </remarks>
        /// <seealso cref="MaxFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double augmentingValue, Graph augmentingFlow) BFPath(this Graph g, int s, int t)
        {
            return g.FindPath<EdgesQueue>(s, t);
        }
        
        /// <summary>
        /// Wyznacza ścieżkę powiekszającą o maksymalnej przepustowości
        /// </summary>
        /// <param name="g">Graf rezydualny</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="t">Wierzchołek docelowy</param>
        /// <returns>
        /// Krotka (augmentingValue, augmentingFlow) składająca się z przepustowości wyznaczonej ścieżki
        /// i grafu opisującego tą ścieżkę
        /// </returns>
        /// <remarks>
        /// Jeśli ścieżka powiększająca nie istnieje, to zwracana jest krotka (0.0,null).<para/>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczona ścieżka powiększająca p jest typu
        /// <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/>,
        /// w przeciwnym przypadku ścieżka p jest takiego samego typu jak graf g.
        /// </remarks>
        /// <seealso cref="MaxFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double augmentingValue, Graph augmentingFlow) MaxFlowPath(this Graph g, int s, int t)
        {
            return g.FindPath<EdgesMaxPriorityQueue>(s, t);
        }
        
        /// <summary>
        /// Wyznacza ścieżkę powiekszającą o maksymalnej przepustowości spośród najkrótszych ścieżek
        /// </summary>
        /// <param name="g">Graf rezydualny</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="t">Wierzchołek docelowy</param>
        /// <returns>
        /// Krotka (augmentingValue, augmentingFlow) składająca się z przepustowości wyznaczonej ścieżki
        /// i grafu opisującego tą ścieżkę
        /// </returns>
        /// <remarks>
        /// Jeśli ścieżka powiększająca nie istnieje, to zwracana jest krotka (0.0,null).<para/>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczona ścieżka powiększająca p jest typu
        /// <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/>,
        /// w przeciwnym przypadku ścieżka p jest takiego samego typu jak graf g.
        /// </remarks>
        /// <seealso cref="MaxFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double augmentingValue, Graph augmentingFlow) BFMaxPath(this Graph g, int s, int t)
        {
            var pi = new PathsInfo[g.VerticesCount];
            var steps = new int[g.VerticesCount];
            steps[s] = 1;
            pi[s].Dist = double.PositiveInfinity;

            bool VisitEdge(Edge e)
            {
                if (steps[e.From] == steps[t])
                    return false;
                if (steps[e.To] == 0)
                    steps[e.To] = steps[e.From] + 1;
                else if (steps[e.To] != steps[e.From] + 1)
                    return true;
                var d = Math.Min(pi[e.From].Dist, e.Weight);
                if (!(pi[e.To].Dist < d)) return true;
                pi[e.To].Dist = d;
                pi[e.To].Last = e;
                return true;
            }

            g.GeneralSearchFrom<EdgesQueue>(s, null, null, VisitEdge);
            return (pi[t].Dist, g.BuildGraph(s, t, pi));
        }
        
        /// <summary>
        /// Wyznacza przepływ blokujący oryginalnym algorytmem Dinica
        /// </summary>
        /// <param name="g">Graf rezydualny</param>
        /// <param name="source">Wierzchołek źródłowy</param>
        /// <param name="target">Wierzchołek docelowy</param>
        /// <returns>
        /// Krotka (augmentingValue, augmentingFlow) składająca się z przepustowości wyznaczonej ścieżki
        /// i grafu opisującego tą ścieżkę
        /// </returns>
        /// <remarks>
        /// Dla zerowego przepływu blokującego zwracana jest krotka (0.0,null).<para/>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczony przepływ blokujący bf jest typu
        /// <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/>,
        /// w przeciwnym przypadku przepływ blokujący bf jest takiego samego typu jak graf g.
        /// </remarks>
        /// <seealso cref="MaxFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double augmentingValue, Graph augmentingFlow) OriginalDinicBlockingFlow(Graph g, int source, int target)
        {
            var list = new List<Edge>();
            var levelGraph = ConstructLevelGraph(g, source, target);
            if (levelGraph == null)
                return (0.0, null);
            var augmentingFlow = levelGraph.IsolatedVerticesGraph();
            var levelGraphReversed = levelGraph.Reverse();
            var augmentingValue = 0.0;
            var flag = true;
            while (flag)
            {
                list.Clear();
                var flow = double.PositiveInfinity;
                int to;
                for (var vert = source; vert != target; vert = to)
                {
                    var edge = levelGraph.OutEdges(vert).First();
                    list.Add(edge);
                    if (flow > edge.Weight) flow = edge.Weight;
                    to = edge.To;
                }
                augmentingValue += flow;
                foreach (var edge in list)
                {
                    if (!augmentingFlow.AddEdge(edge.From, edge.To, flow))
                        augmentingFlow.ModifyEdgeWeight(edge.From, edge.To, flow);
                    if (levelGraph.GetEdgeWeight(edge.From, edge.To) == flow)
                    {
                        levelGraph.DelEdge(edge);
                        levelGraphReversed.DelEdge(edge.To, edge.From);
                        if (levelGraph.OutDegree(edge.From) == 0 && IsPath(levelGraph, levelGraphReversed, edge.From, source))
                            flag = false;
                    }
                    else
                        levelGraph.ModifyEdgeWeight(edge.From, edge.To, -flow);
                }
            }
            return (augmentingValue, augmentingFlow);
        }
        
        /// <summary>
        /// Wyznacza przepływ blokujący algorytmem trzech Hindusów
        /// </summary>
        /// <param name="g">Graf rezydualny</param>
        /// <param name="source">Wierzchołek źródłowy</param>
        /// <param name="target">Wierzchołek docelowy</param>
        /// <returns>
        /// Krotka (augmentingValue, augmentingFlow) składająca się z przepustowości wyznaczonej ścieżki
        /// i grafu opisującego tą ścieżkę
        /// </returns>
        /// <remarks>
        /// Dla zerowego przepływu blokującego zwracana jest krotka (0.0,null).<para/>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczony przepływ blokujący bf jest typu
        /// <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/>,
        /// w przeciwnym przypadku przepływ blokujący bf jest takiego samego typu jak graf g.<para/>
        /// Wspomniani Hindusi to Malhotra, Kumar i Maheshwari.
        /// </remarks>
        /// <seealso cref="MaxFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double augmentingValue, Graph augmentingFlow) MKMBlockingFlow(Graph g, int source, int target)
        {
            var levelGraph = ConstructLevelGraph(g, source, target);
            if (levelGraph == null)
                return (0.0, null);
            var augmentingFlow = levelGraph.IsolatedVerticesGraph();
            var augmentingValue = levelGraph.GetEdgeWeight(source, target);
            if (!augmentingValue.IsNaN())
            {
                augmentingFlow.AddEdge(source, target, augmentingValue);
                return (augmentingValue, augmentingFlow);
            }
            var levelGraphReversed = levelGraph.Reverse();
            augmentingValue = 0.0;
            var mkmHelper = new MKMHelper(levelGraph, source, target);
            while (true)
            {
                var (index, minCapacity) = mkmHelper.MinimumCapacityVertex();
                if (index == -1)
                    break;
                augmentingValue += minCapacity;
                var dictionary = new Dictionary<int, double> {{index, minCapacity}};
                dictionary = mkmHelper.Push(levelGraph, levelGraphReversed, augmentingFlow, dictionary, true, true);;
                while (dictionary.Count > 0)
                {
                    dictionary = mkmHelper.Push(levelGraph, levelGraphReversed, augmentingFlow, dictionary, true, false);
                }

                dictionary = new Dictionary<int, double> {{index, minCapacity}};
                dictionary = mkmHelper.Push(levelGraphReversed, levelGraph, augmentingFlow, dictionary, false, true);
                while (dictionary.Count > 0)
                {
                    dictionary = mkmHelper.Push(levelGraphReversed, levelGraph, augmentingFlow, dictionary, false, false);
                }
                mkmHelper.pushed[index] = true;
            }
            return (augmentingValue, augmentingFlow);
        }
        
        /// <summary>
        /// Wyznacza przepływ blokujący algorytmem DFS
        /// </summary>
        /// <param name="g">Graf rezydualny</param>
        /// <param name="source">Wierzchołek źródłowy</param>
        /// <param name="target">Wierzchołek docelowy</param>
        /// <returns>
        /// Krotka (augmentingValue, augmentingFlow) składająca się z przepustowości wyznaczonej ścieżki
        /// i grafu opisującego tą ścieżkę
        /// </returns>
        /// <remarks>
        /// Dla zerowego przepływu blokującego zwracana jest krotka (0.0,null).<para/>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczony przepływ blokujący bf jest typu
        /// <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/>,
        /// w przeciwnym przypadku przepływ blokujący bf jest takiego samego typu jak graf g.<para/>
        /// Algorytm jest zainspirowany wykładem Marka Cygana (nagranym na Uniwersytecie Warszawskim w roku 2008)
        /// </remarks>
        /// <seealso cref="MaxFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double augmentingValue, Graph augmentingFlow) DFSBlockingFlow(Graph g, int source, int target)
        {
            var levelGraph = ConstructLevelGraph(g, source, target);
            if (levelGraph == null)
                return (0.0, null);
            var augmentingFlow = levelGraph.IsolatedVerticesGraph();
            return (DFS(levelGraph, augmentingFlow, source, target, double.PositiveInfinity), augmentingFlow);
        }
        
        private static (double augmentingValue, Graph augmentingFlow) FindPath<T>(this Graph g, int s, int t) where T : IEdgesContainer, new()
        {
            var pi = new PathsInfo[g.VerticesCount];
            pi[s].Dist = double.PositiveInfinity;
            
            bool VisitEdge(Edge e)
            {
                if (pi[e.To].Dist != 0.0) return true;
                pi[e.To].Dist = Math.Min(pi[e.From].Dist, e.Weight);
                pi[e.To].Last = e;
                return e.To != t;
            }

            g.GeneralSearchFrom<T>(s, null, null, VisitEdge);
            
            return (pi[t].Dist, g.BuildGraph(s, t, pi));
        }

        private static Graph BuildGraph(this Graph g, int s, int t, PathsInfo[] pi)
        {
            if (pi == null) throw new ArgumentNullException(nameof(pi));
            if (pi.Length == 0) throw new ArgumentException("Value cannot be an empty collection.", nameof(pi));

            var dist = pi[t].Dist;
            
            if (dist == 0.0)
                return null;
            
            var graph = (g is AdjacencyMatrixGraph) ? new AdjacencyListsGraph<HashTableAdjacencyList>(true, g.VerticesCount) : g.IsolatedVerticesGraph();
            int from;
            for (var vert = t; vert != s; vert = from)
            {
                from = pi[vert].Last.Value.From;
                graph.AddEdge(from, vert, dist);
            }
            return graph;
        }

        private static Graph ConstructLevelGraph(Graph g, int s, int t)
        {
            var levels = new int?[g.VerticesCount];
            levels[s] = 0;

            var tempGraph = g is AdjacencyMatrixGraph
                ? new AdjacencyListsGraph<HashTableAdjacencyList>(true, g.VerticesCount)
                : g.IsolatedVerticesGraph();
            
            bool VisitEdge(Edge e)
            {
                var fromLevel = levels[e.From];
                var targetLevel = levels[t];
                if (fromLevel.GetValueOrDefault() == targetLevel.GetValueOrDefault() &
                    fromLevel != null == (targetLevel != null))
                    return false;
                if (levels[e.To] == null) 
                    levels[e.To] = levels[e.From] + 1;
                
                var toLevel = levels[e.To];
                fromLevel = levels[e.From] + 1;
                if (toLevel.GetValueOrDefault() == fromLevel.GetValueOrDefault() &
                    toLevel != null == (fromLevel != null) && (levels[t] == null || e.To == t))
                    tempGraph.AddEdge(e.To, e.From, e.Weight);
                return true;
            }

            g.GeneralSearchFrom<EdgesQueue>(s, null, null, VisitEdge);
            
            if (levels[t] == null)
                return null;
            
            var levelGraph = tempGraph.IsolatedVerticesGraph();
            
            bool VisitEdge2(Edge e)
            {
                levelGraph.AddEdge(e.To, e.From, e.Weight);
                return true;
            }
            
            tempGraph.GeneralSearchFrom<EdgesQueue>(t, null, null, VisitEdge2);
            
            return levelGraph;
        }

        private static bool IsPath(Graph levelGraph, Graph levelGraphReversed, int from, int to)
        {
            if (from == to)
                return true;
            foreach (var edge in levelGraphReversed.OutEdges(from))
            {
                levelGraph.DelEdge(edge.To, from);
                levelGraphReversed.DelEdge(from, edge.To);
                if (levelGraph.OutDegree(edge.To) == 0 && IsPath(levelGraph, levelGraphReversed, edge.To, to))
                    return true;
            }
            return false;
        }

        private static double DFS(Graph levelGraph, Graph augmentingFlow, int from, int target, double currFlow)
        {
            if (from == target)
                return currFlow;
            var flow = 0.0;
            foreach (var edge in levelGraph.OutEdges(from))
            {
                var maxFlow = Math.Min(currFlow, edge.Weight);
                var nextFlow = DFS(levelGraph, augmentingFlow, edge.To, target, maxFlow);
                if (nextFlow != 0.0)
                {
                    flow += nextFlow;
                    if (augmentingFlow.ModifyEdgeWeight(edge.From, edge.To, nextFlow).IsNaN())
                    {
                        augmentingFlow.AddEdge(edge.From, edge.To, nextFlow);
                    }
                    if (nextFlow < maxFlow || nextFlow == edge.Weight)
                    {
                        levelGraph.DelEdge(edge);
                    }
                    else
                        levelGraph.ModifyEdgeWeight(edge.From, edge.To, -nextFlow);
                    currFlow -= nextFlow;
                    if (currFlow == 0.0)
                    {
                        break;
                    }
                    continue;
                }
                levelGraph.DelEdge(edge);
            }
            return flow;
        }
        
        /// <summary>
        /// Wyznacza maksymalny przepływ metodą przepychania wstepnego przepływu (push-relabel).
        /// </summary>
        /// <param name="g">Badany graf (sieć przepływowa)</param>
        /// <param name="source">Wierzchołek źródłowy</param>
        /// <param name="target">Wierzchołek docelowy</param>
        /// <param name="af">Parametr ignorowany</param>
        /// <param name="matrixToHashTable">Czy optymalizować sposób reprezentacji grafu rezydualnego</param>
        /// <returns>
        /// Krotka (value, flow) składająca się z wartości maksymalnego przepływu i grafu opisującego ten przepływ
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Parametr fi został wprowadzony dla zachowania zgodności
        /// sygnatur metod PushRelabelMaxFlow i <see cref="FordFulkersonDinicMaxFlow"/>.<para/>
        /// Jeśli parametr matrixToHashTable ma wartość true oraz graf g jest
        /// typu <see cref="AdjacencyMatrixGraph"/> (czyli macierz sąsiedztwa) i ma nie więcej
        /// niż 10% krawędzi, to wykorzystywany przez algorytm graf rezydualny jest
        /// typu <see cref="AdjacencyListsGraph{HashTableAdjacencyList}"/>. W przeciwnym przypadku
        /// graf rezydualny jest takiego samego typu jak graf g.<para/>
        /// Wynikowy przepływ maksymalny flow zawsze jest takiego samego typu jak graf g
        /// (niezależnie od wartości parametru matrixToHashTable).<para/>
        /// Jeśli po danej krawędzi nie płynie żaden przepływ,
        /// to nadal jest ona w wynikowym grafie flow (oczywiście z wagą 0).<para/>
        /// Algorytm nie dopuszcza krawędzi wychodzących ze żródła o wagach większych od 1E+14
        /// (zgłaszany jest wyjątek <see cref="ArgumentException"/>). Wynika to ze specyfiki algorytmu
        /// przepychania wstępnego przepływu oraz specyfiki arytmetyki maszynowej
        /// na liczbach typu double (wartość graniczna jest dobrana arbitralnie).
        /// Jeśli rzeczywiście wagi są tak duże to należy użyć metody <see cref="FordFulkersonDinicMaxFlow"/>,
        /// która nie ma takich ograniczeń (ale oczywiście podlega
        /// ograniczeniom arytmetyki na liczbach typu double).<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego lub grafu
        /// z ujemnymi wagami krawędzi zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Gdy parametry source i target są równe metoda
        /// również zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// </remarks>
        /// <seealso cref="MaxFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double value, Graph flow) PushRelabelMaxFlow(this Graph g, int source, int target, AugmentFlow af = null, bool matrixToHashTable = true)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");
            if (source == target)
                throw new ArgumentException("Source and target must be different");
            
            var heights = new int[g.VerticesCount];
            bool VisitEdge(Edge edge)
            {
                if (heights[edge.To] == 0 && edge.To != target) 
                    heights[edge.To] = heights[edge.From] + 1;
                return true;
            }

            var pushed = new double[g.VerticesCount];

            var queue = new Queue<int>();
            var flow = g.IsolatedVerticesGraph();

            var residual = matrixToHashTable
                ? g is AdjacencyMatrixGraph && 10 * g.EdgesCount < g.VerticesCount * g.VerticesCount
                    ? new AdjacencyListsGraph<HashTableAdjacencyList>(true, g.VerticesCount)
                    : g.IsolatedVerticesGraph()
                : g.IsolatedVerticesGraph();

            for (var i = 0; i < g.VerticesCount; i++)
            {
                pushed[i] = 0.0;
                foreach (var edge in g.OutEdges(i))
                {
                    if (edge.Weight < 0.0)
                        throw new ArgumentException("Negative capacity edges are not allowed");
                    if (i == source && edge.Weight > 100000000000000.0)
                        throw new ArgumentException(
                            "Capacity edges from source can't exceed 1E+14, reduce weights or use FordFulkersonDinicMaxFlow method");
                    flow.AddEdge(edge.From, edge.To, 0.0);
                    if (edge.Weight > 0.0) residual.AddEdge(edge);
                }
            }
            var residualReversed = residual.Reverse();
            residualReversed.GeneralSearchFrom<EdgesQueue>(target, null, null, VisitEdge);
            heights[source] = g.VerticesCount;
            foreach (var edge in g.OutEdges(source))
            {
                if (edge.Weight == 0.0) continue;
                flow.ModifyEdgeWeight(source, edge.To, edge.Weight);
                residual.DelEdge(source, edge.To);
                if (!residual.AddEdge(edge.To, source, edge.Weight))
                    residual.ModifyEdgeWeight(edge.To, source, edge.Weight);
                pushed[edge.To] = edge.Weight;
                if (edge.To != target) queue.Enqueue(edge.To);
            }
            while (queue.Count > 0)
            {
                var i = queue.Dequeue();
                foreach (var edge in residual.OutEdges(i))
                {
                    if (heights[i] != heights[edge.To] + 1) continue;
                    var maxPush = Math.Min(pushed[i], edge.Weight);
                    if (flow.GetEdgeWeight(edge.To, i) > 0.0)
                    {
                        var weight = flow.ModifyEdgeWeight(edge.To, edge.From, -maxPush);
                        if (weight < 0.0)
                        {
                            flow.ModifyEdgeWeight(edge.To, edge.From, -weight);
                            flow.ModifyEdgeWeight(edge.From, edge.To, -weight);
                        }
                    }
                    else
                        flow.ModifyEdgeWeight(edge.From, edge.To, maxPush);

                    double overflowCheck;
                    
                    if (edge.To != source)
                    {
                        if (edge.To != target && pushed[edge.To] == 0.0) 
                            queue.Enqueue(edge.To);
                        overflowCheck = pushed[edge.To];
                        pushed[edge.To] += maxPush;
                        if (overflowCheck == pushed[edge.To])
                            throw new ArgumentException(
                                "Overflow is too large, reduce weights or use FordFulkersonDinicMaxFlow method");
                    }
                    
                    if (residual.ModifyEdgeWeight(edge.From, edge.To, -maxPush) == 0.0) 
                        residual.DelEdge(edge);
                    if (residual.ModifyEdgeWeight(edge.To, edge.From, maxPush).IsNaN())
                        residual.AddEdge(edge.To, edge.From, maxPush);
                    
                    overflowCheck = pushed[i];
                    pushed[i] -= maxPush;
                    if (overflowCheck == pushed[i])
                        throw new ArgumentException(
                            "Overflow is too large, reduce weights or use FordFulkersonDinicMaxFlow method");
                    if (pushed[i] == 0.0)
                        break;
                }
                if (pushed[i] == 0.0)
                    continue;
                heights[i] = residual.OutEdges(i).Select(edge => heights[edge.To]).Concat(new[] {int.MaxValue}).Min() + 1;
                queue.Enqueue(i);
            }
            return (pushed[target], flow);
        }

        private class MKMHelper
        {
            internal bool[] pushed;
            private int verticesCount;
            private int source;
            private int target;
            private double[] weightsIncoming;
            private double[] weightsOutgoing;

            public MKMHelper(Graph g, int source, int target)
            {
                verticesCount = g.VerticesCount;
                this.source = source;
                this.target = target;
                weightsIncoming = new double[verticesCount];
                weightsOutgoing = new double[verticesCount];
                pushed = new bool[verticesCount];
                for (var j = 0; j < verticesCount; j++)
                {
                    foreach (var edge in g.OutEdges(j))
                    {
                        weightsIncoming[edge.From] += edge.Weight;
                        weightsOutgoing[edge.To] += edge.Weight;
                    }
                }
            }
            
            public (int index, double capacity) MinimumCapacityVertex()
            {
                var index = -1;
                var capacity = double.PositiveInfinity;
                for (var i = 0; i < verticesCount; i++)
                {
                    if (pushed[i] || i == source || i == target) continue;
                    var maxPush = Math.Min(weightsOutgoing[i], weightsIncoming[i]);
                    if (!(capacity > maxPush)) continue;
                    capacity = maxPush;
                    index = i;
                }
                return (index, capacity);
            }

            public Dictionary<int, double> Push(Graph levelGraph, Graph levelGraphReversed, Graph augmentingFlow, Dictionary<int, double> dict, bool forward, bool initial)
            {
                var dictionary = new Dictionary<int, double>();
                foreach (var num in dict.Keys)
                {
                    if (num == source || num == target || levelGraph.OutDegree(num) == 0) continue;
                    var capacity = dict[num];
                    if (forward)
                    {
                        weightsIncoming[num] -= capacity;
                    }
                    else
                    {
                        weightsOutgoing[num] -= capacity;
                    }
                    foreach (var edge in levelGraph.OutEdges(num))
                    {
                        if (pushed[edge.To]) continue;
                        var from = forward ? edge.From : edge.To;
                        var to = forward ? edge.To : edge.From;
                        var maxPush = Math.Min(edge.Weight, capacity);
                        if (maxPush > 0.0)
                        {
                            if (augmentingFlow.ModifyEdgeWeight(from, to, maxPush).IsNaN())
                                augmentingFlow.AddEdge(from, to, maxPush);
                            if (!dictionary.Keys.Contains(edge.To))
                                dictionary.Add(edge.To, maxPush);
                            else
                                dictionary[edge.To] += maxPush;
                            capacity -= maxPush;
                        }
                        if (maxPush == edge.Weight || initial)
                        {
                            levelGraph.DelEdge(edge);
                            levelGraphReversed.DelEdge(edge.To, edge.From);
                        }
                        else
                        {
                            levelGraph.ModifyEdgeWeight(edge.From, edge.To, -maxPush);
                            levelGraphReversed.ModifyEdgeWeight(edge.To, edge.From, -maxPush);
                        }

                        if (forward)
                            weightsOutgoing[edge.To] -= (initial ? edge.Weight : maxPush);
                        else
                            weightsIncoming[edge.To] -= (initial ? edge.Weight : maxPush);
                        if (capacity == 0.0 && !initial)
                            break;
                    }
                }
                return dictionary;
            }
        }
        
    }
}
