using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o algorytmy wyznaczania najkrótszych ścieżek
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class ShortestPathsGraphExtender
    {
        /// <summary>
        /// Wyznacza najkrótsze ścieżki algorytmem Forda-Bellmana
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>Informacja czy graf spełnia założenia algorytmu Forda-Bellmana</returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu nieskierowanego</exception>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości od źródła do wierzchołka określonego przez indeks elementu.<para/>
        /// Jeśli ścieżka od źródła do danego wierzchołka nie istnieje, to odległość ma wartość NaN.<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Jeśli badany graf nie spełnia założeń algorytmu Forda-Bellmana, to metoda zwraca false.
        /// Parametr d zawiera wówczas informacje umożliwiające wyznaczenie cyklu o ujemnej długości.<para/>
        /// Założenia badane są jedynie częściowo, w zakresie mogącym wpłynąć na działanie algorytmu
        /// (na przykład dla grafu niespójnego cykle o ujemnej długości w innej składowej spójnej
        /// niż źródło nie zostaną wykryte - metoda zwróci true).
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool FordBellmanShortestPaths(this ASD.Graphs.Graph g, int s, out PathsInfo[] d)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");
            
            var array = new bool[g.VerticesCount];
            
            d = new PathsInfo[g.VerticesCount];
            for (var i = 0; i < g.VerticesCount; i++)
                d[i].Dist = double.NaN;
            d[s].Dist = 0.0;
            
            array[s] = true;
            var rotations = 0;
            var change = g.VerticesCount > 1;
            while (change && rotations++ != g.VerticesCount)
            {
                change = false;
                for (var j = 0; j < g.VerticesCount; j++)
                {
                    if (!array[j]) continue;
                    array[j] = false;
                    foreach (var edge in g.OutEdges(j))
                    {
                        if (!d[edge.To].Dist.IsNaN() && d[edge.To].Dist <= d[edge.From].Dist + edge.Weight) 
                            continue;
                        
                        change = true;
                        array[edge.To] = true;
                        d[edge.To].Dist = d[edge.From].Dist + edge.Weight;
                        d[edge.To].Last = edge;
                    }
                }
            }
            return !change;
        }
        
        /// <summary>
        /// Wyznacza najkrótsze ścieżki algorytmem Forda-Bellmana
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>Informacja czy graf spełnia założenia algorytmu Forda-Bellmana</returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu nieskierowanego</exception>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości od źródła do wierzchołka określonego przez indeks elementu.<para/>
        /// Jeśli ścieżka od źródła do danego wierzchołka nie istnieje, to odległość ma wartość NaN.<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Jeśli badany graf nie spełnia założeń algorytmu Forda-Bellmana, to metoda zwraca false.
        /// Parametr d zawiera wówczas informacje umożliwiające wyznaczenie cyklu o ujemnej długości.<para/>
        /// Założenia badane są jedynie częściowo, w zakresie mogącym wpłynąć na działanie algorytmu
        /// (na przykład dla grafu niespójnego cykle o ujemnej długości w innej składowej spójnej
        /// niż źródło nie zostaną wykryte - metoda zwróci true).<para/>
        /// Metoda wykonuje obliczenia równolegle w wielu wątkach.
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool FordBellmanShortestPathsParallel(this ASD.Graphs.Graph g, int s, out PathsInfo[] d)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");

            var change = g.VerticesCount > 1;
            
            var array1 = new bool[g.VerticesCount];
            var array2 = new bool[g.VerticesCount];
            var mutex = new object[g.VerticesCount];
            
            var dTemp = new PathsInfo[g.VerticesCount];
            
            for (var i = 0; i < g.VerticesCount; i++)
                dTemp[i].Dist = double.NaN;
            dTemp[s].Dist = 0.0;
            
            array2[s] = true;
            
            var rotations = 0;
            for (var i = 0; i < g.VerticesCount; i++)
                mutex[i] = new object();
            
            void Action(int vert)
            {
                if (!array2[vert]) return;
                array2[vert] = false;
                foreach (var edge in g.OutEdges(vert))
                {
                    if (!dTemp[edge.To].Dist.IsNaN() && !(dTemp[edge.To].Dist > dTemp[edge.From].Dist + edge.Weight)) continue;
                    lock (mutex[edge.To])
                    {
                        if (!dTemp[edge.To].Dist.IsNaN() && !(dTemp[edge.To].Dist > dTemp[edge.From].Dist + edge.Weight)) continue;
                        change = true;
                        array1[edge.To] = true;
                        dTemp[edge.To].Dist = dTemp[vert].Dist + edge.Weight;
                        dTemp[edge.To].Last = edge;
                    }
                }
            }
            
            while (change && rotations++ != g.VerticesCount)
            {
                change = false;
                Parallel.For(0, g.VerticesCount, Action);
                var temp = array2;
                array2 = array1;
                array1 = temp;
            }
            d = dTemp;
            return !change;
        }

        /// <summary>
        /// Znajduje cykl o ujemnej długości (wadze)
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="d">Informacje o najkrótszych ścieżkach</param>
        /// <returns>
        /// Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi)
        /// znalezionego cyklu i tablicy krawędzi tworzących ten cykl)
        /// </returns>
        /// <remarks>
        /// Elementy tablicy d powinny zawierać odległości wyznaczone algorytmem Forda-Bellmana,
        /// który zatrzymał się z wynikiem false.<para/>
        /// Jeśli analiza tablicy d nie wykryła cyklu o ujemnej długości metoda zwraca (0,null).<para/>
        /// Nie oznacza to, że w grafie nie ma żadnego cyklu o ujemnej dlugości, a jedynie że nie ma takiego cyklu,
        /// który zakłóciłby działanie uruchomionego wcześciej algorytmu Forda-Bellmana (dla danego źródła).<para/>
        /// Elementy (krawędzie) umieszczone są w zwracanej tablicy w kolejności swojego następstwa w znalezionym cyklu.
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) FindNegativeCostCycle(this ASD.Graphs.Graph g, PathsInfo[] d)
        {
            Edge? edge = null;
            var vert = 0;
            while (edge == null && vert < g.VerticesCount)
            {
                if (!d[vert].Dist.IsNaN())
                {
                    foreach (var e in g.OutEdges(vert))
                    {
                        if (!d[e.To].Dist.IsNaN() && !(d[e.To].Dist > d[vert].Dist + e.Weight))
                            continue;
                        edge = e;
                        break;
                    }
                }
                vert++;
            }
            if (edge == null) return (0.0, null);

            var hashSet = new HashSet<int> {edge.Value.To};
            var from = d[edge.Value.To].Last.Value.From;
            while (!hashSet.Contains(from))
            {
                hashSet.Add(from);
                from = d[from].Last.Value.From;
            }
            var start = from;
            var weight = 0.0;
            var edgesStack = new EdgesStack();
            do
            {
                edge = d[from].Last;
                edgesStack.Put(edge.Value);
                weight += edge.Value.Weight;
                from = edge.Value.From;
            }
            while (from != start);
            return (weight, edgesStack.ToArray());
        }
        
        /// <summary>
        /// Wyznacza najkrótsze ścieżki algorytmem Dijkstry
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>Informacja czy graf spełnia założenia algorytmu Dijkstry</returns>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości od źródła do wierzchołka określonego przez indeks elementu.<para/>
        /// Jeśli ścieżka od źródła do danego wierzchołka nie istnieje, to odległość ma wartość NaN.<para/>
        /// Jeśli badany graf nie spełnia założeń algorytmu Dijkstry, to metoda zwraca false.
        /// Parametr d jest wówczas równy null.<para/>
        /// Założenia badane są jedynie częściowo, w zakresie mogącym wpłynąć na działanie algorytmu
        /// (na przykład dla grafu niespójnego krawędzie o ujemnych wagach w innej składowej spójnej
        /// niż źródło nie zostaną wykryte - metoda zwróci true).<para/>
        /// Zaimplementowana jest wersja alorytmu Dijkstry korzystająca z kolejki priorytetowej.
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool DijkstraShortestPaths(this ASD.Graphs.Graph g, int s, out PathsInfo[] d)
        {
            bool Cmp(KeyValuePair<int, double> kvp1, KeyValuePair<int, double> kvp2)
            {
                return kvp1.Value != kvp2.Value ? kvp1.Value < kvp2.Value : kvp1.Key < kvp2.Key;
            }
            var priorityQueue = new PriorityQueue<int, double>(Cmp, CMonDoSomething.Nothing);
            
            d = new PathsInfo[g.VerticesCount];
            for (var i = 0; i < g.VerticesCount; i++)
                d[i].Dist = double.NaN;
            d[s].Dist = 0.0;
            
            for (var j = 0; j < g.VerticesCount; j++)
                priorityQueue.Put(j, (j != s) ? double.PositiveInfinity : 0.0);
            
            while (!priorityQueue.Empty)
            {
                var vert = priorityQueue.Get();
                if (d[vert].Dist.IsNaN()) return true;
                foreach (var edge in g.OutEdges(vert))
                {
                    if (edge.Weight < 0.0)
                    {
                        d = null;
                        return false;
                    }

                    if (!priorityQueue.Contains(edge.To) ||
                        !priorityQueue.ImprovePriority(edge.To, d[vert].Dist + edge.Weight)) continue;
                    d[edge.To].Dist = d[vert].Dist + edge.Weight;
                    d[edge.To].Last = edge;
                }
            }
            return true;
        }

        /// <summary>
        /// Wyznacza najkrótsze ścieżki w grafie skierowanym bez cykli (DAG)
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>Informacja czy graf spełnia założenia algorytmu</returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu nieskierowanego</exception>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości od źródła do wierzchołka określonego przez indeks elementu.<para/>
        /// Jeśli ścieżka od źródła do danego wierzchołka nie istnieje, to odległość ma wartość NaN.<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Jeśli badany graf jest skierowany, ale zawiera cykl, to metoda zwraca false.
        /// Parametr d jest wówczas równy null.<para/>
        /// Wykrywane są jedynie cykle osiągalne ze źródła (tylko takie mogą wpłynąć na działanie algorytmu),
        /// cykle nieosiągalne nie zostaną wykryte - metoda zwróci true.
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool DAGShortestPaths(this ASD.Graphs.Graph g, int s, out PathsInfo[] d)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");

            if (!g.TopologicalSort(out var original2topological, out var topological2original))
            {
                d = null;
                return false;
            }
            d = new PathsInfo[g.VerticesCount];
            for (var i = 0; i < g.VerticesCount; i++)
                d[i].Dist = double.NaN;
            d[s].Dist = 0.0;
            
            for (var j = original2topological[s]; j < g.VerticesCount; j++)
            {
                if (d[topological2original[j]].Dist.IsNaN()) continue;
                foreach (var edge in g.OutEdges(topological2original[j]))
                {
                    if (!d[edge.To].Dist.IsNaN() && !(d[edge.To].Dist > d[edge.From].Dist + edge.Weight)) continue;
                    d[edge.To].Dist = d[edge.From].Dist + edge.Weight;
                    d[edge.To].Last = edge;
                }
            }
            return true;
        }

        /// <summary>
        /// Wyznacza ścieżki składające się z najmniejszej liczby krawędzi
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>true</returns>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości od źródła do wierzchołka określonego przez indeks elementu.<para/>
        /// Jeśli ścieżka od źródła do danego wierzchołka nie istnieje, to odległość ma wartość NaN. <para/>
        /// Metoda zawsze zwraca true (można ją stosować do każdego grafu).
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool BFPaths(this ASD.Graphs.Graph g, int s, out PathsInfo[] d)
        {
            var dTemp = new PathsInfo[g.VerticesCount];
            for (var i = 0; i < g.VerticesCount; i++)
                dTemp[i].Dist = double.NaN;
            dTemp[s].Dist = 0.0;
            
            bool VisitEdge(Edge edge)
            {
                if (!dTemp[edge.To].Dist.IsNaN()) return true;
                dTemp[edge.To].Dist = dTemp[edge.From].Dist + 1.0;
                dTemp[edge.To].Last = edge;
                return true;
            }

            g.GeneralSearchFrom<EdgesQueue>(s, null, null, VisitEdge);
            d = dTemp;
            return true;
        }

        /// <summary>
        /// Wyznacza najkrótsze ścieżki algorytmem Floyda-Warshalla
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>Informacja czy graf nie zawiera cyklu o ujemnej długości</returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu nieskierowanego</exception>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości pomiedzy każdą parą wierzchołków grafu.<para/>
        /// Jeśli dla danej pary wierzchołków odpowiednia ścieżka nie istnieje, to odległość ma wartość NaN.<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Jeśli badany graf zawiera cykl o ujemnej długości, to metoda zwraca false.
        /// Parametr d jest wówczas równy null.
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool FloydWarshallShortestPaths(this ASD.Graphs.Graph g, out PathsInfo[,] d)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");
            
            d = new PathsInfo[g.VerticesCount, g.VerticesCount];
            for (var i = 0; i < g.VerticesCount; i++)
            {
                for (var j = 0; j < g.VerticesCount; j++)
                    d[i, j].Dist = double.NaN;
                
                foreach (var edge in g.OutEdges(i))
                {
                    d[i, edge.To].Dist = edge.Weight;
                    d[i, edge.To].Last = edge;
                }
                
                if (d[i, i].Dist.IsNaN() || d[i, i].Dist >= 0.0)
                {
                    d[i, i].Dist = 0.0;
                    d[i, i].Last = null;
                }
                
                if (!(d[i, i].Dist < 0.0)) continue;
                d = null;
                return false;
            }
            
            for (var k = 0; k < g.VerticesCount; k++)
            {
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    for (var j = 0; j < g.VerticesCount;j++) 
                    {
                        if (!d[i, j].Dist.IsNaN() && !(d[i, j].Dist > d[i, k].Dist + d[k, j].Dist)) continue;
                        d[i, j].Dist = d[i, k].Dist + d[k, j].Dist;
                        d[i, j].Last = d[k, j].Last;
                        if (i != j || d[i, j].Dist >= 0.0)
                            continue;
                        d = null;
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Wyznacza najkrótsze ścieżki algorytmem Floyda-Warshalla
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>Informacja czy graf nie zawiera cyklu o ujemnej długości</returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu nieskierowanego</exception>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości pomiedzy każdą parą wierzchołków grafu.<para/>
        /// Jeśli dla danej pary wierzchołków odpowiednia ścieżka nie istnieje, to odległość ma wartość NaN.<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Jeśli badany graf zawiera cykl o ujemnej długości, to metoda zwraca false.
        /// Parametr d jest wówczas równy null.<para/>
        /// Metoda wykonuje obliczenia równolegle w wielu wątkach.
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool FloydWarshallShortestPathsParallel(this ASD.Graphs.Graph g, out PathsInfo[,] d)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");

            var dTemp = new PathsInfo[g.VerticesCount, g.VerticesCount];
            
            void Init(int i, ParallelLoopState pls)
            {
                for (var j = 0; j < g.VerticesCount; j++) 
                    dTemp[i, j].Dist = double.NaN;
                
                foreach (var edge in g.OutEdges(i))
                {
                    dTemp[i, edge.To].Dist = edge.Weight;
                    dTemp[i, edge.To].Last = edge;
                }
                if (dTemp[i, i].Dist.IsNaN() || dTemp[i, i].Dist >= 0.0)
                {
                    dTemp[i, i].Dist = 0.0;
                    dTemp[i, i].Last = null;
                }
                if (dTemp[i, i].Dist < 0.0) pls.Stop();
            }
            
            var parallelLoopResult = Parallel.For(0, g.VerticesCount, Init);
            var k = 0;
            
            void Action(int i, ParallelLoopState pls)
            {
                for (var j = 0; j < g.VerticesCount; j++)
                {
                    if (!dTemp[i, j].Dist.IsNaN() && !(dTemp[i, j].Dist > dTemp[i, k].Dist + dTemp[k, j].Dist))
                        continue;
                    dTemp[i, j].Dist = dTemp[i, k].Dist + dTemp[k, j].Dist;
                    dTemp[i, j].Last = dTemp[k, j].Last;
                    if (i == j && dTemp[i, j].Dist < 0.0) 
                        pls.Stop();
                }
            }

            while (k < g.VerticesCount)
            {
                if (!parallelLoopResult.IsCompleted)
                    break;
                parallelLoopResult = Parallel.For(0, g.VerticesCount, Action);
                k++;
            }
            d = parallelLoopResult.IsCompleted ? dTemp : null;
            return parallelLoopResult.IsCompleted;
        }

        /// <summary>
        /// Wyznacza najkrótsze ścieżki algorytmem Johnsona
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>Informacja czy graf nie zawiera cyklu o ujemnej długości</returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu nieskierowanego</exception>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości pomiedzy każdą parą wierzchołków grafu.<para/>
        /// Jeśli dla danej pary wierzchołków odpowiednia ścieżka nie istnieje, to odległość ma wartość NaN.<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Jeśli badany graf zawiera cykl o ujemnej długości, to metoda zwraca false.
        /// Parametr d jest wówczas równy null.
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool JohnsonShortestPaths(this ASD.Graphs.Graph g, out PathsInfo[,] d)
        {
            d = null;
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");
            
            var graph = g.IsolatedVerticesGraph(true, g.VerticesCount + 1);
            for (var i = 0; i < g.VerticesCount; i++)
            {
                graph.AddEdge(g.VerticesCount, i, 0.0);
                foreach (var e in g.OutEdges(i)) graph.AddEdge(e);
            }

            if (!FordBellmanShortestPaths(graph, g.VerticesCount, out var dFordBellman))
                return false;

            graph = g.IsolatedVerticesGraph();
            
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var edge in g.OutEdges(i))
                    graph.AddEdge(edge.From, edge.To, edge.Weight + dFordBellman[edge.From].Dist - dFordBellman[edge.To].Dist);

            d = new PathsInfo[g.VerticesCount, g.VerticesCount];
            for (var i = 0; i < g.VerticesCount; i++)
            {
                graph.DijkstraShortestPaths(i, out var dDijkstra);
                for (var j = 0; j < g.VerticesCount; j++)
                {
                    d[i, j].Dist = dDijkstra[j].Dist + dFordBellman[j].Dist - dFordBellman[i].Dist;
                    d[i, j].Last = dDijkstra[j].Last;
                }
            }
            return true;
        }

        /// <summary>
        /// Wyznacza najkrótsze ścieżki algorytmem Johnsona
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="d">Znalezione najkrótsze ścieżki (parametr wyjściowy)</param>
        /// <returns>Informacja czy graf nie zawiera cyklu o ujemnej długości</returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu nieskierowanego</exception>
        /// <remarks>
        /// Elementy tablicy d zawierają odległości pomiedzy każdą parą wierzchołków grafu.<para/>
        /// Jeśli dla danej pary wierzchołków odpowiednia ścieżka nie istnieje, to odległość ma wartość NaN.<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Jeśli badany graf zawiera cykl o ujemnej długości, to metoda zwraca false.
        /// Parametr d jest wówczas równy null.<para/>
        /// Metoda wykonuje obliczenia równolegle w wielu wątkach.
        /// </remarks>
        /// <seealso cref="ShortestPathsGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool JohnsonShortestPathsParallel(this ASD.Graphs.Graph g, out PathsInfo[,] d)
        {
            d = null;
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");
            
            var graph = g.IsolatedVerticesGraph(true, g.VerticesCount + 1);
            for (var i = 0; i < g.VerticesCount; i++)
            {
                graph.AddEdge(g.VerticesCount, i, 0.0);
                foreach (var edge in g.OutEdges(i)) graph.AddEdge(edge);
            }
            
            if (!FordBellmanShortestPathsParallel(graph, g.VerticesCount, out var dFordBellman))
                return false;
            
            graph = g.IsolatedVerticesGraph();
            
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var edge in g.OutEdges(i))
                    graph.AddEdge(edge.From, edge.To,
                        edge.Weight + dFordBellman[edge.From].Dist - dFordBellman[edge.To].Dist);

            var dTemp = new PathsInfo[g.VerticesCount, g.VerticesCount];
            void Action(int i)
            {
                g.DijkstraShortestPaths(i, out var dDijkstra);
                for (var j = 0; j < g.VerticesCount; j++)
                {
                    dTemp[i, j].Dist = dDijkstra[j].Dist + dFordBellman[j].Dist - dFordBellman[i].Dist;
                    dTemp[i, j].Last = dDijkstra[j].Last;
                }
            }

            Parallel.For(0, g.VerticesCount, Action);
            
            d = dTemp;
            return true;
        }
    }
}
