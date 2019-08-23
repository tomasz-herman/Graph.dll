using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Kolejka krawędzi
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class EdgesQueue : IEdgesContainer
    {
        private readonly Queue<Edge> _queue;
        
        /// <summary>
        /// Tworzy pustą kolejkę krawędzi
        /// </summary>
        /// <seealso cref="EdgesQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public EdgesQueue()
        {
            _queue = new Queue<Edge>();
        }

        /// <summary>
        /// Informacja czy kolejka jest pusta (właściwość tylko do odczytu)
        /// </summary>
        /// <seealso cref="EdgesQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Empty => _queue.Count == 0;

        /// <summary>
        /// Liczba elementów kolejki (właściwość tylko do odczytu)
        /// </summary>
        /// <remarks>
        /// Liczba elementów kolejki jest odpowiednio modyfikowana przez metody <see cref="Put"/> i <see cref="Get"/>.
        /// </remarks>
        /// <seealso cref="EdgesQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public int Count => _queue.Count;

        /// <summary>
        /// Wstawia element do kolejki
        /// </summary>
        /// <param name="e">Wstawiany element</param>
        /// <seealso cref="EdgesQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public void Put(Edge e)
        {
            _queue.Enqueue(e);
        }

        /// <summary>
        /// Pobiera element z kolejki
        /// </summary>
        /// <returns>Pobrany element</returns>
        /// <remarks>Pobrany element jest usuwany z kolejki.</remarks>
        /// <seealso cref="EdgesQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge Get()
        {
            return _queue.Dequeue();
        }

        /// <summary>
        /// Podgląda pierwszy element kolejki
        /// </summary>
        /// <returns>Pierwszy element kolejki</returns>
        /// <remarks>Element pozostaje w kolejce.</remarks>
        /// <seealso cref="EdgesQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge Peek()
        {
            return _queue.Peek();
        }

        /// <summary>
        /// Kopiuje elementy kolejki do tablicy
        /// </summary>
        /// <returns>Tablica zawierająca wszystkie elementy kolejki</returns>
        /// <remarks>Kolejnośc elementów jest taka sama jak w kolejce.</remarks>
        /// <seealso cref="EdgesQueue"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge[] ToArray()
        {
            var array = new Edge[_queue.Count];
            _queue.CopyTo(array, 0);
            return array;
        }

    }
}
