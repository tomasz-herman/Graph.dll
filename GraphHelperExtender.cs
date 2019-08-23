using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> (i nie tylko tej) o różne metody pomocnicze
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class GraphHelperExtender
    {
        /// <summary>
        /// Bada czy podana wartość typu double odpowiada nieliczbie
        /// </summary>
        /// <param name="d">Badana wartość</param>
        /// <returns>
        /// To "nakładka" upraszczająca wywołanie metody <see cref="double.IsNaN(double)"/>.<para/>
        /// Wykorzystanie <see cref="double.op_Equality"/> spowoduje błędną odpowiedź (zawsze false).
        /// </returns>
        /// <seealso cref="GraphHelperExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool IsNaN(this double d)
        {
            return double.IsNaN(d);
        }

        /// <summary>
        /// Bada czy zadane grafy są jednakowe
        /// </summary>
        /// <param name="g">Pierwszy badany graf</param>
        /// <param name="h">Drugi badany graf</param>
        /// <returns>Informacja czy zadane grafy są jednakowe</returns>
        /// <remarks>Badana jest struktura grafu, sposób reprezentacji nie ma znaczenia.</remarks>
        /// <seealso cref="GraphHelperExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool IsEqual(this Graph g, Graph h)
        {
            if (g.VerticesCount != h.VerticesCount || g.EdgesCount != h.EdgesCount) return false;
            if (g.Directed != h.Directed) return false;
            for (var i = 0; i < g.VerticesCount; i++)
                if (g.OutDegree(i) != h.OutDegree(i) || g.InDegree(i) != h.InDegree(i)) return false;
            
            for (var i = 0; i < g.VerticesCount; i++)
                if (g.OutEdges(i).Any(edge => h.GetEdgeWeight(i, edge.To) != edge.Weight))
                    return false;

            return true;
        }

        /// <summary>
        /// Bada czy zadane grafy są jednakowe
        /// </summary>
        /// <param name="g">Pierwszy badany graf</param>
        /// <param name="h">Drugi badany graf</param>
        /// <returns>Informacja czy zadane grafy są jednakowe</returns>
        /// <remarks>
        /// Badana jest struktura grafu, sposób reprezentacji nie ma znaczenia.<para/>
        /// Metoda wykonuje obliczenia równolegle w wielu wątkach.
        /// </remarks>
        /// <seealso cref="GraphHelperExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool IsEqualParallel(this Graph g, Graph h)
        {
            if (g.VerticesCount != h.VerticesCount || g.EdgesCount != h.EdgesCount) return false;
            if (g.Directed != h.Directed) return false;
            for (var i = 0; i < g.VerticesCount; i++)
                if (g.OutDegree(i) != h.OutDegree(i) || g.InDegree(i) != h.InDegree(i)) return false;
            
            var result = Parallel.For(0, g.VerticesCount, (i, state) =>
            {
                foreach (var edge in g.OutEdges(i))
                    if (h.GetEdgeWeight(i, edge.To) != edge.Weight)
                        state.Stop();
            });
            return result.IsCompleted;
        }

        /// <summary>
        /// Wyznacza sumę grafów
        /// </summary>
        /// <param name="g">Pierwszy sumowany graf</param>
        /// <param name="h">Drugi sumowany graf</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException">Gdy argumentem jest graf skierowany</exception>
        /// <remarks>
        /// Suma grafów to graf składający się ze wszystkich wierchołków i krawędzi sumowanych grafów
        /// (wierzchołki pochodzące z drugiego grafu są odpowiednio przenumerowane).<para/>
        /// Można sumować grafy reprezentowane w różny sposób,
        /// suma ma taką reprezentację jak pierwszy z sumowanych grafów).
        /// </remarks>
        /// <seealso cref="GraphHelperExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static Graph Union(this Graph g, Graph h)
        {
            if (g.Directed != h.Directed)
                throw new ArgumentException("Union of directed and undirected graph are not allowed");
            
            var union = g.IsolatedVerticesGraph(g.Directed, g.VerticesCount + h.VerticesCount);
            
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var e in g.OutEdges(i))
                    union.AddEdge(e);
            
            for (var i = 0; i < h.VerticesCount; i++)
                foreach (var edge in h.OutEdges(i))
                    union.AddEdge(i + g.VerticesCount, edge.To + g.VerticesCount, edge.Weight);

            return union;
        }

        /// <summary>
        /// Sortowanie topologiczne wierzchołków grafu
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="original2topological">Tablica opisująca przekształcenie numeracji pierwotnej w topologiczną</param>
        /// <param name="topological2original">Tablica opisująca przekształcenie numeracji topologicznej w pierworną</param>
        /// <returns>Informacja czy posortowanie topologiczne jest możliwe</returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu nieskierowanego</exception>
        /// <remarks>
        /// Wartość original2topological[i] to numer w porządku topologicznym wierzchołka o numerze pierwotnym i.<para/>
        /// Wartość topological2original[i] to numer w porządku pierwotnym wierzchołka o numerze topologicznym i.<para/>
        /// Jeśli posortowanie topologiczne grafu g nie jest możliwe (graf nie jest acykliczny)
        /// to parametry original2topological i topological2original są równe null.<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego zgłasza wyjątek <see cref="ArgumentException"/>.
        /// </remarks>
        /// <seealso cref="GraphHelperExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool TopologicalSort(this Graph g, out int[] original2topological, out int[] topological2original)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graph are not allowed");
            
            var o2t = new int[g.VerticesCount];
            var t2o = new int[g.VerticesCount];
            var visitedVertices = new bool[g.VerticesCount];
            var verticesCount = g.VerticesCount;
            topological2original = null;
            original2topological = null;
            
            for (var i = 0; i < g.VerticesCount; i++)
                o2t[i] = -1;
            
            bool PreVisitVertex(int vert)
            {
                visitedVertices[vert] = true;
                return true;
            }
            
            bool PostVisitVertex(int vert)
            {
                verticesCount--;
                o2t[vert] = verticesCount;
                t2o[verticesCount] = vert;
                return true;
            }
            
            bool VisitEdge(Edge edge) => !visitedVertices[edge.To] || o2t[edge.To] != -1;

            if (!g.GeneralSearchAll<EdgesStack>(PreVisitVertex, PostVisitVertex, VisitEdge, out _)) 
                return false;
            original2topological = o2t;
            topological2original = t2o;
            return true;
        }
    }
}
