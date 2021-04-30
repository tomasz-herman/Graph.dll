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
        /// <summary>
        /// Liczba elementów słownika
        /// </summary>
        /// <remarks>
        /// Liczba elementów jest odpowiednio modyfikowana przez metody <see cref="Insert"/> i <see cref="Remove"/>.
        /// </remarks>
        /// <seealso cref="IAbstractDictionary{K,V}"/>
        /// <seealso cref="ASD.Graphs"/>
        int Count { get; }
        
        /// <summary>
        /// Ustawia delegację wywoływaną przy każdym dostępie do elementów słownika
        /// </summary>
        /// <param name="access">Delegacja wywoływana przy każdym dostępie do elementu słownika</param>
        /// <remarks>
        /// Parametr access umożliwia stworzenie licznika odwołań (czyli eksperymentalne badanie wydajności).<para/>
        /// Wartość (null) parametru access oznacza metodę pustą (nie wykonującą żadnych działań).<para/>
        /// Uwaga: pojedyncza operacja na słowniku może spowodować konieczność zbadania wielu elementów
        /// słownika (czyli wielokrotnie zwiększyć licznik odwołań).
        /// </remarks>
        /// <seealso cref="IAbstractDictionary{K,V}"/>
        /// <seealso cref="ASD.Graphs"/>
        void SetAccess(Action access);
        
        /// <summary>
        /// Wstawia element do słownika
        /// </summary>
        /// <param name="k">Klucz wstawianego elementu</param>
        /// <param name="v">Wartość wstawianego elementu</param>
        /// <returns>Informacja czy wstawianie powiodło się</returns>
        /// <remarks>Metoda zwraca false gdy element o wskazanym kluczu już wcześniej był w słowniku.</remarks>
        /// <seealso cref="IAbstractDictionary{K,V}"/>
        /// <seealso cref="ASD.Graphs"/>
        bool Insert(K k, V v);
        
        /// <summary>
        /// Usuwa element ze słownika
        /// </summary>
        /// <param name="k">Klucz usuwanego elementu</param>
        /// <returns>Informacja czy usuwanie powiodło się</returns>
        /// <remarks>Metoda zwraca false gdy elementu o wskazanym kluczu nie było w słowniku.</remarks>
        /// <seealso cref="IAbstractDictionary{K,V}"/>
        /// <seealso cref="ASD.Graphs"/>
        bool Remove(K k);
        
        /// <summary>
        /// Zmienia wartość elementu o wskazanym kluczu
        /// </summary>
        /// <param name="key">Klucz zmienianego elementu</param>
        /// <param name="value">Nowa wartość elementu</param>
        /// <returns>Informacja czy modyfikacja powiodła się</returns>
        /// <remarks>Metoda zwraca false gdy elementu o wskazanym kluczu nie ma w słowniku.</remarks>
        /// <seealso cref="IAbstractDictionary{K,V}"/>
        /// <seealso cref="ASD.Graphs"/>
        bool Modify(K key, V value);

        /// <summary>
        /// Wyszukuje element w słowniku
        /// </summary>
        /// <param name="key">Klucz szukanego elementu</param>
        /// <param name="value">Wartość szukanego elementu (parametr wyjściowy)</param>
        /// <returns>Informacja czy wyszukiwanie powiodło się</returns>
        /// <remarks>
        /// Metoda zwraca false gdy elementu o wskazanym kluczu nie ma w słowniku,
        /// parametr v otrzymuje wówczas wartość domyślną dla typu T.
        /// </remarks>
        /// <seealso cref="IAbstractDictionary{K,V}"/>
        /// <seealso cref="ASD.Graphs"/>
        bool Search(K key, out V value);
    }
}
