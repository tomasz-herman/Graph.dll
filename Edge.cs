using System;
using System.Globalization;

namespace ASD.Graphs
{
    /// <summary>
    /// Struktura opisująca krawędź grafu
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public struct Edge
    {
        /// <summary>
        /// Numer wierzchołka początkowego krawędzi
        /// </summary>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public readonly int From;
        /// <summary>
        /// Numer wierzchołka końcowego krawędzi
        /// </summary>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public readonly int To;
        /// <summary>
        /// Waga krawędzi
        /// </summary>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public readonly double Weight;
        
        /// <summary>
        /// Tworzy krawędź
        /// </summary>
        /// <param name="from">Numer wierzchołka początkowego</param>
        /// <param name="to">Numer wierzchołka końcowego</param>
        /// <param name="weight">Waga</param>
        /// <remarks>W przypadku grafów nieważonych parametr weight należy pominąć</remarks>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge(int from, int to, double weight = 1.0)
        {
            From = from;
            To = to;
            Weight = weight;
        }

        /// <summary>
        /// Testowanie równości
        /// </summary>
        /// <param name="e1">Pierwsza porównywana krawędź</param>
        /// <param name="e2">Druga porównywana krawędź</param>
        /// <returns>Zwraca true jeśli argumenty są jednakowe, a false jeśli nie są jednakowe</returns>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool operator ==(Edge e1, Edge e2)
        {
            return e1.From == e2.From && e1.To == e2.To && e1.Weight == e2.Weight;
        }

        /// <summary>
        /// Testowanie różności
        /// </summary>
        /// <param name="e1">Pierwsza porównywana krawędź</param>
        /// <param name="e2">Druga porównywana krawędź</param>
        /// <returns>Zwraca true jeśli argumenty nie są jednakowe, a false jeśli są jednakowe</returns>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool operator !=(Edge e1, Edge e2)
        {
            return !(e1==e2);
        }

        /// <summary>
        /// Testowanie mniejszości
        /// </summary>
        /// <param name="e1">Pierwsza porównywana krawędź</param>
        /// <param name="e2">Druga porównywana krawędź</param>
        /// <returns>Zwraca true jeśli pierwszy argument jest ostro mniejszy niż drugi</returns>
        /// <remarks>
        /// Najpierw porównywane są numery wierzchołków początkowych, w razie równości numery wierzchołków końcowych,
        /// a w razie kolejnej równości wagi krawędzi.
        /// </remarks>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool operator <(Edge e1, Edge e2)
        {
            if (e1.From != e2.From)
            {
                return e1.From < e2.From;
            }
            if (e1.To != e2.To)
            {
                return e1.To < e2.To;
            }
            return e1.Weight < e2.Weight;
        }

        /// <summary>
        /// Testowanie większości
        /// </summary>
        /// <param name="e1">Pierwsza porównywana krawędź</param>
        /// <param name="e2">Druga porównywana krawędź</param>
        /// <returns>Zwraca true jeśli pierwszy argument jest ostro większy niż drugi</returns>
        /// <remarks>
        /// Najpierw porównywane są numery wierzchołków początkowych, w razie równości numery wierzchołków końcowych,
        /// a w razie kolejnej równości wagi krawędzi.
        /// </remarks>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool operator >(Edge e1, Edge e2)
        {
            return e2 < e1;
        }

        /// <summary>
        /// Testowanie równości
        /// </summary>
        /// <param name="e">Krawędź porównywana z bieżącą</param>
        /// <returns>Zwraca true jeśli bieżący obiekt i argument są jednakowe</returns>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public override bool Equals(object e)
        {
            return this == (Edge)e;
        }

        /// <summary>
        /// Oblicza skrót systemowy
        /// </summary>
        /// <returns>Wartość skrótu systemowego</returns>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public override int GetHashCode()
        {
            return From ^ To ^ Weight.GetHashCode();
        }

        /// <summary>
        /// Dostarcza tekstową reprezentację krawędzi
        /// </summary>
        /// <returns>Tekstowa reprezentacja krawędzi</returns>
        /// <seealso cref="Edge"/>
        /// <seealso cref="ASD.Graphs"/>
        public override string ToString()
        {
            IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
            return string.Format(invariantCulture, "<{0},{1},{2}>", From, To, Weight);
        }
    }
}
