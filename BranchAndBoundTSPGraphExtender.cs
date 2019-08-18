using System;
using System.Collections.Concurrent;
using System.Threading;

namespace ASD.Graphs
{
    public static class BranchAndBoundTSPGraphExtender
    {
        public static (double weight, Edge[] cycle) BranchAndBoundTSP(this Graph g, bool multiThread = false)
        {
            var verticesCount = g.VerticesCount;
            if (verticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);
            var array = new double[verticesCount + 1, verticesCount + 1];
            var array2 = new bool[verticesCount];
            for (var i = 0; i < verticesCount; i++)
            {
                for (var j = 0; j < verticesCount; j++)
                {
                    array[i, j] = double.PositiveInfinity;
                }
            }
            for (var i = 0; i < verticesCount; i++)
            {
                var min = double.PositiveInfinity;
                foreach (var edge in g.OutEdges(i))
                {
                    if (edge.Weight < 0.0)
                        throw new ArgumentException("Negative weights are not allowed");
                    array[i, edge.To] = edge.Weight;
                    if (min > edge.Weight && i != edge.To) 
                        min = edge.Weight;
                }
                if (double.IsPositiveInfinity(min))
                    return (double.NaN, null);
                array[i, i] = double.NaN;
                array[verticesCount, i] = array[i, verticesCount] = i;
                array[verticesCount, verticesCount] += min;
                for (var j = 0; j < verticesCount; j++)
                {
                    array[i, j] -= min;
                    if (array[i, j] == 0.0) array2[j] = true;
                }
            }
            for (var i = 0; i < verticesCount; i++)
            {
                if (array2[i]) continue;
                
                var min = double.PositiveInfinity;
                for (var j = 0; j < verticesCount; j++)
                    if (min > array[j, i])
                        min = array[j, i];
                if (double.IsPositiveInfinity(min))
                    return (double.NaN, null);
                array[verticesCount, verticesCount] += min;
                for (var j = 0; j < verticesCount; j++) array[j, i] -= min;
            }
            
            var class44 = new Class44(g, multiThread);
            var struct8 = new Struct8(array, new Edge[g.VerticesCount]);

            if (multiThread)
                class44.BreachAndBoundMultiThread(struct8);
            else
                class44.BreachAndBoundSingleThread(struct8);
            
            return !double.IsPositiveInfinity(class44.weight) ? (class44.weight, class44.getCycle()) : (double.NaN, null);
        }

        private struct Struct8
        {
            internal Struct8(double[,] double_1, Edge[] edge_1)
            {
                double_0 = double_1;
                edge_0 = edge_1;
            }

            internal double[,] double_0;

            internal Edge[] edge_0;
        }

        private sealed class Class44
        {
            internal double weight;

            [ThreadStatic]
            private static double[][,] double_1;

            private Graph g;

            private Edge[] edge_0;

            private int int_0;

            private ConcurrentStack<Struct8> concurrentStack_0;

            private volatile int int_1;

            private readonly object mutex = new object();

            private Exception exception_0;
            
            internal Class44(Graph graph_1, bool bool_0)
            {
                g = graph_1;
                weight = double.PositiveInfinity;
                if (bool_0)
                {
                    concurrentStack_0 = new ConcurrentStack<Struct8>();
                    return;
                }
                double_1 = new double[graph_1.VerticesCount + 1][,];
                for (int i = 2; i <= graph_1.VerticesCount; i++)
                {
                    double_1[i] = new double[i, i];
                }
            }

            internal Edge[] getCycle()
            {
                var verticesCount = g.VerticesCount;
                var cycle = new Edge[verticesCount];
                int num = 0;
                for (int i = 0; i < verticesCount; i++)
                {
                    int num2 = 0;
                    while (edge_0[num2].From != num)
                    {
                        num2++;
                    }
                    cycle[i] = edge_0[num2];
                    num = edge_0[num2].To;
                }
                return cycle;
            }

