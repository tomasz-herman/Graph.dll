using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o algorytm A*
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class AStarGraphExtender
    {
        public static bool AStar(this Graph g, int s, int t, out Edge[] p, Func<int, int, double> h = null)
        {
            bool Cmp(KeyValuePair<int, double> keyValuePair1, KeyValuePair<int, double> keyValuePair2)
            {
                return keyValuePair1.Value != keyValuePair2.Value
                    ? keyValuePair1.Value < keyValuePair2.Value
                    : keyValuePair1.Key < keyValuePair2.Key;
            }
            var priorityQueue = new PriorityQueue<int, double>(Cmp, CMonDoSomething.Nothing);
            var hashSet = new HashSet<int>();
            var hashTable = new HashTable<int, double>(CMonDoSomething.Nothing);
            var hashTable2 = new HashTable<int, Edge>(CMonDoSomething.Nothing);
            if (h == null)
                h = (i, j) => 0.0;
            p = null;
            priorityQueue.Put(s, 0.0);
            hashTable[s] = 0.0;
            hashTable2[s] = new Edge(s, s, 0.0);
            var num = -1;
            while (!priorityQueue.Empty)
            {
                num = priorityQueue.Get();
                hashSet.Add(num);
                if (num == t)
                    break;
                foreach (var edge in g.OutEdges(num))
                {
                    if (hashSet.Contains(edge.To)) continue;
                    if (!priorityQueue.Contains(edge.To))
                    {
                        priorityQueue.Put(edge.To, double.PositiveInfinity);
                        hashTable[edge.To] = double.PositiveInfinity;
                    }
                    if (!(hashTable[edge.To] > hashTable[num] + edge.Weight)) continue;
                    hashTable[edge.To] = hashTable[num] + edge.Weight;
                    hashTable2[edge.To] = edge;
                    priorityQueue.ImprovePriority(edge.To, hashTable[edge.To] + h(edge.To, t));
                }
            }
            if (num != t)
                return false;
            var edgesStack = new EdgesStack();
            for (var num3 = t; num3 != s; num3 = hashTable2[num3].From) edgesStack.Put(hashTable2[num3]);
            p = edgesStack.ToArray();
            return true;
        }
        
    }
}
