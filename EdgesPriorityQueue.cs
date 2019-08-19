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
        
        public EdgesPriorityQueue(Func<KeyValuePair<Edge, double>, KeyValuePair<Edge, double>, bool> cmp)
        {
            _queue = new PriorityQueue<Edge, double>(cmp, CMonDoSomething.Nothing);
        }
        
        public bool Empty => _queue.Empty;

        public int Count => _queue.Count;

        public void Put(Edge e)
        {
            _queue.Put(e, e.Weight);
        }

        public Edge Get()
        {
            return _queue.Get();
        }

        public Edge Peek()
        {
            return _queue.Peek();
        }

        public Edge[] ToArray()
        {
            return _queue.ToArray();
        }
        
    }
}
