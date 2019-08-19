using System;
using System.Collections;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Lista incydencji implementowana jako tablica haszowana
    /// </summary>
    /// <remarks>Cała funkcjonalność pochodzi z typu bazowego <see cref="HashTable{TKey,TValue}"/>.</remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class HashTableAdjacencyList : HashTable<int, double>, IAdjacencyList
    {
        public HashTableAdjacencyList() : base(null, 8)
        {
        }
    }
}
