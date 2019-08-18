using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace ASD.Graphs
{
    public static class AproxTSPGraphExtender
    {
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
                cycleCounter++;
                cycle[cycleCounter] = new Edge(lastVert, vert, w);
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
        
        public static (double weight, Edge[] cycle) IncludeTSP(this Graph g, TSPIncludeVertexSelectionMethod select = TSPIncludeVertexSelectionMethod.Sequential)
        {
            if (g.Directed && (select == TSPIncludeVertexSelectionMethod.Nearest || select == TSPIncludeVertexSelectionMethod.Furthest))
                throw new ArgumentException(
                    "Directed graphs are not allowed for Nearest and Furthest selection methods");
            if (g.VerticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);
            
            var linkedList = new LinkedList<Edge>();
            var hashSet = new HashSet<int>();
            PriorityQueue<int, double> priorityQueue = null;
            double[] array = null;
            linkedList.AddFirst(new Edge(0, 0, ClampDoubleToFloat(double.NaN)));
            for (var i = 1; i < g.VerticesCount; i++) hashSet.Add(i);
            
            if (select == TSPIncludeVertexSelectionMethod.Nearest)
            {
                bool Cmp(KeyValuePair<int, double> kvp1, KeyValuePair<int, double> kvp2)
                {
                    if (kvp1.Value != kvp2.Value)
                        return kvp1.Value < kvp2.Value;
                    return kvp1.Key < kvp2.Key;
                }
                priorityQueue = new PriorityQueue<int, double>(Cmp, CMonDoSomething.Nothing);
                for (var i = 1; i < g.VerticesCount; i++) 
                    priorityQueue.Put(i, ClampDoubleToFloat(double.PositiveInfinity));
                foreach (var edge in g.OutEdges(0)) 
                    priorityQueue.ImprovePriority(edge.To, ClampDoubleToFloat(edge.Weight));
            }
            if (select == TSPIncludeVertexSelectionMethod.Furthest)
            {
                array = new double[g.VerticesCount];
                for (var i = 1; i < g.VerticesCount; i++) 
                    array[i] = ClampDoubleToFloat(double.PositiveInfinity);
                foreach (var edge in g.OutEdges(0)) 
                    array[edge.To] = ClampDoubleToFloat(edge.Weight);
            }
            var l = 1;
            while (l < g.VerticesCount)
            {
                LinkedListNode<Edge> linkedListNode = null;
                int num2;
                switch (select)
                {
                    case TSPIncludeVertexSelectionMethod.Sequential:
                        num2 = l;
                        break;
                    case TSPIncludeVertexSelectionMethod.Nearest:
                        {
                            num2 = priorityQueue.Get();
                            foreach (var edge in g.OutEdges(num2))
                            {
                                if (hashSet.Contains(edge.To))
                                {
                                    priorityQueue.ImprovePriority(edge.To, ClampDoubleToFloat(edge.Weight));
                                }
                            }
                            break;
                            goto IL_36B;
                        }
                    case TSPIncludeVertexSelectionMethod.Furthest:
                        goto IL_36B;
                    case TSPIncludeVertexSelectionMethod.MinimalCost:
                        goto IL_455;
                    default:
                        throw new ArgumentException("Invalid vertex selection method");
                }
            IL_56D:
            hashSet.Remove(num2);
            bool flag12 = linkedListNode != null;
            double num3;
            if (!flag12)
            {
                num3 = double.PositiveInfinity;
                linkedListNode = linkedList.First;
                for (LinkedListNode<Edge> linkedListNode2 = linkedList.First; linkedListNode2 != null; linkedListNode2 = linkedListNode2.Next)
                {
                    double num4 = ClampDoubleToFloat(g.GetEdgeWeight(linkedListNode2.Value.From, num2));
                    double edgeWeight = g.GetEdgeWeight(num2, linkedListNode2.Value.To);
                    double num5 = num4 + ClampDoubleToFloat(edgeWeight) - ClampDoubleToFloat(linkedListNode2.Value.Weight);
                    if (num3 > num5)
                    {
                        linkedListNode = linkedListNode2;
                        num3 = num5;
                    }
                }
            }
            int to = linkedListNode.Value.To;
            linkedList.AddBefore(linkedListNode, new Edge(linkedListNode.Value.From, num2, g.GetEdgeWeight(linkedListNode.Value.From, num2)));
            LinkedList<Edge> linkedList2 = linkedList;
            LinkedListNode<Edge> node = linkedListNode;
            int int_ = num2;
            int int_2 = linkedListNode.Value.To;
            int from2 = num2;
            int to2 = linkedListNode.Value.To;
            linkedList2.AddBefore(node, new Edge(int_, linkedListNode.Value.To, g.GetEdgeWeight(from2, linkedListNode.Value.To)));
            linkedList.Remove(linkedListNode);
            l++;
            continue;
            IL_36B:
                num3 = double.NegativeInfinity;
                num2 = hashSet.First();
                foreach (int num7 in hashSet)
                {
                    if (num3 < array[num7])
                    {
                        num2 = num7;
                        num3 = array[num7];
                    }
                }
                using (IEnumerator<Edge> enumerator = g.OutEdges(num2).GetEnumerator())
                {
                    while (enumerator.MoveNext())
                    {
                        Edge edge4 = enumerator.Current;
                        double num5 = ClampDoubleToFloat(edge4.Weight);
                        if (array[edge4.To] > num5)
                        {
                            array[edge4.To] = num5;
                        }
                    }
                    goto IL_56D;
                }
            IL_455:
                double positiveInfinity = double.PositiveInfinity;
                num3 = positiveInfinity;
                linkedListNode = linkedList.First;
                num2 = hashSet.First();
                foreach (int num8 in hashSet)
                {
                    for (LinkedListNode<Edge> linkedListNode3 = linkedList.First; linkedListNode3 != null; linkedListNode3 = linkedListNode3.Next)
                    {
                        int from3 = linkedListNode3.Value.From;
                        int to3 = num8;
                        double num5 = ClampDoubleToFloat(g.GetEdgeWeight(from3, to3)) + ClampDoubleToFloat(g.GetEdgeWeight(num8, linkedListNode3.Value.To)) - ClampDoubleToFloat(linkedListNode3.Value.Weight);
                        if (num3 > num5)
                        {
                            linkedListNode = linkedListNode3;
                            num2 = num8;
                            num3 = num5;
                        }
                    }
                }
                goto IL_56D;
            }
            var cycle = linkedList.ToArray();
            var weight = 0.0;
            for (var i = 0; i < cycle.Length; i++) 
                weight += cycle[i].Weight;
            return !weight.IsNaN() ? (weight, cycle) : (double.NaN, null);
        }
        
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
                bool flag4 = false;
                int verticesCount = g.VerticesCount;
                double[] array = new double[verticesCount];
                int i;
                int j;
                if (init != null)
                {
                    int num2 = init.Length;
                    int num3 = verticesCount;
                    if (num2 == num3 && init[0] > 0 && init[0] < verticesCount)
                    {
                        bool[] array2 = new bool[verticesCount];
                        i = 2;
                        j = init[0];
                        while (i < verticesCount)
                        {
                            if (init[j] <= 0 || init[j] >= verticesCount || init[j] == j || array2[j])
                            {
                                break;
                            }
                            array2[j] = true;
                            int num4 = i;
                            i = num4 + 1;
                            j = init[j];
                        }
                        if (i == verticesCount && init[j] == 0)
                        {
                            flag4 = true;
                        }
                    }
                }
                int[] array3;
                if (flag4)
                {
                    array3 = (int[])init.Clone();
                }
                else
                {
                    array3 = new int[verticesCount];
                    for (j = 1; j < verticesCount; j++)
                    {
                        array3[j - 1] = j;
                    }
                    array3[verticesCount - 1] = 0;
                }
                int num5 = -1;
                int num6 = -1;
                int num7 = -1;
                int num8 = num5;
                double weight;
                for (; ; )
                {
                    double num9 = 0.0;
                    for (j = 0; j < verticesCount; j++)
                    {
                        double[] array4 = array;
                        int num10 = j;
                        array4[num10] = ClampDoubleToFloat(g.GetEdgeWeight(j, array3[j]));
                    }
                    for (j = 0; j < verticesCount; j++)
                    {
                        if (array3[j] != 0)
                        {
                            int[] array5 = array3;
                            int num11 = j;
                            int num12 = array5[num11];
                            for (; ; )
                            {
                                int[] array6 = array3;
                                int num13 = num12;
                                if (array6[num13] == 0)
                                {
                                    break;
                                }
                                weight = ClampDoubleToFloat(g.GetEdgeWeight(j, array3[num12]));
                                for (i = array3[num12]; i != 0; i = array3[i])
                                {
                                    double num15 = array[j] + array[num12];
                                    double num16 = num15 + array[i] - weight - ClampDoubleToFloat(g.GetEdgeWeight(num12, array3[i])) - ClampDoubleToFloat(g.GetEdgeWeight(i, array3[j]));
                                    if (num9 < num16)
                                    {
                                        num9 = num16;
                                        num8 = j;
                                        num7 = num12;
                                        num6 = i;
                                    }
                                }
                                num12 = array3[num12];
                            }
                        }
                    }
                    if (num9 == 0.0)
                    {
                        break;
                    }
                    j = array3[num8];
                    array3[num8] = array3[num7];
                    int[] array7 = array3;
                    array7[num7] = array3[num6];
                    array3[num6] = j;
                }
                Edge[] cycle = new Edge[verticesCount];
                weight = 0.0;
                int num17 = 0;
                j = 0;
                i = num17;
                while (i < verticesCount)
                {
                    Edge[] array9 = cycle;
                    int num18 = i;
                    int int_ = j;
                    array9[num18] = new Edge(int_, array3[j], g.GetEdgeWeight(j, array3[j]));
                    weight += cycle[i].Weight;
                    i++;
                    int[] array10 = array3;
                    j = array10[j];
                }
                return !weight.IsNaN() ? (weight, cycle) : (double.NaN, null);
            }
        }
        
        public static (double weight, Edge[] cycle) ThreeOptTSPParallel(this Graph g, int[] init = null)
        {
            Class42 @class = new Class42();
            Class42 class2 = @class;
            class2.g = g;
            if (class2.g.VerticesCount <= (class2.g.Directed ? 1 : 2))
            {
                return new ValueTuple<double, Edge[]>(double.NaN, null);
            }
            int verticesCount = class2.g.VerticesCount;
            int num = 2;
            if (verticesCount == num)
            {
                double num2 = class2.g.GetEdgeWeight(0, 1) + class2.g.GetEdgeWeight(1, 0);
                if (!num2.IsNaN())
                {
                    double item = num2;
                    Edge[] array = new Edge[2];
                    int num3 = 0;
                    Graph graph_ = class2.g;
                    array[num3] = graph_.OutEdges(0).First();
                    array[1] = class2.g.OutEdges(1).First();
                    return new ValueTuple<double, Edge[]>(item, array);
                }
                return new ValueTuple<double, Edge[]>(double.NaN, null);
            }

            class2.init = null;
            bool flag4 = false;
            int verticesCount2 = class2.g.VerticesCount;
            class2.double_1 = new double[verticesCount2];
            class2.int_1 = new int[verticesCount2];
            class2.int_2 = new int[verticesCount2];
            class2.double_0 = new double[verticesCount2];
            Class42 class3 = class2;
            Graph graph_2 = class2.g;
            int i;
            int j;
            if (init != null && init.Length == verticesCount2 && init[0] > 0 && init[0] < verticesCount2)
            {
                bool[] array2 = new bool[verticesCount2];
                bool[] array3 = array2;
                i = 2;
                j = init[0];
                while (i < verticesCount2)
                {
                    if (init[j] <= 0 || init[j] >= verticesCount2)
                    {
                        break;
                    }
                    int num4 = init[j];
                    int num5 = j;
                    if (num4 == num5 || array3[j])
                    {
                        break;
                    }
                    array3[j] = true;
                    i++;
                    j = init[j];
                }
                int num6 = i;
                int num7 = verticesCount2;
                if (num6 == num7 && init[j] == 0)
                {
                    flag4 = true;
                }
            }
            if (flag4)
            {
                class2.init = (int[])init.Clone();
            }
            else
            {
                class2.init = new int[verticesCount2];
                for (j = 1; j < verticesCount2; j++)
                {
                    Class42 class4 = class2;
                    class4.init[j - 1] = j;
                }
                class2.init[verticesCount2 - 1] = 0;
            }
            double num8;
            for (; ; )
            {
                for (j = 0; j < verticesCount2; j++)
                {
                    class2.double_1[j] = ClampDoubleToFloat(class2.g.GetEdgeWeight(j, class2.init[j]));
                }
                Parallel.For(0, verticesCount2, class2.method_0);
                num8 = 0.0;
                for (i = 0; i < verticesCount2; i++)
                {
                    if (num8 < class2.double_0[i])
                    {
                        num8 = class2.double_0[i];
                        j = i;
                    }
                }
                if (num8 == 0.0)
                {
                    break;
                }
                i = class2.init[j];
                Class42 class5 = class2;
                class5.init[j] = class2.init[class2.int_1[j]];
                int[] int_ = class2.init;
                int num9 = class2.int_1[j];
                int num10 = class2.init[class2.int_2[j]];
                int_[num9] = num10;
                class2.init[class2.int_2[j]] = i;
            }
            Edge[] array4 = new Edge[verticesCount2];
            num8 = 0.0;
            int num11 = 0;
            j = 0;
            i = num11;
            while (i < verticesCount2)
            {
                Edge[] array5 = array4;
                int num12 = i;
                int int_2 = j;
                array5[num12] = new Edge(int_2, class2.init[j], class2.g.GetEdgeWeight(j, class2.init[j]));
                num8 += array4[i].Weight;
                int num13 = i;
                int num14 = 1;
                i = num13 + num14;
                j = class2.init[j];
            }
            if (!num8.IsNaN())
            {
                return new ValueTuple<double, Edge[]>(num8, array4);
            }
            return new ValueTuple<double, Edge[]>(double.NaN, null);
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

        private static double ClampDoubleToFloat(double double0)
        {
            if (double0.IsNaN() || double0 > 3.4028234663852886E+38)
            {
                double0 = 3.4028234663852886E+38;
            }
            return Math.Max(double0, -3.4028234663852886E+38);
        }
        
        [CompilerGenerated]
        private sealed class Class42
        {
            internal void method_0(int int_3)
            {
                double_0[int_3] = 0.0;
                if (init[int_3] == 0)
                {
                    return;
                }
                int num = init[int_3];
                while (init[num] != 0)
                {
                    double num2 = ClampDoubleToFloat(g.GetEdgeWeight(int_3, init[num]));
                    for (int num3 = init[num]; num3 != 0; num3 = init[num3])
                    {
                        double num4 = double_1[int_3] + double_1[num] + double_1[num3] - num2 - ClampDoubleToFloat(g.GetEdgeWeight(num, init[num3])) - ClampDoubleToFloat(g.GetEdgeWeight(num3, init[int_3]));
                        if (double_0[int_3] < num4)
                        {
                            double_0[int_3] = num4;
                            int_1[int_3] = num;
                            int_2[int_3] = num3;
                        }
                    }
                    num = init[num];
                }
            }

            public double[] double_0;

            public int[] init;

            public Graph g;

            public double[] double_1;

            public int[] int_1;

            public int[] int_2;
        }
    }
}
