using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    public interface IAbstractDictionary<K, V> : IEnumerable<KeyValuePair<K, V>>
    {
        int Count { get; }
        
        void SetAccess(Action access);
        
        bool Insert(K k, V v);
        
        bool Remove(K k);
        
        bool Modify(K key, V value);

        bool Search(K key, out V value);
    }
}