            internal void BreachAndBoundMultiThread(Struct8 struct8_0)
            {
                var num = (ulong)struct8_0.double_0.GetLength(0);
                double num2 = num * (num + 1UL) * (2UL * num + 1UL) / 6UL * 8UL;
                double num3 = 4000000000.0;
                int_0 = Environment.ProcessorCount;
                if (num2 * int_0 > Math.Min(num3, 4000000000.0))
                {
                    if (num3 < 4000000000.0)
                    {
                        GC.Collect(GC.MaxGeneration, GCCollectionMode.Forced, true, true);
                        num3 = 4000000000.0;
                    }
                    int_0 = (int)(Math.Min(num3, 4000000000.0) / num2);
                    if (int_0 == 0)
                    {
                        int_0 = 1;
                    }
                }
                concurrentStack_0.Push(struct8_0);
                int_1 = int_0 + 1;
                Thread[] array = new Thread[int_0];
                for (int i = 0; i < array.Length; i++)
                {
                    array[i] = new Thread(method_2, 0);
                    array[i].Start();
                }
                for (int j = 0; j < array.Length; j++)
                {
                    array[j].Join();
                }
                if (exception_0 != null)
                {
                    throw exception_0;
                }
            }

            private void method_2()
            {
                bool flag = true;
                try
                {
                    Struct8 @struct = default;
                    int num = g.VerticesCount;
                    int num2 = num - int_0;
                    if (num2 < 45)
                    {
                        num2 = 45;
                    }
                    int num3 = 6 * int_0;
                    double_1 = new double[num][,];
                    for (int i = 2; i < num; i++)
                    {
                        double_1[i] = new double[i, i];
                    }
                    for (; ; )
                    {
                        if (flag)
                        {
                            Interlocked.Decrement(ref int_1);
                            while (!concurrentStack_0.TryPop(out @struct))
                            {
                                if (int_1 == 0)
                                {
                                    goto IL_1D5;
                                }
                                Thread.Sleep(10);
                            }
                        }
                        flag = true;
                        num = @struct.double_0.GetLength(0) - 1;
                        if (@struct.double_0[num, num] < weight)
                        {
                            if (num == 2)
                            {
                                method_6(@struct);
                            }
                            else
                            {
                                ValueTuple<double, int, int> valueTuple = method_5(@struct.double_0);
                                double item = valueTuple.Item1;
                                int item2 = valueTuple.Item2;
                                int item3 = valueTuple.Item3;
                                if (item >= 0.0)
                                {
                                    Struct8 struct2 = method_4(@struct, item2, item3, new double[num, num]);
                                    if (@struct.double_0[num, num] + item < weight)
                                    {
                                        @struct.double_0[item2, item3] = double.PositiveInfinity;
                                        if (smethod_0(@struct.double_0, item2) && smethod_1(@struct.double_0, item3))
                                        {
                                            concurrentStack_0.Push(new Struct8(@struct.double_0, (Edge[])@struct.edge_0.Clone()));
                                        }
                                        Interlocked.Increment(ref int_1);
                                    }
                                    if (num >= num2 && int_1 <= num3)
                                    {
                                        @struct = struct2;
                                        flag = false;
                                    }
                                    else
                                    {
                                        BreachAndBoundSingleThread(struct2);
                                    }
                                }
                            }
                        }
                    }
                IL_1D5:;
                }
                catch (Exception ex)
                {
                    if (exception_0 == null)
                    {
                        exception_0 = ex;
                    }
                    Interlocked.Decrement(ref int_1);
                }
            }

            internal void BreachAndBoundSingleThread(Struct8 struct8_0)
            {
                int num = struct8_0.double_0.GetLength(0) - 1;
                for (; ; )
                {
                    if (struct8_0.double_0[num, num] >= weight)
                    {
                        return;
                    }
                    if (num == 2)
                    {
                        break;
                    }
                    var (m, i, j) = method_5(struct8_0.double_0);
                    if (m < 0.0)
                    {
                        return;
                    }
                    BreachAndBoundSingleThread(method_4(struct8_0, i, j, double_1[num]));
                    if (struct8_0.double_0[num, num] + m >= weight)
                    {
                        return;
                    }
                    struct8_0.double_0[i, j] = double.PositiveInfinity;
                    if (smethod_0(struct8_0.double_0, i))
                    {
                        smethod_1(struct8_0.double_0, j);
                    }
                }
                method_6(struct8_0);
            }

