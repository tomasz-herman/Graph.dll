using System;
using System.Linq;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o wyszukiwanie ścieżki Eulera
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class EulerPathGraphExtender
    {
        public static bool EulerPath(this Graph g, out Edge[] ec)
        {
            ec = null;
            var oddDegreeCounter = 0;
            var startVertex = 0;
            var hasOutGreaterThanIn = false;
            var hasInGreaterThanOut = false;
            if (g.Directed)
            {
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    var outDegree = g.OutDegree(i);
                    var inDegree = g.InDegree(i);
                    if (Math.Abs(outDegree - inDegree) > 1)
                        return false;
                    if (outDegree > inDegree)
                    {
                        if (hasOutGreaterThanIn)
                            return false;
                        startVertex = i;
                        hasOutGreaterThanIn = true;
                    }
                    if (inDegree > outDegree)
                    {
                        if (hasInGreaterThanOut)
                            return false;
                        hasInGreaterThanOut = true;
                    }
                }
            }
            else
            {
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    if ((g.OutDegree(i) & 1) != 1) continue;
                    startVertex = i;
                    if (++oddDegreeCounter > 2)
                        return false;
                }
            }
            var visited = new bool[g.VerticesCount];
            var graph = g.Clone();
            var s1 = new EdgesStack();
            var s2 = new EdgesStack();
            s2.Put(new Edge(startVertex, startVertex));
            while (!s2.Empty)
            {
                var vertex = s2.Peek().To;
                visited[vertex] = true;
                if (graph.OutDegree(vertex) > 0)
                {
                    var edge = graph.OutEdges(vertex).First();
                    s2.Put(edge);
                    graph.DelEdge(edge);
                }
                else
                    s1.Put(s2.Get());
            }
            s1.Get();
            
            if (graph.EdgesCount > 0)
                return false;

            for (var i = 0; i < g.VerticesCount; i++)
                if (!visited[i])
                    return false;

            ec = s1.ToArray();
            return true;
        }

      
    }
}
