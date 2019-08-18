using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ASD.Graphs
{
    public static class ShortestPathsGraphExtender
    {
        public static bool FordBellmanShortestPaths(this Graph g, int s, out PathsInfo[] d)
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
        
        public static bool FordBellmanShortestPathsParallel(this Graph g, int s, out PathsInfo[] d)
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

        public static (double weight, Edge[] cycle) FindNegativeCostCycle(this Graph g, PathsInfo[] d)
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
        
        public static bool DijkstraShortestPaths(this Graph g, int s, out PathsInfo[] d)
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

        public static bool DAGShortestPaths(this Graph g, int s, out PathsInfo[] d)
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

        public static bool BFPaths(this Graph g, int s, out PathsInfo[] d)
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

        public static bool FloydWarshallShortestPaths(this Graph g, out PathsInfo[,] d)
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

        public static bool FloydWarshallShortestPathsParallel(this Graph g, out PathsInfo[,] d)
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

        public static bool JohnsonShortestPaths(this Graph g, out PathsInfo[,] d)
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

        public static bool JohnsonShortestPathsParallel(this Graph g, out PathsInfo[,] d)
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
