using System;
using System.Collections;
using System.Collections.Generic;

namespace ASD.Graphs
{
    [Serializable]
    public class HashTableAdjacencyList : HashTable<int, double>, IAbstractDictionary<int, double>, IEnumerable<KeyValuePair<int, double>>, IEnumerable, IAdjacencyList
    {
        public HashTableAdjacencyList() : base(null, 8)
        {
        }
    }
}
