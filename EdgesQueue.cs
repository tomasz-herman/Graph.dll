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
        
        public EdgesQueue()
        {
            _queue = new Queue<Edge>();
        }

        public bool Empty => _queue.Count == 0;

        public int Count => _queue.Count;

        public void Put(Edge e)
        {
            _queue.Enqueue(e);
        }

        public Edge Get()
        {
            return _queue.Dequeue();
        }

        public Edge Peek()
        {
            return _queue.Peek();
        }

        public Edge[] ToArray()
        {
            var array = new Edge[_queue.Count];
            _queue.CopyTo(array, 0);
            return array;
        }

    }
}
