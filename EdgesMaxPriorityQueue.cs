using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Kolejka priorytetowa krawędzi, lepszy priorytet mają krawędzie o większej wadze
    /// </summary>
    /// <remarks>
    /// Implementacja za pomocą kopca.<para/>
    /// W przypadku równych wag porównywane są numery wierzchołków (najpierw początkowych, potem końcowych) - lepsze są mniejsze numery.
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class EdgesMaxPriorityQueue : EdgesPriorityQueue
    {
        /// <summary>
        /// Tworzy pustą kolejkę priorytetową krawędzi
        /// </summary>
        /// <seealso cref="EdgesMaxPriorityQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public EdgesMaxPriorityQueue():base(Cmp)
        {
            
        }

        private static bool Cmp(KeyValuePair<Edge, double> kvp1, KeyValuePair<Edge, double> kvp2)
        {
            if (kvp1.Value != kvp2.Value)
            {
                return kvp1.Value > kvp2.Value;
            }
            return kvp1.Key < kvp2.Key;
        }
    }
}
