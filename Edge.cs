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
        public readonly int From;
        /// <summary>
        /// Numer wierzchołka końcowego krawędzi
        /// </summary>
        public readonly int To;
        /// <summary>
        /// Waga krawędzi
        /// </summary>
        public readonly double Weight;
        
        public Edge(int from, int to, double weight = 1.0)
        {
            From = from;
            To = to;
            Weight = weight;
        }

        public static bool operator ==(Edge e1, Edge e2)
        {
            return e1.From == e2.From && e1.To == e2.To && e1.Weight == e2.Weight;
        }

        public static bool operator !=(Edge e1, Edge e2)
        {
            return !(e1==e2);
        }

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

        public static bool operator >(Edge e1, Edge e2)
        {
            return e2 < e1;
        }

        public override bool Equals(object e)
        {
            return this == (Edge)e;
        }

        public override int GetHashCode()
        {
            return From ^ To ^ Weight.GetHashCode();
        }

        public override string ToString()
        {
            IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
            return string.Format(invariantCulture, "<{0},{1},{2}>", From, To, Weight);
        }
    }
}