            // Token: 0x06000246 RID: 582 RVA: 0x00016AAC File Offset: 0x00014CAC
            private Struct8 method_4(Struct8 struct8_0, int int_2, int int_3, double[,] double_2)
            {
                int num = struct8_0.double_0.GetLength(0) - 1;
                bool[] array = new bool[num - 1];
                bool[] array2 = new bool[num - 1];
                bool[] array3 = new bool[num - 1];
                bool[] array4 = new bool[num - 1];
                int num2 = 0;
                int i = 0;
                int j = num2;
                int l;
                while (i <= num)
                {
                    if (i == int_2)
                    {
                        j--;
                    }
                    else
                    {
                        int num3 = 0;
                        int k = 0;
                        l = num3;
                        while (k <= num)
                        {
                            if (k == int_3)
                            {
                                l--;
                            }
                            else
                            {
                                double_2[j, l] = struct8_0.double_0[i, k];
                                if (i != num && k != num)
                                {
                                    if (double_2[j, l] == 0.0)
                                    {
                                        bool[] array5 = array3;
                                        int num4 = j;
                                        array4[l] = true;
                                        array5[num4] = true;
                                    }
                                    if (double_2[j, l].IsNaN())
                                    {
                                        bool[] array6 = array;
                                        int num5 = j;
                                        array2[l] = true;
                                        array6[num5] = true;
                                    }
                                }
                            }
                            k++;
                            l++;
                        }
                    }
                    i++;
                    j++;
                }
                j = 0;
                while (j < num - 1 && array[j])
                {
                    j++;
                }
                l = 0;
                while (l < num - 1 && array2[l])
                {
                    l++;
                }
                if (double_2[j, l] == 0.0)
                {
                    bool[] array7 = array3;
                    int num6 = j;
                    array4[l] = false;
                    array7[num6] = false;
                }
                double_2[j, l] = double.NaN;
                int from = (int)struct8_0.double_0[int_2, num];
                int to = (int)struct8_0.double_0[num, int_3];
                struct8_0.edge_0[num - 1] = new Edge(from, to, g.GetEdgeWeight(from, to));
                for (j = 0; j < num - 1; j++)
                {
                    if (!array3[j])
                    {
                        smethod_0(double_2, j);
                    }
                }
                for (l = 0; l < num - 1; l++)
                {
                    if (!array4[l])
                    {
                        smethod_1(double_2, l);
                    }
                }
                return new Struct8(double_2, struct8_0.edge_0);
            }

