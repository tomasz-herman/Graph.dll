using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASD.Graphs
{
    /// <summary>
    /// Kolejka priorytetowa
    /// </summary>
    /// <typeparam name="TKey">Typ przechowywanych elementów (kluczy)</typeparam>
    /// <typeparam name="TPriority">Typ priorytetów</typeparam>
    /// <remarks>
    /// Kryterium porównywania elementów należy podać jako parametr konstruktora.<para/>
    /// Implementacja za pomocą kopca.
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class PriorityQueue<TKey, TPriority>
    {
        private List<KeyValuePair<TKey, TPriority>> elements;
        private HashTable<TKey, int> index;
        [NonSerialized]
        private Func<KeyValuePair<TKey, TPriority>, KeyValuePair<TKey, TPriority>, bool> cmp;
        private string serializedCmp;
        [NonSerialized]
        private Action access;
        private string serializedAccess;
        
        /// <summary>
        /// Tworzy pustą kolejkę priorytetową
        /// </summary>
        /// <param name="cmp">Kryterium porównywania elementów</param>
        /// <param name="access">Delegacja wywoływana przy każdym dostępie do elementu kolejki</param>
        /// <remarks>
        /// Kryterium powinno zwracać true jeśli lewy argument porównania ma lepszy priorytet niż prawy.<para/>
        /// Parametr access umożliwia stworzenie licznika odwołań (czyli eksperymentalne badanie wydajności).<para/>
        /// Wartość domyślna (null) tego parametru oznacza metodę pustą (nie wykonującą żadnych działań).<para/>
        /// Zwrotu "delegacja wywoływana przy każdym dostępie do elementu kolejki" nie należy rozumieć dosłownie,
        /// ale liczba wywołań delegacji jest tego samego rzędu co liczba dostępów.
        /// </remarks>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public PriorityQueue(Func<KeyValuePair<TKey, TPriority>, KeyValuePair<TKey, TPriority>, bool> cmp, Action access = null)
        {
            this.cmp = cmp;
            this.access = access ?? CMonDoSomething.Nothing;
            elements = new List<KeyValuePair<TKey, TPriority>>();
            index = new HashTable<TKey, int>(access);
        }
        
        /// <summary>
        /// Informacja czy kolejka priorytetowa jest pusta (właściwość tylko do odczytu)
        /// </summary>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Empty => elements.Count == 0;

        /// <summary>
        /// Liczba elementów kolejki priorytetowej (właściwość tylko do odczytu)
        /// </summary>
        /// <remarks>
        /// Liczba elementów jest odpowiednio modyfikowana przez metody <see cref="Put"/> i <see cref="Get"/>.
        /// </remarks>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public int Count => elements.Count;

        /// <summary>
        /// Informuje czy zadany element (klucz) należy do kolejki priorytetowej
        /// </summary>
        /// <param name="k">Szukany element (klucz)</param>
        /// <returns>Informacja czy zadany element (klucz) należy do kolejki priorytetowej</returns>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Contains(TKey k)
        {
            return index.Search(k, out _);
        }

        /// <summary>
        /// Wstawia element do kolejki priorytetowej
        /// </summary>
        /// <param name="k">Wstawiany element (klucz)</param>
        /// <param name="p">Priorytet wstawianego elementu</param>
        /// <returns>Informacja czy wstawianie powiodło się</returns>
        /// <remarks>
        /// Jeśli element o zadanym kluczu już jest w kolejce to metoda zwraca false,
        /// a element nie jest wstawiany (klucze elementów należących do kolejki są unikalne).
        /// </remarks>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Put(TKey k, TPriority p)
        {
            if (Contains(k))
                return false;
            var keyValuePair = new KeyValuePair<TKey, TPriority>(k, p);
            elements.Add(keyValuePair);
            var num = elements.Count - 1;
            var i = (elements.Count - 2) >> 1;
            while (i >= 0 && cmp(keyValuePair, elements[i]))
            {
                elements[num] = elements[i];
                index[elements[num].Key] = num;
                num = i;
                i = num - 1 >> 1;
                access();
            }
            elements[num] = keyValuePair;
            index[k] = num;
            access();
            return true;
        }

        /// <summary>
        /// Pobiera z kolejki element o najlepszym priorytecie
        /// </summary>
        /// <returns>Pobrany element (klucz)</returns>
        /// <remarks>Pobrany element jest usuwany z kolejki.</remarks>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public TKey Get()
        {
            var key = elements[0].Key;
            index.Remove(key);
            var keyValuePair = elements[elements.Count - 1];
            elements.RemoveAt(elements.Count - 1);
            var num = 0;
            var i = 1;
            while (i < elements.Count)
            {
                if (i + 1 < elements.Count)
                    if (cmp(elements[i + 1], elements[i]))
                        i++;
                access();
                if (!cmp(elements[i], keyValuePair))
                    break;
                elements[num] = elements[i];
                index[elements[num].Key] = num;
                num = i;
                i = (num << 1) + 1;
                access();
            }

            if (elements.Count <= 0) return key;
            elements[num] = keyValuePair;
            index[keyValuePair.Key] = num;
            access();
            return key;
        }

        /// <summary>
        /// Podgląda pierwszy element kolejki priorytetowej
        /// </summary>
        /// <returns>Pierwszy element kolejki</returns>
        /// <remarks>
        /// Metoda zwraca klucz elementu o najlepszym priorytecie.<para/>
        /// Element pozostaje w kolejce.
        /// </remarks>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public TKey Peek()
        {
            access();
            return elements[0].Key;
        }

        /// <summary>
        /// Podaje priorytet pierwszego elementu kolejki
        /// </summary>
        /// <returns>Priorytet pierwszego elementu kolejki</returns>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public TPriority BestPriority()
        {
            access();
            return elements[0].Value;
        }

        /// <summary>
        /// Poprawia priorytet elementu kolejki
        /// </summary>
        /// <param name="k">Klucz zadanego elementu</param>
        /// <param name="p">Nowa wartość priorytetu</param>
        /// <returns>Informacja czy priorytet został poprawiony</returns>
        /// <remarks>
        /// Metoda zwraca false jeśli nowa wartość priorytetu jest gorsza (ściślej nie lepsza)
        /// niż poprzednia (wprowadzenie zmiany byłoby pogorszeniem, a nie poprawą, priorytetu).<para/>
        /// Jeśli zadanego elementu nie ma w kolejce zgłaszany jest wyjątek.
        /// </remarks>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool ImprovePriority(TKey k, TPriority p)
        {
            var keyValuePair = new KeyValuePair<TKey, TPriority>(k, p);
            var num = index[k];
            access();
            if (!cmp(keyValuePair, elements[num]))
                return false;
            var i = num - 1 >> 1;
            while (i >= 0)
            {
                if (!cmp(keyValuePair, elements[i]))
                {
                    break;
                }
                elements[num] = elements[i];
                index[elements[num].Key] = num;
                num = i;
                i = num - 1 >> 1;
                access();
            }
            elements[num] = keyValuePair;
            index[k] = num;
            access();
            return true;
        }

        /// <summary>
        /// Kopiuje elementy kolejki priorytetowej do tablicy
        /// </summary>
        /// <returns>Tablica zawierająca wszystkie elementy kolejki</returns>
        /// <remarks>Elementy nie są uporządkowane według priorytetów.</remarks>
        /// <seealso cref="PriorityQueue{TKey,TPriority}"/>
        /// <seealso cref="ASD.Graphs"/>
        public TKey[] ToArray()
        {
            var array = new TKey[elements.Count];
            for (var i = 0; i < elements.Count; i++)
            {
                array[i] = elements[i].Key;
                access();
            }
            return array;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext streamingContext)
        {
            serializedCmp = DelegateSerializationHelper.Serialize(cmp);
            serializedAccess = DelegateSerializationHelper.Serialize(access);
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext streamingContext)
        {
            serializedCmp = null;
            serializedAccess = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            cmp = (Func<KeyValuePair<TKey, TPriority>, KeyValuePair<TKey, TPriority>, bool>)DelegateSerializationHelper.Deserialize(serializedCmp);
            access = (Action)DelegateSerializationHelper.Deserialize(serializedAccess);
            serializedCmp = null;
            serializedAccess = null;
        }

    }
}
