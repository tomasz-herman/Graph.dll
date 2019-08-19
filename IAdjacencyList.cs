using System;
using System.Collections;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Interfejs opisujący listy incydencji (listy sąsiedztwa) wierzchołków grafu
    /// </summary>
    /// <remarks>
    /// Elementy słownika odpowiadają krawędziom wychodzącym z danego wiedzchołka grafu.<para/>
    /// Klucze elementów słownika to numery wierzchołków docelowych krawędzi, a wartości elementów słownika to wagi tych krawędzi
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    public interface IAdjacencyList : IAbstractDictionary<int, double>
    {
    }
}