            private (double m, int i, int j) method_5(double[,] double_2)
            {
                int num = double_2.GetLength(0) - 1;
                bool[] array = new bool[num];
                bool[] array2 = new bool[num];
                double[] array3 = new double[num];
                double[] array4 = new double[num];
                double num2 = -1.0;
                int num3 = -1;
                int item = -1;
                int item2 = num3;
                for (int i = 0; i < num; i++)
                {
                    double[] array5 = array3;
                    int num4 = i;
                    double[] array6 = array4;
                    int num5 = i;
                    double positiveInfinity = double.PositiveInfinity;
                    double positiveInfinity2 = float.PositiveInfinity;
                    array6[num5] = positiveInfinity;
                    array5[num4] = positiveInfinity2;
                }
                for (int j = 0; j < num; j++)
                {
                    for (int k = 0; k < num; k++)
                    {
                        double num6 = double_2[j, k];
                        if (double_2[j, k] == 0.0)
                        {
                            if (!array[j])
                            {
                                for (int l = k + 1; l < num; l++)
                                {
                                    if (array3[j] > double_2[j, l])
                                    {
                                        array3[j] = double_2[j, l];
                                        if (array3[j] == 0.0)
                                        {
                                            break;
                                        }
                                    }
                                }
                                array[j] = true;
                            }
                            if (!array2[k])
                            {
                                for (int m = j + 1; m < num; m++)
                                {
                                    if (array4[k] > double_2[m, k])
                                    {
                                        array4[k] = double_2[m, k];
                                        if (array4[k] == 0.0)
                                        {
                                            break;
                                        }
                                    }
                                }
                                array2[k] = true;
                            }
                            if (num2 < array3[j] + array4[k])
                            {
                                num2 = array3[j] + array4[k];
                                item2 = j;
                                item = k;
                            }
                        }
                        else
                        {
                            if (array3[j] > num6)
                            {
                                array3[j] = num6;
                            }
                            if (array4[k] > num6)
                            {
                                array4[k] = num6;
                            }
                        }
                    }
                    if (!array[j])
                    {
                        return new ValueTuple<double, int, int>(-1.0, -1, -1);
                    }
                }
                for (int n = 0; n < num; n++)
                {
                    if (!array2[n])
                    {
                        return new ValueTuple<double, int, int>(-1.0, -1, -1);
                    }
                }
                return new ValueTuple<double, int, int>(num2, item2, item);
            }

            private static bool smethod_0(double[,] matrix, int i)
            {
                var length = matrix.GetLength(0) - 1;
                var min = double.PositiveInfinity;
                for (var j = 0; j < length; j++)
                {
                    if (!(min > matrix[i, j])) continue;
                    min = matrix[i, j];
                    if (min == 0.0)
                        break;
                }
                if (double.IsPositiveInfinity(min))
                {
                    matrix[length, length] = double.PositiveInfinity;
                    return false;
                }

                if (min <= 0.0) return true;
                {
                    matrix[length, length] += min;
                    for (var j = 0; j < length; j++) 
                        matrix[i, j] -= min;
                }
                return true;
            }

            private static bool smethod_1(double[,] matrix, int j)
            {
                var length = matrix.GetLength(0) - 1;
                var min = double.PositiveInfinity;
                for (int i = 0; i < length; i++)
                {
                    if (!(min > matrix[i, j])) continue;
                    min = matrix[i, j];
                    if (min == 0.0)
                        break;
                }
                if (double.IsPositiveInfinity(min))
                {
                    matrix[length, length] = double.PositiveInfinity;
                    return false;
                }

                if (min <= 0.0) return true;
                {
                    matrix[length, length] += min;
                    for (var i = 0; i < length; i++) 
                        matrix[i, j] -= min;
                }
                return true;
            }

            private void method_6(Struct8 struct8_0)
            {
                if (struct8_0.double_0[2, 2] >= weight)
                {
                    return;
                }
                if (!struct8_0.double_0[0, 0].IsNaN())
                {
                    int from = (int)struct8_0.double_0[0, 2];
                    int to = (int)struct8_0.double_0[2, 0];
                    struct8_0.edge_0[0] = new Edge(from, to, g.GetEdgeWeight(from, to));
                    from = (int)struct8_0.double_0[1, 2];
                    to = (int)struct8_0.double_0[2, 1];
                    struct8_0.edge_0[1] = new Edge(from, to, g.GetEdgeWeight(from, to));
                }
                else
                {
                    int from = (int)struct8_0.double_0[0, 2];
                    int to = (int)struct8_0.double_0[2, 1];
                    struct8_0.edge_0[0] = new Edge(from, to, g.GetEdgeWeight(from, to));
                    from = (int)struct8_0.double_0[1, 2];
                    to = (int)struct8_0.double_0[2, 0];
                    struct8_0.edge_0[1] = new Edge(from, to, g.GetEdgeWeight(from, to));
                }
                lock (mutex)
                {
                    if (struct8_0.double_0[2, 2] < weight)
                    {
                        weight = struct8_0.double_0[2, 2];
                        edge_0 = (Edge[])struct8_0.edge_0.Clone();
                    }
                }
            }
        }
    }
}
