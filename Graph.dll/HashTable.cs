using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASD.Graphs
{
    /// <summary>
    /// Tablica haszowana
    /// </summary>
    /// <typeparam name="TKey">Typ kluczy elementów przechowywanych w tablicy</typeparam>
    /// <typeparam name="TValue">Typ wartości elementów przechowywanych w tablicy</typeparam>
    /// <remarks>Wartości kluczy muszą być unikalne.</remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class HashTable<TKey, TValue> : IAbstractDictionary<TKey, TValue>
    {
        private const int Empty = 0;
        private const int Used = 1;
        private const int Deleted = -1;

        private int notEmptyCount;
        private (int flag, (TKey key, TValue value) keyValueTuple)[] table;

        [NonSerialized]
        private Action access;
        private string serializedAccess;
        
        /// <summary>
        /// Tworzy pustą tablicę haszowaną
        /// </summary>
        /// <param name="access">Delegacja wywoływana przy każdym dostępie do elementu wewnętrznej tablicy</param>
        /// <param name="capacity">Początkowy rozmiar tablicy</param>
        /// <remarks>
        /// Tablica automatycznie rozrasta się w miarę potrzeby.<para/>
        /// Parametr access umożliwia stworzenie licznika odwołań (czyli eksperymentalne badanie wydajności).<para/>
        /// Wartość domyślna (null) tego parametru oznacza metodę pustą (nie wykonującą żadnych działań).<para/>
        /// Zwrotu "delegacja wywoływana przy każdym dostępie do elementu wewnętrznej tablicy"
        /// nie należy rozumieć dosłownie, ale liczba wywołań delegacji jest tego samego rzędu co liczba dostępów.<para/>
        /// </remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public HashTable(Action access = null, int capacity = 8)
        {
            this.access = access ?? CMonDoSomething.Nothing;
            table = new (int state, (TKey key, TValue value))[capacity];
            Count = 0;
        }

        /// <summary>
        /// Liczba elementów tablicy haszowanej
        /// </summary>
        /// <remarks>Liczba elementów jest odpowiednio modyfikowana przez metody <see cref="Insert"/>
        /// i <see cref="Remove"/>.</remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public int Count
        {
            get;
            private set;
        }

        /// <summary>
        /// Indeksator
        /// </summary>
        /// <param name="key">Klucz elementu</param>
        /// <exception cref="Exception">Jeśli nie ma takiego elementu</exception>
        /// <remarks>
        /// Indeksator get zwraca wartość elementu o zadanym kluczu.
        /// Jeśli nie ma takiego elementu zgłasza wyjątek ArgumentException.<para/>
        /// Indeksator set modyfikuje wartość elementu o zadanym kluczu (jeśli taki istnieje)
        /// lub dodaje element (jeśli nie było elementu o zadanym kluczu).<para/>
        /// Indeksator set może spowodować podwojenie rozmiaru tablicy.<para/>
        /// Jeśli wiadomo, że elementu nie ma w tablicy zamiast
        /// indeksatora set wydajniej jest wykorzystać metodę Insert.
        /// </remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public TValue this[TKey key]
        {
            get
            {
                if (!Search(key, out var result))
                    throw new Exception("Element not found");
                return result;
            }
            set
            {
                if (!Modify(key, value))
                    Insert(key, value);
            }
        }

        /// <summary>
        /// Ustawia delegację wywoływaną przy każdym dostępie do elementu wewnętrznej tablicy
        /// </summary>
        /// <param name="access">Delegacja wywoływana przy każdym dostępie do elementu wewnętrznej tablicy</param>
        /// <remarks>
        /// Metoda umożliwia stworzenie licznika odwołań (czyli eksperymentalne badanie wydajności).<para/>
        /// Wartość (null) parametru access oznacza metodę pustą (nie wykonującą żadnych działań).<para/>
        /// Zwrotu "delegacja wywoływana przy każdym dostępie do elementu wewnętrznej tablicy"
        /// nie należy rozumieć dosłownie, ale liczba wywołań delegacji jest tego samego rzędu co liczba dostępów.
        /// </remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public void SetAccess(Action access)
        {
            this.access = access ?? CMonDoSomething.Nothing;
        }

        /// <summary>
        /// Wstawia element do tablicy haszowanej
        /// </summary>
        /// <param name="k">Klucz wstawianego elementu</param>
        /// <param name="v">Wartość wstawianego elementu</param>
        /// <returns>Informacja czy wstawianie powiodło się</returns>
        /// <remarks>
        /// Metoda zwraca false gdy element o wskazanym kluczu już wcześniej był w tablicy.<para/>
        /// Metoda może spowodować podwojenie rozmiaru wewnętrznej tablicy.
        /// </remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Insert(TKey k, TValue v)
        {
            var (i, d) = FindSlot(k);
            access();
            if (table[i].flag == Used)
                return false;
            Count++;
            if (table[d].flag == Empty) notEmptyCount++;
            table[d].flag = Used;
            table[d].keyValueTuple.key = k;
            table[d].keyValueTuple.value = v;
            if (!(notEmptyCount >= 0.875 * table.Length)) return true;
            var oldTable = table;
            Count = 0;
            notEmptyCount = 0;
            table = new (int state, (TKey key, TValue value))[table.Length << 1];
            for (var j = 0; j< oldTable.Length; j++)
                if (oldTable[j].flag == Used)
                    Insert(oldTable[j].keyValueTuple.key, oldTable[j].keyValueTuple.value);
            return true;
        }

        /// <summary>
        /// Usuwa element z tablicy haszowanej
        /// </summary>
        /// <param name="k">Klucz usuwanego elementu</param>
        /// <returns>Informacja czy usuwanie powiodło się</returns>
        /// <remarks>,
        /// Metoda zwraca false gdy elementu o wskazanym kluczu nie było w tablicy.<para/> 
        /// Metoda nigdy nie powodowuje zmniejszenia rozmiaru wewnętrznej tablicy.
        /// </remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Remove(TKey k)
        {
            var item = FindSlot(k).i;
            access();
            if (table[item].flag == Empty)
                return false;
            table[item].flag = Deleted;
            Count--;
            return true;
        }

        /// <summary>
        /// Zmienia wartość elementu o wskazanym kluczu
        /// </summary>
        /// <param name="key">Klucz zmienianego elementu</param>
        /// <param name="value">Nowa wartość elementu</param>
        /// <returns>Informacja czy modyfikacja powiodła się</returns>
        /// <remarks>,Metoda zwraca false gdy elementu o wskazanym kluczu nie ma w tablicy.</remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Modify(TKey key, TValue value)
        {
            var item = FindSlot(key).Item1;
            access();
            if (table[item].flag == Empty)
                return false;
            table[item].keyValueTuple.value = value;
            return true;
        }

        /// <summary>
        /// Wyszukuje element w tablicy haszowanej
        /// </summary>
        /// <param name="key">Klucz szukanego elementu</param>
        /// <param name="value">Wartość szukanego elementu (parametr wyjściowy)</param>
        /// <returns>Informacja czy wyszukiwanie powiodło się</returns>
        /// <remarks>
        /// Metoda zwraca false gdy elementu o wskazanym kluczu nie ma w tablicy,
        /// Parametr v otrzymuje wówczas wartość domyślną dla typu T.
        /// </remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Search(TKey key, out TValue value)
        {
            var item = FindSlot(key).Item1;
            access();
            if (table[item].flag == Empty)
            {
                value = default;
                return false;
            }
            value = table[item].keyValueTuple.value;
            return true;
        }

        /// <summary>
        /// Wylicza wszystkie elementy tablicy haszowanej
        /// </summary>
        /// <returns>Żądane wyliczenie elementów</returns>
        /// <remarks>
        /// Metoda umożliwia przeglądanie tablicy haszowanej przy pomocy instrukcji foreach.<para/>
        /// Metoda jest wymagana przez interfejs <see cref="IEnumerable"/>.
        /// </remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            return new HashTableEnumerator(this);
        }

        /// <summary>
        /// Wylicza wszystkie elementy tablicy haszowanej
        /// </summary>
        /// <returns>Żądane wyliczenie elementów</returns>
        /// <remarks>
        /// Metoda umożliwia przeglądanie tablicy haszowanej przy pomocy instrukcji foreach.<para/>
        /// Metoda jest wymagana przez interfejs <see cref="IEnumerable"/>.
        /// </remarks>
        /// <seealso cref="HashTable{TKey,TValue}"/>
        /// <seealso cref="ASD.Graphs"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext streamingContext)
        {
            serializedAccess = DelegateSerializationHelper.Serialize(access);
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext streamingContext)
        {
            serializedAccess = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            access = (Action)DelegateSerializationHelper.Deserialize(serializedAccess);
            serializedAccess = null;
        }
        
        private (int i, int d) FindSlot(TKey key)
        {
            var i = HashFunction(key);
            var shift = 1;
            var d = -1;
            while (table[i].flag != Empty)
            {
                if (table[i].flag == Used && table[i].keyValueTuple.key.Equals(key))
                    break;
                
                if (table[i].flag == Deleted && d == -1)
                    d = i;
                
                access();
                i = (i + shift++) % table.Length;
            }
            if (d == -1) d = i;
            return (i, d);
        }

        private int HashFunction(TKey key)
        {
            var num = (key.GetHashCode() & int.MaxValue) * 0.6180339887;//(sqrt(5)-1)/2
            num -= Math.Truncate(num);
            return (int)(table.Length * num);
        }

        [Serializable]
        private sealed class HashTableEnumerator : IEnumerator<KeyValuePair<TKey, TValue>>
        {
            private (int state, (TKey key, TValue value)) _current;
            private HashTable<TKey, TValue> hashTable;
            int current, found;

            public HashTableEnumerator(HashTable<TKey, TValue> ht)
            {
                hashTable = ht;
                current = -1;
                found = 0;
            }

            public bool MoveNext()
            {
                for (var i = current + 1; i < hashTable.table.Length && found < hashTable.Count; i++)
                {
                    if (hashTable.table[i].flag != Used) continue;
                    current = i;
                    found++;
                    _current = hashTable.table[i];
                    return true;
                }

                return false;
            }

            public void Reset()
            {
                current = - 1;
            }

            public KeyValuePair<TKey, TValue> Current => new KeyValuePair<TKey, TValue>(_current.Item2.key, _current.Item2.value);

            object IEnumerator.Current => Current;

            public void Dispose()
            {
            }

        }
    }
}