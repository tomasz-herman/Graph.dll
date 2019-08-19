using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o ogólne algorytmy przeszukiwania (nie rekurencyjne)
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class GeneralSearchGraphExtender
    {
        public static bool GeneralSearchAll<TEdgesContainer>(this Graph g, Predicate<int> preVisitVertex, Predicate<int> postVisitVertex, Predicate<Edge> visitEdge, out int cc, int[] nr = null) where TEdgesContainer : IEdgesContainer, new()
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
            for (var i = 0; i < g.VerticesCount; i++)
            {
                if (visitedVertices[nr[i]]) continue;
                cc++;
                if (!g.GeneralSearchFrom<TEdgesContainer>(nr[i], preVisitVertex, postVisitVertex, visitEdge, visitedVertices))
                    return false;
            }
            return true;
        }

        public static bool GeneralSearchFrom<TEdgesContainer>(this Graph g, int from, Predicate<int> preVisitVertex, Predicate<int> postVisitVertex, Predicate<Edge> visitEdge, bool[] visitedVertices = null) where TEdgesContainer : IEdgesContainer, new()
        {
            if (postVisitVertex != null && typeof(TEdgesContainer) != typeof(EdgesStack))
                throw new ArgumentException("Parameter postVisitVertex must be null for containers other than EdgesStack");
            
            var stack = new Stack<int>();
            var edgesContainer = Activator.CreateInstance<TEdgesContainer>();
            
            if (visitedVertices == null)
                visitedVertices = new bool[g.VerticesCount];
            else if (visitedVertices.Length != g.VerticesCount)
                throw new ArgumentException("Invalid visitedVertices length");
            
            if (visitedVertices[from])
                throw new ArgumentException("Start vertex is already visited");
            
            if (visitEdge == null)
                visitEdge = edge => true;
            
            if (preVisitVertex == null)
                preVisitVertex = i => true;
            
            visitedVertices[from] = true;
            stack.Push(from);
            
            if (!preVisitVertex(from))
                return false;
            
            if (postVisitVertex != null && g.OutDegree(from) == 0)
                return postVisitVertex(from);
            
            foreach (var edge in g.OutEdges(from))
                edgesContainer.Put(edge);
            
            while (true)
            {
                if (edgesContainer.Empty)
                    return true;
                
                var edge = edgesContainer.Get();
                
                if (!visitEdge(edge))
                    return false;
                
                if (!visitedVertices[edge.To])
                {
                    visitedVertices[edge.To] = true;
                    stack.Push(edge.To);
                    
                    if (!preVisitVertex(edge.To))
                        break;
                    
                    foreach (var e in g.OutEdges(edge.To))
                        edgesContainer.Put(e);
                }

                if (postVisitVertex == null) continue;
                while (edgesContainer.Empty || stack.Peek() != edgesContainer.Peek().From)
                {
                    if (!postVisitVertex(stack.Pop()))
                        return false;
                    if (stack.Count == 0)
                        break;
                }
            }
            return false;
        }
    }
}
