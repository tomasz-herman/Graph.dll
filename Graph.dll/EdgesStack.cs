using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Stos krawędzi
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class EdgesStack : IEdgesContainer
    {
        private readonly Stack<Edge> _stack;
        
        /// <summary>
        /// Tworzy pusty stos krawędzi
        /// </summary>
        /// <seealso cref="EdgesStack"/>
        /// <seealso cref="ASD.Graphs"/>
        public EdgesStack()
        {
            _stack = new Stack<Edge>();
        }
        
        /// <summary>
        /// Informacja czy stos jest pusty (właściwość tylko do odczytu)
        /// </summary>
        /// <seealso cref="EdgesStack"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool Empty => _stack.Count == 0;

        /// <summary>
        /// Liczba elementów stosu (właściwość tylko do odczytu)
        /// </summary>
        /// <remarks>
        /// Liczba elementów stosu jest odpowiednio modyfikowana przez metody <see cref="Put"/> i <see cref="Get"/>.
        /// </remarks>
        /// <seealso cref="EdgesStack"/>
        /// <seealso cref="ASD.Graphs"/>
        public int Count => _stack.Count;

        /// <summary>
        /// Wstawia element na stos
        /// </summary>
        /// <param name="e">Wstawiany element</param>
        /// <seealso cref="EdgesStack"/>
        /// <seealso cref="ASD.Graphs"/>
        public void Put(Edge e)
        {
            _stack.Push(e);
        }
        
        /// <summary>
        /// Pobiera element ze stosu
        /// </summary>
        /// <returns>Pobrany element</returns>
        /// <remarks>Pobrany element jest usuwany ze stosu.</remarks>
        /// <seealso cref="EdgesStack"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge Get()
        {
            return _stack.Pop();
        }
        
        /// <summary>
        /// Podgląda szczytowy element stosu
        /// </summary>
        /// <returns>Szczytowy element stosu</returns>
        /// <remarks>Element pozostaje na stosie.</remarks>
        /// <seealso cref="EdgesStack"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge Peek()
        {
            return _stack.Peek();
        }
        
        /// <summary>
        /// Kopiuje elementy stosu do tablicy
        /// </summary>
        /// <returns>Tablica zawierająca wszystkie elementy stosu</returns>
        /// <remarks>Kolejnośc elementów jest zgodna z głębokością na stosie (odległoscią od szczytu).</remarks>
        /// <seealso cref="EdgesStack"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge[] ToArray()
        {
            var array = new Edge[_stack.Count];
            _stack.CopyTo(array, 0);
            return array;
        }

    }
}
