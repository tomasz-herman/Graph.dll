using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o algorytmy wyznaczania minimalnego drzewa rozpinającego grafu
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class MSTGraphExtender
    {
        /// <summary>
        /// Wyznacza minimalne drzewo rozpinające grafu algorytmem Prima
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <returns>
        /// Krotka (weight, mst) składająca się z wagi minimalnego drzewa rozpinającego i grafu opisującego to drzewo
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczone drzewo rozpinające mst jest typu
        /// <see cref="AdjacencyListsGraph{AVLAdjacencyList}"/>, w przeciwnym
        /// przypadku drzewo rozpinające mst jest takiego samego typu jak graf g.<para/>
        /// Dla grafu skierowanego metoda zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Wyznaczone drzewo reprezentowane jest jako graf bez cykli, to umożliwia jednolitą obsługę sytuacji
        /// gdy analizowany graf jest niespójny, wyznaczany jest wówczas las rozpinający.
        /// </remarks>
        /// <seealso cref="MSTGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Graph mst) Prim(this Graph g)
        {
            if (g.Directed)
                throw new ArgumentException("Directed graphs are not allowed");
            
            var tree = g is AdjacencyMatrixGraph ? new AdjacencyListsGraph<AVLAdjacencyList>(false, g.VerticesCount) : g.IsolatedVerticesGraph();

            var weight = 0.0;
            bool VisitEdge(Edge edge)
            {
                if (tree.InDegree(edge.To) != 0) return true;
                tree.AddEdge(edge);
                weight += edge.Weight;
                return tree.EdgesCount != g.VerticesCount - 1;
            }
            g.GeneralSearchAll<EdgesMinPriorityQueue>(null, null, VisitEdge, out _);
            return (weight, tree);
        }
        
        /// <summary>
        /// Wyznacza minimalne drzewo rozpinające grafu algorytmem Kruskala
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <returns>
        /// Krotka (weight, mst) składająca się z wagi minimalnego drzewa rozpinającego i grafu opisującego to drzewo
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczone drzewo rozpinające mst jest typu
        /// <see cref="AdjacencyListsGraph{AVLAdjacencyList}"/>, w przeciwnym
        /// przypadku drzewo rozpinające mst jest takiego samego typu jak graf g.<para/>
        /// Dla grafu skierowanego metoda zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Wyznaczone drzewo reprezentowane jest jako graf bez cykli, to umożliwia jednolitą obsługę sytuacji
        /// gdy analizowany graf jest niespójny, wyznaczany jest wówczas las rozpinający.
        /// </remarks>
        /// <seealso cref="MSTGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Graph mst) Kruskal(this Graph g)
        {
            if (g.Directed)
                throw new ArgumentException("Directed graphs are not allowed");
            
            var unionFind = new UnionFind(g.VerticesCount);
            var edgesMinPriorityQueue = new EdgesMinPriorityQueue();
            var tree = g is AdjacencyMatrixGraph ? new AdjacencyListsGraph<AVLAdjacencyList>(false, g.VerticesCount) : g.IsolatedVerticesGraph();
            var weight = 0.0;
            
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var edge in g.OutEdges(i))
                    if (edge.From < edge.To)
                        edgesMinPriorityQueue.Put(edge);

            while (tree.EdgesCount < g.VerticesCount - 1 && !edgesMinPriorityQueue.Empty)
            {
                var edge = edgesMinPriorityQueue.Get();
                if (!unionFind.Union(edge.From, edge.To)) continue;
                tree.AddEdge(edge);
                weight += edge.Weight;
            }
            return (weight, tree);
        }
        
        /// <summary>
        /// Wyznacza minimalne drzewo rozpinające grafu algorytmem Boruvki
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <returns>
        /// Krotka (weight, mst) składająca się z wagi minimalnego drzewa rozpinającego i grafu opisującego to drzewo
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Jeśli graf g jest typu <see cref="AdjacencyMatrixGraph"/> to wyznaczone drzewo rozpinające mst jest typu
        /// <see cref="AdjacencyListsGraph{AVLAdjacencyList}"/>, w przeciwnym
        /// przypadku drzewo rozpinające mst jest takiego samego typu jak graf g.<para/>
        /// Dla grafu skierowanego metoda zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Wyznaczone drzewo reprezentowane jest jako graf bez cykli,
        /// to umożliwia jednolitą obsługę sytuacji gdy analizowany graf jest niespójny,
        /// wyznaczany jest wówczas las rozpinający.<para/>
        /// Jest to nieco zmodyfikowana wersja algorytmu Boruvki
        /// (nie ma operacji "sciągania" spójnych składowych w jeden wierzchołek).
        /// </remarks>
        /// <seealso cref="MSTGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Graph mst) Boruvka(this Graph g)
        {
            if (g.Directed)
                throw new ArgumentException("Directed graphs are not allowed");
            
            var unionFind = new UnionFind(g.VerticesCount);
            var edgesMinPriorityQueue = new EdgesMinPriorityQueue();
            var tree = g is AdjacencyMatrixGraph ? new AdjacencyListsGraph<AVLAdjacencyList>(false, g.VerticesCount) : g.IsolatedVerticesGraph();
            var weight = 0.0;
            var change = true;
            while (change)
            {
                change = false;
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    Edge? edge = null;
                    foreach (var e in g.OutEdges(i))
                        if (unionFind.Find(i) != unionFind.Find(e.To) && (edge == null || e.Weight < edge.Value.Weight))
                            edge = e;
                    if (edge != null)
                        edgesMinPriorityQueue.Put(edge.Value);
                }
                while (!edgesMinPriorityQueue.Empty)
                {
                    var edge = edgesMinPriorityQueue.Get();
                    if (!unionFind.Union(edge.From, edge.To)) continue;
                    tree.AddEdge(edge);
                    weight += edge.Weight;
                    if (tree.EdgesCount == g.VerticesCount - 1)
                        return (weight, tree);
                    change = true;
                }
            }
            return (weight, tree);
        }
    }
}
