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
        /// <summary>
        /// Wyznacza najkrótszą ścieżkę do wskazanego wierzchołka algorytmem A*
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="s">Wierzchołek źródłowy</param>
        /// <param name="t">Wierzchołek docelowy</param>
        /// <param name="p">Znaleziona ścieżka (parametr wyjściowy)</param>
        /// <param name="h">Oszacowanie odległości wierzchołków (funkcja)</param>
        /// <returns>Informacja czy ścieżka do wierzchołka docelowego istnieje</returns>
        /// <remarks>
        /// Domyślna wartość parametru h (null) oznacza, że zostanie przyjęte oszacowanie zerowe.
        /// Algorytm A* sprowadza się wówczas do algorytmu Dijkstry.<para/>
        /// Metoda nie bada spełnienia założeń algorytmu A* - jeśli nie one są spełnione
        /// może zwrócić błędny wynik (nieoptymalną ścieżkę).<para/>
        /// Informacja, czy szukana ścieżka istnieje, zawsze jest zwracana poprawnie.
        /// Jeśli nie istnieje (wynik false), to parametr p jest równy null.
        /// </remarks>
        /// <seealso cref="AStarGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
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
