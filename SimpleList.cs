using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASD.Graphs
{
    /// <summary>
    /// Prosta lista elementów klucz-wartość
    /// </summary>
    /// <typeparam name="TKey">Typ kluczy elementów przechowywanych na liście</typeparam>
    /// <typeparam name="TValue">Typ wartości elementów przechowywanych na liście</typeparam>
    /// <remarks>Wartości kluczy muszą być unikalne.</remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class SimpleList<TKey, TValue> : IAbstractDictionary<TKey, TValue>
    {
        private readonly List<KeyValuePair<TKey, TValue>> _pairs;
        [NonSerialized]
        private Action _access;
        private string _serializedAccess;
        
        /// <summary>
        /// Tworzy pustą listę
        /// </summary>
        /// <param name="access">Delegacja wywoływana przy każdym dostępie do elementu wewnętrznej listy</param>
        /// <param name="capacity">Początkowy rozmiar listy</param>
        /// <remarks>
        /// Parametr access umożliwia stworzenie licznika odwołań (czyli eksperymentalne badanie wydajności).<para/>
        /// Wartość domyślna (null) tego parametru oznacza metodę pustą (nie wykonującą żadnych działań).<para/>
        /// Zwrotu "delegacja wywoływana przy każdym dostępie do elementu wewnętrznej listy"
        /// nie należy rozumieć dosłownie, ale liczba wywołań delegacji jest tego samego rzędu co liczba dostępów.
        /// </remarks>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public SimpleList(Action access = null, int capacity = 8)
        {
            SetAccess(access);
            _pairs = new List<KeyValuePair<TKey, TValue>>(capacity);
        }
        
        /// <summary>
        /// Liczba elementów listy
        /// </summary>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public int Count => _pairs.Count;

        /// <summary>
        /// Ustawia delegację wywoływaną przy każdym dostępie do elementu wewnętrznej listy
        /// </summary>
        /// <param name="access">Delegacja wywoływana przy każdym dostępie do elementu wewnętrznej listy</param>
        /// <remarks>
        /// Parametr access umożliwia stworzenie licznika odwołań (czyli eksperymentalne badanie wydajności).<para/>
        /// Wartość (null) parametru access oznacza metodę pustą (nie wykonującą żadnych działań).<para/>
        /// Zwrotu "delegacja wywoływana przy każdym dostępie do elementu wewnętrznej listy"
        /// nie należy rozumieć dosłownie, ale liczba wywołań delegacji jest tego samego rzędu co liczba dostępów.
        /// </remarks>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public void SetAccess(Action access)
        {
            _access = access ?? CMonDoSomething.Nothing;
        }

        /// <summary>
        /// Wstawia element do listy
        /// </summary>
        /// <param name="k">Klucz wstawianego elementu</param>
        /// <param name="v">Wartość wstawianego elementu</param>
        /// <returns>Informacja czy wstawianie powiodło się</returns>
        /// <remarks>Metoda zwraca false gdy element o wskazanym kluczu już wcześniej był na liście.</remarks>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Insert(TKey k, TValue v)
        {
            foreach (var t in _pairs)
            {
                _access();
                if (t.Key.Equals(k)) return false;
            }
            _access();
            _pairs.Add(new KeyValuePair<TKey, TValue>(k, v));
            return true;
        }

        /// <summary>
        /// Usuwa element z listy
        /// </summary>
        /// <param name="k">Klucz usuwanego elementu</param>
        /// <returns>Informacja czy usuwanie powiodło się</returns>
        /// <remarks>Metoda zwraca false gdy elementu o wskazanym kluczu nie było na liście.</remarks>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Remove(TKey k)
        {
            for (var i = 0; i < _pairs.Count; i++)
            {
                _access();
                if (!_pairs[i].Key.Equals(k)) continue;
                _pairs.RemoveAt(i);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Zmienia wartość elementu o wskazanym kluczu
        /// </summary>
        /// <param name="key">Klucz zmienianego elementu</param>
        /// <param name="value">Nowa wartość elementu</param>
        /// <returns>Informacja czy modyfikacja powiodła się</returns>
        /// <remarks>Metoda zwraca false gdy elementu o wskazanym kluczu nie ma na liście.</remarks>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Modify(TKey key, TValue value)
        {
            for (var i = 0; i < _pairs.Count; i++)
            {
                _access();
                if (!_pairs[i].Key.Equals(key)) continue;
                _pairs[i] = new KeyValuePair<TKey, TValue>(key, value);
                return true;
            }
            return false;
        }

        /// <summary>
        /// Wyszukuje element w liście
        /// </summary>
        /// <param name="key">Klucz szukanego elementu</param>
        /// <param name="value">Wartość szukanego elementu (parametr wyjściowy)</param>
        /// <returns>Informacja czy wyszukiwanie powiodło się</returns>
        /// <remarks>
        /// Metoda zwraca false gdy elementu o wskazanym kluczu nie ma na liscie,
        /// parametr v otrzymuje wówczas wartość domyślną dla typu T.
        /// </remarks>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Search(TKey key, out TValue value)
        {
            foreach (var t in _pairs)
            {
                _access();
                if (!t.Key.Equals(key)) continue;
                value = t.Value;
                return true;
            }
            value = default;
            return false;
        }

        /// <summary>
        /// Wylicza wszystkie elementy listy
        /// </summary>
        /// <returns>Żądane wyliczenie elementów</returns>
        /// <remarks>
        /// Metoda umożliwia przeglądanie listy przy pomocy instrukcji foreach.
        /// Metoda jest wymagana przez interfejs <see cref="IEnumerable"/>.
        /// </remarks>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return _pairs.GetEnumerator();
        }

        /// <summary>
        /// Wylicza wszystkie elementy listy
        /// </summary>
        /// <returns>Żądane wyliczenie elementów</returns>
        /// <remarks>
        /// Metoda umożliwia przeglądanie listy przy pomocy instrukcji foreach.
        /// Metoda jest wymagana przez interfejs <see cref="IEnumerable"/>.
        /// </remarks>
        /// <seealso cref="SimpleList{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext streamingContext)
        {
            _serializedAccess = DelegateSerializationHelper.Serialize(_access);
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext streamingContext)
        {
            _serializedAccess = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            _access = (Action) DelegateSerializationHelper.Deserialize(_serializedAccess);
            _serializedAccess = null;
        }
    }
}
