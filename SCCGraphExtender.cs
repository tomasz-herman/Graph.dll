using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o algorytmy wyznaczania odwrotności grafu i silnie spójnych składowych grafu
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class SCCGraphExtender
    {
        public static Graph Reverse(this Graph g)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");

            var graph = g.IsolatedVerticesGraph();
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var edge in g.OutEdges(i))
                    graph.AddEdge(edge.To, edge.From, edge.Weight);

            return graph;
        }

        public static (int count, int[] scc) Kosaraju(this Graph g)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");

            var array = new int[g.VerticesCount];
            var vertCount = g.VerticesCount;
            var count = 0;
            var scc = new int[g.VerticesCount];

            // ReSharper disable once ImplicitlyCapturedClosure
            bool PostVisitVertex(int vert)
            {
                // ReSharper disable once AccessToModifiedClosure
                count++;
                array[vertCount - count] = vert;
                return true;
            }

            // ReSharper disable once ImplicitlyCapturedClosure
            bool PreVisitVertex(int vert)
            {
                // ReSharper disable once AccessToModifiedClosure
                scc[vert] = count - 1;
                return true;
            }

            g.GeneralSearchAll<EdgesStack>(null, PostVisitVertex, null, out _);
            Reverse(g).GeneralSearchAll<EdgesStack>(PreVisitVertex, null, null, out count, array);
            return (count, scc);
        }

        public static (int count, int[] scc) Tarjan(this Graph g)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");

            var count = 0;
            var visited = new bool[g.VerticesCount];
            var scc = new int[g.VerticesCount];
            var stack = new Stack<int>();
            var k = 0;
            var array = new int[g.VerticesCount];

            void StronglyConnected(int currVert)
            {
                k++;
                var temp = array[currVert] = k;
                visited[currVert] = true;
                stack.Push(currVert);
                foreach (var edge in g.OutEdges(currVert))
                {
                    if (!visited[edge.To])
                        StronglyConnected(edge.To);

                    if (array[edge.To] < array[currVert])
                        array[currVert] = array[edge.To];
                }

                if (array[currVert] < temp)
                    return;

                int vert;
                do
                {
                    vert = stack.Pop();
                    scc[vert] = count;
                    array[vert] = g.VerticesCount;
                } while (vert != currVert);

                count++;
            }

            for (var i = 0; i < g.VerticesCount; i++)
                if (!visited[i])
                    StronglyConnected(i);

            return (count, scc);
        }

    }
}
