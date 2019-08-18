using System;
using System.Linq;

namespace ASD.Graphs
{
    public static class DFSGraphExtender
    {
        public static bool DFSearchAll(this Graph g, Predicate<int> preVisit, Predicate<int> postVisit, out int cc, int[] nr = null)
        {
            if (nr != null)
            {
                if (nr.Length != g.VerticesCount)
                    throw new ArgumentException("Invalid order table");
                
                var used = new bool[g.VerticesCount];
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    if (nr[i] < 0 || nr[i] >= g.VerticesCount || used[nr[i]])
                        throw new ArgumentException("Invalid order table");
                    used[nr[i]] = true;
                }
            }
            else
            {
                nr = new int[g.VerticesCount];
                for (var i = 0; i < g.VerticesCount; i++) nr[i] = i;
            }
            var visitedVertices = new bool[g.VerticesCount];
            cc = 0;
            for (var j = 0; j < g.VerticesCount; j++)
            {
                if (visitedVertices[nr[j]]) continue;
                cc++;
                if (!DFSearchFrom(g, nr[j], preVisit, postVisit, visitedVertices))
                    return false;
            }
            return true;
        }

        public static bool DFSearchFrom(this Graph g, int from, Predicate<int> preVisit, Predicate<int> postVisit, bool[] visitedVertices = null)
        {
            if (visitedVertices == null)
                visitedVertices = new bool[g.VerticesCount];
            
            else if (visitedVertices.Length != g.VerticesCount)
                throw new ArgumentException("Invalid visitedVertices length");
            
            if (visitedVertices[from])
                throw new ArgumentException("Start vertex is already visited");

            if (preVisit == null)
                preVisit = i => true;
            
            if (postVisit == null)
                postVisit = i => true;
            
            visitedVertices[from] = true;
            
            if (!preVisit(from))
                return false;
            
            return !g.OutEdges(from).Any(edge => !visitedVertices[edge.To] && !DFSearchFrom(g, edge.To, preVisit, postVisit, visitedVertices)) && postVisit(from);
        }
    }
}
