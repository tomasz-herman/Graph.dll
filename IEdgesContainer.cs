using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Interfejs opisujący kontenery na krawędzie
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public interface IEdgesContainer
    {
        bool Empty { get; }
        int Count { get; }
        void Put(Edge e);
        Edge Get();
        Edge Peek();
        Edge[] ToArray();
    }
}
