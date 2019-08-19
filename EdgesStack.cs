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
        
        public EdgesStack()
        {
            _stack = new Stack<Edge>();
        }
        
        public bool Empty => _stack.Count == 0;

        public int Count => _stack.Count;

        public void Put(Edge e)
        {
            _stack.Push(e);
        }
        
        public Edge Get()
        {
            return _stack.Pop();
        }
        
        public Edge Peek()
        {
            return _stack.Peek();
        }
        
        public Edge[] ToArray()
        {
            var array = new Edge[_stack.Count];
            _stack.CopyTo(array, 0);
            return array;
        }

    }
}
