using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> (i nie tylko tej) o różne metody pomocnicze
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class GraphHelperExtender
    {
        public static bool IsNaN(this double d)
        {
            return double.IsNaN(d);
        }

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
