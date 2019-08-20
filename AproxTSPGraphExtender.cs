using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o rozwiązywanie problemu komiwojażera metodami przybliżonymi
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class AproxTSPGraphExtender
    {
        /// <summary>
        /// Znajduje rozwiązanie przybliżone problemu komiwojażera naiwnym algorytmem zachłannym
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <returns>Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi) znalezionego cyklu i tablicy krawędzi tworzących ten cykl)</returns>
        /// <remarks>
        /// Elementy (krawędzie) umieszczone są w tablicy cycle w kolejności swojego następstwa w znalezionym cyklu Hamiltona.<para/>
        /// Jeśli naiwny algorytm zachłanny nie znajdzie w badanym grafie cyklu Hamiltona (co oczywiście nie znaczy, że taki cykl nie istnieje) to metoda zwraca krotkę (NaN,null).<para/>
        /// Metodę można stosować dla grafów skierowanych i nieskierowanych.<para/>
        /// Metodę można stosować dla dla grafów z dowolnymi (również ujemnymi) wagami krawędzi.
        /// </remarks>
        /// <seealso cref="AproxTSPGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) SimpleGreedyTSP(this Graph g)
        {
            if (g.VerticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);
            
            var edge = new Edge(-1, -1, 0.0);
            var visited = new bool[g.VerticesCount];
            var cycle = new Edge[g.VerticesCount];
            var weight = 0.0;
            var vertex = 0;
            for (var i = 0; i < g.VerticesCount - 1;)
            {
                visited[vertex] = true;
                edge = new Edge(vertex, -1, double.PositiveInfinity);
                
                foreach (var e in g.OutEdges(vertex))
                    if (!visited[e.To] && edge.Weight > e.Weight)
                        edge = e;
                
                if (edge.To == -1)
                    return (double.NaN, null);
                
                cycle[i++] = edge;
                weight += edge.Weight;
                vertex = edge.To;
            }
            vertex = edge.To;
            edge = new Edge(vertex, 0, g.GetEdgeWeight(vertex, 0));
            if (edge.Weight.IsNaN())
                return (double.NaN, null);
            cycle[g.VerticesCount - 1] = edge;
            weight += edge.Weight;
            return (weight, cycle);
        }
        
        /// <summary>
        /// Znajduje rozwiązanie przybliżone problemu komiwojażera algorytmem zachłannym "kruskalopodobnym"
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <returns>Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi) znalezionego cyklu i tablicy krawędzi tworzących ten cykl)</returns>
        /// <remarks>
        /// Elementy (krawędzie) umieszczone są w tablicy cycle w kolejności swojego następstwa w znalezionym cyklu Hamiltona.<para/>
        /// Jeśli algorytm "kruskalopodobny" nie znajdzie w badanym grafie cyklu Hamiltona (co oczywiście nie znaczy, że taki cykl nie istnieje) to metoda zwraca krotkę (NaN,null).<para/>
        /// Metodę można stosować dla grafów skierowanych i nieskierowanych.<para/>
        /// Metodę można stosować dla dla grafów z dowolnymi (również ujemnymi) wagami krawędzi.
        /// </remarks>
        /// <seealso cref="AproxTSPGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) KruskalTSP(this Graph g)
        {
            if (g.VerticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);

            var graph = !(g is AdjacencyMatrixGraph) ? g.IsolatedVerticesGraph() : new AdjacencyListsGraph<SimpleAdjacencyList>(g.Directed, g.VerticesCount);
            var unionFind = new UnionFind(g.VerticesCount);
            var edgesMinPriorityQueue = new EdgesMinPriorityQueue();
            
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var edge in g.OutEdges(i))
                    if (g.Directed || edge.From < edge.To)
                        edgesMinPriorityQueue.Put(edge);
            
            while (graph.EdgesCount < g.VerticesCount && !edgesMinPriorityQueue.Empty)
            {
                var edge = edgesMinPriorityQueue.Get();
                if (graph.OutDegree(edge.From) >= (g.Directed ? 1 : 2) ||
                    graph.InDegree(edge.To) >= (g.Directed ? 1 : 2)) continue;
                
                if (unionFind.Union(edge.From, edge.To) || graph.EdgesCount == g.VerticesCount - 1) 
                    graph.AddEdge(edge);
            }
            
            if (graph.EdgesCount < g.VerticesCount)
                return (double.NaN, null);
            
            var cycle = new Edge[g.VerticesCount];
            var weight = 0.0;
            var prevVert = -1;
            var vert = 0;
            for (var i = 0; i < g.VerticesCount;)
            {
                foreach (var edge in graph.OutEdges(vert))
                {
                    if (edge.To == prevVert) continue;
                    cycle[i++] = edge;
                    weight += edge.Weight;
                    prevVert = vert;
                    vert = edge.To;
                    break;
                }
            }
            return (weight, cycle);
        }
        
        /// <summary>
        /// Znajduje rozwiązanie przybliżone problemu komiwojażera tworząc cykl Hamiltona na podstawie drzewa rozpinającego
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="version">Wersja algorytmu (prosta, Christofidesa lub zmodyfikowana Christofidesa)</param>
        /// <returns>Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi) znalezionego cyklu i tablicy krawędzi tworzących ten cykl)</returns>
        /// <remarks>
        /// Parametr version opisany jest w wyliczeniu <see cref="TSPTreeBasedVersion"/>.<para/>
        /// Elementy (krawędzie) umieszczone są w tablicy cycle w kolejności swojego następstwa w znalezionym cyklu Hamiltona.<para/>
        /// Jeśli algorytm bazujący na drzewie rozpinającym nie znajdzie w badanym grafie cyklu Hamiltona (co oczywiście nie znaczy,
        /// że taki cykl nie istnieje) to metoda zwraca krotkę (NaN,null).<para/>
        /// Metodę można stosować dla grafów nieskierowanych.<para/>
        /// Zastosowana do grafu skierowanego zgłasza wyjątek <see cref="ArgumentException"/>>.<para/>
        /// Dla wartości parametru <see cref="TSPTreeBasedVersion.Simple"/>) metodę można stosować dla dla grafów z dowolnymi (również ujemnymi) wagami krawędzi,
        /// dla innych wartości parametru version wagi krawędzi powinny być z przedziału &lt;float.MinValue,float.MaxValue&gt;.<para/>
        /// Dla grafu nieskierowanego spełniajacego nierówność trójkąta metoda realizuje 1.5-aproksymacyjny algorytm Christofidesa
        /// (gdy parametr version ma wartość <see cref="TSPTreeBasedVersion.Christofides"/>) lub algorytm 2-aproksymacyjny (dla innych wartości parametru version).
        /// </remarks>
        /// <seealso cref="AproxTSPGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) TreeBasedTSP(this Graph g, TSPTreeBasedVersion version = TSPTreeBasedVersion.Simple)
        {
            if (g.Directed)
                throw new ArgumentException("Directed graphs are not allowed");
            if (g.VerticesCount <= 2)
                return (double.NaN, null);
            var tree = g.Prim().mst;
            if (tree.EdgesCount != g.VerticesCount - 1)
                return (double.NaN, null);
            if (version != TSPTreeBasedVersion.Simple)
            {
                var oddDegreeVertices = new bool[g.VerticesCount];
                if (version == TSPTreeBasedVersion.Christofides)
                    for (var i = 0; i < g.VerticesCount; i++)
                        oddDegreeVertices[i] = (tree.OutDegree(i) % 2 == 1);
                else
                {
                    var count = 0;
                    for (var i = 0; i < g.VerticesCount; i++)
                    {
                        if (tree.OutDegree(i) != 1) continue;
                        oddDegreeVertices[i] = true;
                        count++;
                    }
                    if (count % 2 == 1)
                    {
                        var i = 0;
                        for (; tree.OutDegree(i) % 2 == 0 || tree.OutDegree(i) == 1; i++) ;
                        oddDegreeVertices[i] = true;
                    }
                }
                foreach (var edge in g.PerfectMatching(oddDegreeVertices))
                    tree.AddEdge(edge);
            }
            var weight = 0.0;
            var lastVert = 0;
            var cycleCounter = 0;
            var cycle = new Edge[g.VerticesCount];
            
            bool PreVisitVertex(int vert)
            {
                if (vert == 0)
                    return true;
                var w = g.GetEdgeWeight(lastVert, vert);
                cycle[cycleCounter++] = new Edge(lastVert, vert, w);
                weight += w;
                lastVert = vert;
                return true;
            }
            
            tree.GeneralSearchAll<EdgesStack>(PreVisitVertex, null, null, out _);
            var edgeWeight = g.GetEdgeWeight(lastVert, 0);
            cycle[g.VerticesCount - 1] = new Edge(lastVert, 0, edgeWeight);
            weight += edgeWeight;
            return !weight.IsNaN() ? (weight, cycle) : (double.NaN, null);
        }
        
        /// <summary>
        /// Znajduje rozwiązanie przybliżone problemu komiwojażera algorytmem przyrostowym
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="select">Metoda wyboru wstawianego wierzchołka</param>
        /// <returns>Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi) znalezionego cyklu i tablicy krawędzi tworzących ten cykl)</returns>
        /// <remarks>
        /// Znaczenia parametru select opisane jest w wyliczeniu <see cref="TSPIncludeVertexSelectionMethod"/>.<para/>
        /// Jeśli algorytm przyrostowy nie znajdzie w badanym grafie cyklu Hamiltona (co oczywiście nie znaczy,
        /// że taki cykl nie istnieje) to metoda zwraca krotkę (NaN,null).<para/>
        /// Przy wyborze wstawiania najbliższego i najdalszego wierzchołka metodę można stosować jedynie
        /// dla grafów nieskierowanych.<para/>
        /// Przy wyborze wstawiania kolejnego wierzchołka i wstawiania wierzchołka o
        /// najniższym koszcie wstawiania metodę można stosować dla grafów skierowanych i nieskierowanych.<para/>
        /// Metodę można stosować dla dla grafów z wagami krawędzi z przedziału &lt;float.MinValue,float.MaxValue&gt;.
        /// </remarks>
        /// <seealso cref="AproxTSPGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) IncludeTSP(this Graph g, TSPIncludeVertexSelectionMethod select = TSPIncludeVertexSelectionMethod.Sequential)
        {
            if (g.Directed && (select == TSPIncludeVertexSelectionMethod.Nearest || select == TSPIncludeVertexSelectionMethod.Furthest))
                throw new ArgumentException(
                    "Directed graphs are not allowed for Nearest and Furthest selection methods");
            if (g.VerticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);
            
            var solution = new LinkedList<Edge>();
            var available = new HashSet<int>();
            PriorityQueue<int, double> nearest = null;
            double[] furthest = null;
            solution.AddFirst(new Edge(0, 0, ClampDoubleToFloat(double.NaN)));
            for (var i = 1; i < g.VerticesCount; i++) available.Add(i);
            
            switch (select)
            {
                case TSPIncludeVertexSelectionMethod.Nearest:
                    bool MinValueOrKey(KeyValuePair<int, double> kvp1, KeyValuePair<int, double> kvp2)
                    {
                        if (kvp1.Value != kvp2.Value)
                            return kvp1.Value < kvp2.Value;
                        return kvp1.Key < kvp2.Key;
                    }
                    nearest = new PriorityQueue<int, double>(MinValueOrKey, CMonDoSomething.Nothing);
                    for (var i = 1; i < g.VerticesCount; i++)
                        nearest.Put(i, ClampDoubleToFloat(double.PositiveInfinity));
                    foreach (var edge in g.OutEdges(0))
                        nearest.ImprovePriority(edge.To, ClampDoubleToFloat(edge.Weight));
                    break;
                case TSPIncludeVertexSelectionMethod.Furthest:
                    furthest = new double[g.VerticesCount];
                    for (var i = 1; i < g.VerticesCount; i++)
                        furthest[i] = ClampDoubleToFloat(double.PositiveInfinity);
                    foreach (var edge in g.OutEdges(0))
                        furthest[edge.To] = ClampDoubleToFloat(edge.Weight);
                    break;
                case TSPIncludeVertexSelectionMethod.Sequential:
                    break;
                case TSPIncludeVertexSelectionMethod.MinimalCost:
                    break;
                default:
                    throw new ArgumentException("Invalid vertex selection method");
            }

            for (var i = 1; i < g.VerticesCount; i++)
            {
                LinkedListNode<Edge> edgeNode = null;
                int currVert;
                switch (select)
                {
                    case TSPIncludeVertexSelectionMethod.Sequential:
                        currVert = i;
                        break;
                    case TSPIncludeVertexSelectionMethod.Nearest:
                        currVert = nearest.Get();
                        foreach (var edge in g.OutEdges(currVert))
                            if (available.Contains(edge.To))
                                nearest.ImprovePriority(edge.To, ClampDoubleToFloat(edge.Weight));
                        break;
                    case TSPIncludeVertexSelectionMethod.Furthest:
                        var max = double.NegativeInfinity;
                        currVert = available.First();
                        foreach (var vert in available)
                            if (max < furthest[vert])
                            {
                                currVert = vert;
                                max = furthest[vert];
                            }
                        foreach (var edge in g.OutEdges(currVert))
                        {
                            var w = ClampDoubleToFloat(edge.Weight);
                            if (furthest[edge.To] > w) furthest[edge.To] = w;
                        }
                        break;
                    case TSPIncludeVertexSelectionMethod.MinimalCost:
                        var min = double.PositiveInfinity;
                        edgeNode = solution.First;
                        currVert = available.First();
                        foreach (var vert in available)
                            for (var k = solution.First; k != null; k = k.Next)
                            {
                                var dif = ClampDoubleToFloat(g.GetEdgeWeight(k.Value.From, vert)) +
                                        ClampDoubleToFloat(g.GetEdgeWeight(vert, k.Value.To)) -
                                        ClampDoubleToFloat(k.Value.Weight);
                                if (!(min > dif)) continue;
                                edgeNode = k;
                                currVert = vert;
                                min = dif;
                            }
                        break;
                    default:
                        throw new ArgumentException("Invalid vertex selection method");
                }
                
                available.Remove(currVert);
                if (edgeNode == null)
                {
                    var min = double.PositiveInfinity;
                    edgeNode = solution.First;
                    for (var j = solution.First; j != null; j = j.Next)
                    {
                        var diff = ClampDoubleToFloat(g.GetEdgeWeight(j.Value.From, currVert)) + 
                                   ClampDoubleToFloat(g.GetEdgeWeight(currVert, j.Value.To)) - 
                                   ClampDoubleToFloat(j.Value.Weight);
                        if (!(min > diff)) continue;
                        edgeNode = j;
                        min = diff;
                    }
                }
                solution.AddBefore(edgeNode, new Edge(edgeNode.Value.From, currVert, g.GetEdgeWeight(edgeNode.Value.From, currVert)));
                solution.AddBefore(edgeNode, new Edge(currVert, edgeNode.Value.To, g.GetEdgeWeight(currVert, edgeNode.Value.To)));
                solution.Remove(edgeNode);
            }
            var cycle = solution.ToArray();
            var weight = 0.0;
            for (var i = 0; i < cycle.Length; i++) 
                weight += cycle[i].Weight;
            return !weight.IsNaN() ? (weight, cycle) : (double.NaN, null);
        }
        
        /// <summary>
        /// Znajduje rozwiązanie przybliżone problemu komiwojażera algorytmem 3-optymalnym
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="init">Cykl początkowy</param>
        /// <returns>Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi) znalezionego cyklu i tablicy krawędzi tworzących ten cykl)</returns>
        /// <remarks>
        /// Elementy (krawędzie) umieszczone są w tablicy cycle w kolejności swojego następstwa w znalezionym cyklu Hamiltona.<para/>
        /// Jeśli algorytm 3-optymalny nie znajdzie w badanym grafie cyklu Hamiltona (co oczywiście nie znaczy, że taki cykl nie istnieje) to metoda zwraca krotkę (NaN,null).<para/>
        /// Metodę można stosować dla grafów skierowanych i nieskierowanych.<para/>
        /// Metodę można stosować dla dla grafów z wagami krawędzi z przedziału &lt;float.MinValue,float.MaxValue&gt;.<para/>
        /// Metoda sprawdza poprawność cyklu początkowego (dopuszcza sztuczne krawędzie),
        /// nie jest on poprawny lub nie został podany (ma wartość domyślną null),
        /// to jako cykl początkowy przyjmuje cykl przechodzący przez wierzchołki w kolejności ich numeracji
        /// (jeśli jakaś krawędź wchodząca w skład takiego "cyklu" nie istnieje przyjmowana
        /// jest sztuczna krawędź z wagą <see cref="float.MaxValue"/>).<para/>
        /// Element init[i] zawiera numer wierzchołka do którego przechodzimy z wierzchołka i-tego.
        /// </remarks>
        /// <seealso cref="AproxTSPGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) ThreeOptTSP(this Graph g, int[] init = null)
        {
            if (g.VerticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);
            if (g.VerticesCount == 2)
            {
                var weight = g.GetEdgeWeight(0, 1) + g.GetEdgeWeight(1, 0);
                if (weight.IsNaN()) return (double.NaN, null);
                Edge[] c = {
                    g.OutEdges(0).First(),
                    g.OutEdges(1).First()
                };
                return (weight, c);
            }
            else
            {
                var useInit = false;
                var weights = new double[g.VerticesCount];
                if (init != null)
                {
                    if (init.Length == g.VerticesCount && init[0] > 0 && init[0] < g.VerticesCount)
                    {
                        var visited = new bool[g.VerticesCount];
                        int vertCount;
                        var i = init[0];
                        for (vertCount = 2; vertCount < g.VerticesCount; vertCount++, i = init[i], visited[i] = true)
                            if (init[i] <= 0 || init[i] >= g.VerticesCount || init[i] == i || visited[i])
                                break;
                        if (vertCount == g.VerticesCount && init[i] == 0) useInit = true;
                    }
                }
                int[] tempCycle;
                if (useInit)
                    tempCycle = (int[]) init.Clone();
                else
                {
                    tempCycle = new int[g.VerticesCount];
                    for (var i = 1; i < g.VerticesCount; i++) tempCycle[i - 1] = i;
                    tempCycle[g.VerticesCount - 1] = 0;
                }
                var newK = -1;
                var newJ = -1;
                var newI = -1;
                double weight;
                while (true)
                {
                    var maxDifference = 0.0;
                    for (var i = 0; i < g.VerticesCount; i++) weights[i] = ClampDoubleToFloat(g.GetEdgeWeight(i, tempCycle[i]));
                    for (var i = 0; i < g.VerticesCount; i++)
                    {
                        if (tempCycle[i] == 0) continue;
                        for (var j = tempCycle[i]; 0 != tempCycle[j]; j = tempCycle[j])
                        {
                            weight = ClampDoubleToFloat(g.GetEdgeWeight(i, tempCycle[j]));
                            for (var k = tempCycle[j]; k != 0; k = tempCycle[k])
                            {
                                var difference = weights[i] + weights[j] + weights[k] - weight - ClampDoubleToFloat(g.GetEdgeWeight(j, tempCycle[k])) - ClampDoubleToFloat(g.GetEdgeWeight(k, tempCycle[i]));
                                if (!(maxDifference < difference)) continue;
                                maxDifference = difference;
                                newI = i;
                                newJ = j;
                                newK = k;
                            }
                        }
                    }
                    if (maxDifference == 0.0)
                        break;
                    var temp = tempCycle[newI];
                    tempCycle[newI] = tempCycle[newJ];
                    tempCycle[newJ] = tempCycle[newK];
                    tempCycle[newK] = temp;
                }
                var cycle = new Edge[g.VerticesCount];
                {
                    int i;
                    int j;
                    for (i = 0, j = 0, weight = 0; i < g.VerticesCount; i++, j = tempCycle[j])
                    {
                        cycle[i] = new Edge(j, tempCycle[j], g.GetEdgeWeight(j, tempCycle[j]));
                        weight += cycle[i].Weight;
                    }
                }
                return weight.IsNaN() ? (double.NaN, null) : (weight, cycle);
            }
        }
        
        /// <summary>
        /// Znajduje rozwiązanie przybliżone problemu komiwojażera algorytmem 3-optymalnym
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="init">Cykl początkowy</param>
        /// <returns>Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi) znalezionego cyklu i tablicy krawędzi tworzących ten cykl)</returns>
        /// <remarks>
        /// Elementy (krawędzie) umieszczone są w tablicy cycle w kolejności swojego następstwa w znalezionym cyklu Hamiltona.<para/>
        /// Jeśli algorytm 3-optymalny nie znajdzie w badanym grafie cyklu Hamiltona (co oczywiście nie znaczy, że taki cykl nie istnieje) to metoda zwraca krotkę (NaN,null).<para/>
        /// Metodę można stosować dla grafów skierowanych i nieskierowanych.<para/>
        /// Metodę można stosować dla dla grafów z wagami krawędzi z przedziału &lt;float.MinValue,float.MaxValue&gt;.<para/>
        /// Metoda sprawdza poprawność cyklu początkowego (dopuszcza sztuczne krawędzie),
        /// nie jest on poprawny lub nie został podany (ma wartość domyślną null),
        /// to jako cykl początkowy przyjmuje cykl przechodzący przez wierzchołki w kolejności ich numeracji
        /// (jeśli jakaś krawędź wchodząca w skład takiego "cyklu" nie istnieje przyjmowana
        /// jest sztuczna krawędź z wagą <see cref="float.MaxValue"/>).<para/>
        /// Element init[i] zawiera numer wierzchołka do którego przechodzimy z wierzchołka i-tego.<para/>
        /// Metoda wykonuje obliczenia równolegle w wielu wątkach.
        /// </remarks>
        /// <seealso cref="AproxTSPGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) ThreeOptTSPParallel(this Graph g, int[] init = null)
        {
            if (g.VerticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);
            double weight;
            if (g.VerticesCount == 2)
            {
                weight = g.GetEdgeWeight(0, 1) + g.GetEdgeWeight(1, 0);
                if (weight.IsNaN()) return (double.NaN, null);
                var c = new Edge[2];
                c[0] = g.OutEdges(0).First();
                c[1] = g.OutEdges(1).First();
                return (weight, c);
            }
            
            int[] tempCycle;
            var useInit = false;
            var weights = new double[g.VerticesCount];
            var newJ = new int[g.VerticesCount];
            var newK = new int[g.VerticesCount];
            var differences = new double[g.VerticesCount];
            
            void Loop(int i)
            {
                differences[i] = 0.0;
                if (tempCycle[i] == 0)
                    return;
                for (var j = tempCycle[i]; tempCycle[j] != 0; j = tempCycle[j])
                {
                    var w = ClampDoubleToFloat(g.GetEdgeWeight(i, tempCycle[j]));
                    for (var k = tempCycle[j]; k != 0; k = tempCycle[k])
                    {
                        var difference = weights[i] + weights[j] + weights[k] - w - ClampDoubleToFloat(g.GetEdgeWeight(j, tempCycle[k])) - ClampDoubleToFloat(g.GetEdgeWeight(k, tempCycle[i]));
                        if (!(differences[i] < difference)) continue;
                        differences[i] = difference;
                        newJ[i] = j;
                        newK[i] = k;
                    }
                }
            }

            if (init != null && init.Length == g.VerticesCount && init[0] > 0 && init[0] < g.VerticesCount)
            {
                var visited = new bool[g.VerticesCount];
                int vertCount;
                var i = init[0];
                for (vertCount = 2; vertCount < g.VerticesCount; vertCount++, i = init[i], visited[i] = true)
                    if (init[i] <= 0 || init[i] >= g.VerticesCount || init[i] == i || visited[i])
                        break;
                if (vertCount == g.VerticesCount && init[i] == 0) useInit = true;
            }

            if (useInit)
                tempCycle = (int[]) init.Clone();
            else
            {
                tempCycle = new int[g.VerticesCount];
                for (var i = 1; i < g.VerticesCount; i++)
                {
                    tempCycle[i - 1] = i;
                }
                tempCycle[g.VerticesCount - 1] = 0;
            }

            while(true)
            {
                for (var i = 0; i < g.VerticesCount; i++) weights[i] = ClampDoubleToFloat(g.GetEdgeWeight(i, tempCycle[i]));
                Parallel.For(0, g.VerticesCount, Loop);
                var maxDifference = 0.0;
                var newI = -1;
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    if (!(maxDifference < differences[i])) continue;
                    maxDifference = differences[i];
                    newI = i;
                }
                if (maxDifference == 0.0)
                    break;
                var temp = tempCycle[newI];
                tempCycle[newI] = tempCycle[newJ[newI]];
                tempCycle[newJ[newI]] = tempCycle[newK[newI]];
                tempCycle[newK[newI]] = temp;
            }
            var cycle = new Edge[g.VerticesCount];
            weight = 0.0;
            {
                int i;
                int j;
                for (i = 0, j = 0; j < g.VerticesCount; j++, i = tempCycle[i])
                {
                    cycle[j] = new Edge(i, tempCycle[i], g.GetEdgeWeight(i, tempCycle[i]));
                    weight += cycle[j].Weight;
                }
            }
            
            return weight.IsNaN() ? (double.NaN, null) : (weight, cycle);
        }

        private static IEnumerable<Edge> PerfectMatching(this Graph g, IReadOnlyList<bool> oddVertices)
        {
            var array = new int[g.VerticesCount];
            var oddVerticesCount = g.VerticesCount;
            for (var i = 0; i < g.VerticesCount; i++)
                if (!oddVertices[i])
                    oddVerticesCount--;
            if (oddVerticesCount % 2 != 0)
                throw new ArgumentException("Invalid arguments in Matching");
            
            for (var n = -1; ; )
            {
                var m = n + 1;
                while (m < g.VerticesCount && !oddVertices[m]) 
                    m++;
                if (m == g.VerticesCount)
                    break;
                n = m + 1;
                while (n < g.VerticesCount && !oddVertices[n]) n++;
                array[m] = n;
                array[n] = m;
                var xD = -1;
                var xDD = -1;
                var flag1 = false;
                var flag2 = false;
                var floatWeight = ClampDoubleToFloat(g.GetEdgeWeight(m, n));
                
                for (var i = 0; i < m; i++)
                {
                    if (!oddVertices[i]) continue;
                    var num11 = ClampDoubleToFloat(g.GetEdgeWeight(i, m)) + ClampDoubleToFloat(g.GetEdgeWeight(array[i], n)) - ClampDoubleToFloat(g.GetEdgeWeight(i, array[i]));
                    if (!(floatWeight > num11)) continue;
                    floatWeight = num11;
                    xDD = i;
                    flag2 = true;
                }
                for (var i = 0; i < m; i++)
                {
                    if (!oddVertices[i]) continue;
                    for (var j = 0; j < m; j++)
                    {
                        if (!oddVertices[j] || i == j || i == array[j] || j == array[i]) continue;
                        
                        var num11 = ClampDoubleToFloat(g.GetEdgeWeight(i, m)) + ClampDoubleToFloat(g.GetEdgeWeight(j, n)) + ClampDoubleToFloat(g.GetEdgeWeight(array[i], array[j])) - ClampDoubleToFloat(g.GetEdgeWeight(i, array[i])) - ClampDoubleToFloat(g.GetEdgeWeight(j, array[j]));
                        if (!(floatWeight > num11)) continue;
                        floatWeight = num11;
                        xDD = i;
                        xD = j;
                        flag1 = true;
                    }
                }
                if (flag1)
                {
                    var x = array[xDD];
                    var d = array[xD];
                    array[xDD] = m;
                    array[m] = xDD;
                    array[xD] = n;
                    array[n] = xD;
                    array[x] = d;
                    array[d] = x;
                }
                else if (flag2)
                {
                    array[n] = array[xDD];
                    array[array[xDD]] = n;
                    array[m] = xDD;
                    array[xDD] = m;
                }
            }
            var matching = new Edge[oddVerticesCount / 2];
            for (int i = 0, j = 0; i < g.VerticesCount; i++)
            {
                if (i < array[i])
                {
                    matching[j++] = new Edge(i, array[i], ClampDoubleToFloat(g.GetEdgeWeight(i, array[i])));
                }
            }
            return matching;
        }

        private static double ClampDoubleToFloat(double d)
        {
            if (d.IsNaN() || d > 3.4028234663852886E+38)
            {
                d = 3.4028234663852886E+38;
            }
            return Math.Max(d, -3.4028234663852886E+38);
        }
        
    }
}
