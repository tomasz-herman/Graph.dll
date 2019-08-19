using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Interfejs opisujący słownik (abstrakcyjny typ danych)
    /// </summary>
    /// <typeparam name="K">Typ kluczy elementów przechowywanych w słowniku</typeparam>
    /// <typeparam name="V">Typ wartości elementów przechowywanych w słowniku</typeparam>
    /// <remarks>Wartości kluczy muszą być unikalne.</remarks>
    /// <seealso cref="ASD.Graphs"/>
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
