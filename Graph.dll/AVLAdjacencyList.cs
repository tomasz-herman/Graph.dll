using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Lista incydencji implementowana jako drzewo AVL
    /// </summary>
    /// <remarks>
    /// Cała funkcjonalność pochodzi z typu bazowego <see cref="AVL{TKey,TValue}"/>.
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public class AVLAdjacencyList : AVL<int, double>, IAdjacencyList
    {
    }
}
