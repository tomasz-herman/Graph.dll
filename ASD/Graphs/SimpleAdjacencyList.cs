using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Lista incydencji implementowana jako prosta lista
    /// </summary>
    /// <remarks>Cała funkcjonalność pochodzi z typu bazowego <see cref="SimpleList{TKey,TValue}"/>.</remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class SimpleAdjacencyList : SimpleList<int, double>, IAdjacencyList
    {
    }
}
