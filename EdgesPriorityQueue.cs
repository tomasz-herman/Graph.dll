using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Kolejka priorytetowa krawędzi
    /// </summary>
    /// <remarks>
    /// Kryterium porównywania krawędzi należy podać jako parametr konstruktora.<para/>
    /// Implementacja za pomocą kopca.<para/>
    /// Zwykle lepiej jest uzyć jednej z klas pochodnych (<see cref="EdgesMinPriorityQueue"/> lub <see cref="EdgesMaxPriorityQueue"/>).
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class EdgesPriorityQueue : IEdgesContainer
    {
        private readonly PriorityQueue<Edge, double> _queue;
        
        /// <summary>
        /// Tworzy pustą kolejkę priorytetową krawędzi
        /// </summary>
        /// <param name="cmp">Kryterium porównywania krawędzi</param>
        /// <remarks>Kryterium powinno zwracać true jeśli lewy argument porównania ma lepszy priorytet niż prawy.</remarks>
        /// <seealso cref="EdgesPriorityQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public EdgesPriorityQueue(Func<KeyValuePair<Edge, double>, KeyValuePair<Edge, double>, bool> cmp)
        {
            _queue = new PriorityQueue<Edge, double>(cmp, CMonDoSomething.Nothing);
        }
        
        /// <summary>
        /// Informacja czy kolejka jest pusta (właściwość tylko do odczytu)
        /// </summary>
        /// <seealso cref="EdgesPriorityQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Empty => _queue.Empty;

        /// <summary>
        /// Liczba elementów kolejki (właściwość tylko do odczytu)
        /// </summary>
        /// <remarks>
        /// Liczba elementów kolejki jest odpowiednio modyfikowana przez metody <see cref="Put"/> i <see cref="Get"/>.
        /// </remarks>
        /// <seealso cref="EdgesPriorityQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public int Count => _queue.Count;

        /// <summary>
        /// Wstawia element do kolejki
        /// </summary>
        /// <param name="e">Wstawiany element</param>
        /// <remarks>Priorytetem jest waga krawędzi.</remarks>
        /// <seealso cref="EdgesPriorityQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public void Put(Edge e)
        {
            _queue.Put(e, e.Weight);
        }

        /// <summary>
        /// Pobiera z kolejki element o najlepszym priorytecie
        /// </summary>
        /// <returns>Pobrany element</returns>
        /// <remarks>Pobrany element jest usuwany z kolejki.</remarks>
        /// <seealso cref="EdgesPriorityQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge Get()
        {
            return _queue.Get();
        }

        /// <summary>
        /// Podgląda element kolejki o najlepszym priorytecie
        /// </summary>
        /// <returns>Element kolejki o najlepszym priorytecie</returns>
        /// <remarks>Element pozostaje w kolejce.</remarks>
        /// <seealso cref="EdgesPriorityQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge Peek()
        {
            return _queue.Peek();
        }

        /// <summary>
        /// Kopiuje elementy kolejki do tablicy
        /// </summary>
        /// <returns>Tablica zawierająca wszystkie elementy kolejki</returns>
        /// <remarks>Elementy nie są uporządkowane według priorytetów.</remarks>
        /// <seealso cref="EdgesPriorityQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge[] ToArray()
        {
            return _queue.ToArray();
        }
        
    }
}
