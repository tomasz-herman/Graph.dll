using System;

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
        /// <summary>
        /// Initializes a new instance of the HashTableAdjacencyList class
        /// </summary>
        /// <seealso cref="HashTableAdjacencyList"/>
        /// <seealso cref="ASD.Graphs"/>
        public HashTableAdjacencyList() : base(null, 8)
        {
        }
    }
}
