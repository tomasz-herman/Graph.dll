using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    [Serializable]
    public class EdgesMaxPriorityQueue : EdgesPriorityQueue
    {
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
