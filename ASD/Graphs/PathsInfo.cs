using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Pomocnicza struktura ułatwiająca konstruowanie ścieżek w grafach
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public struct PathsInfo
    {
        /// <summary>
        /// Odległość
        /// </summary>
        /// <remarks>
        /// Odległość (suma wag wszystkich krawędzi wchodzących w skład ścieżki) od źródła do danego wierzchołka.<para/>
        /// Jeśli ścieżka od źródła do danego wierzchołka nie istnieje odległość ma wartość NaN.<para/>
        /// Dla źródła odległość ma wartość 0.
        /// </remarks>
        /// <seealso cref="PathsInfo"/>
        /// <seealso cref="ASD.Graphs"/>
        public double Dist;
        
        /// <summary>
        /// Ostatnia krawędź na ścieżce
        /// </summary>
        /// <remarks>
        /// Ostatnia krawędź na ścieżce od źródła do danego wierzchołka.<para/>
        /// Jeśli ścieżka od źródła do danego wierzchołka nie istnieje krawędź ma wartość null.<para/>
        /// Również dla źródła krawędź ma wartość null.
        /// </remarks>
        /// <seealso cref="PathsInfo"/>
        /// <seealso cref="ASD.Graphs"/>
        public Edge? Last;
        
        /// <summary>
        /// Konstruuje ścieżkę od źródła do danego wierzchołka
        /// </summary>
        /// <param name="s">Wierzchołek początkowy (źródło)</param>
        /// <param name="t">Wierzołek końcowy (cel)</param>
        /// <param name="pi">Tablica odległości od źródła</param>
        /// <exception cref="ArgumentException"></exception>
        /// <returns>Szukana ścieżka</returns>
        /// <remarks>
        /// Ścieżka reprezentowana jest jako tablica krawędzi,
        /// kolejne elementy tej tablicy to kolejne krawędzie na ścieżce.<para/>
        /// Jeśli ścieżka nie istnieje metoda zwraca null.<para/>
        /// Jeśli wierzchołek końcowy jest równy początkowemu metoda zwraca pustą (zeroelementową) tablicę.
        /// </remarks>
        /// <seealso cref="ConstructPath(int,int,PathsInfo[,])"/>
        /// <seealso cref="PathsInfo"/>
        /// <seealso cref="ASD.Graphs"/>
        public static Edge[] ConstructPath(int s, int t, PathsInfo[] pi)
        {
            if (pi[s].Dist != 0.0 || pi[s].Last != null)
                throw new ArgumentException("Incorrect paths infos (invalid source vertex)");
            
            if (pi[t].Dist.IsNaN())
                return null;
            
            if (s == t)
                return new Edge[0];
            
            var edgesStack = new EdgesStack();
            
            for (var vert = t; vert != s; vert = pi[vert].Last.Value.From)
                edgesStack.Put(pi[vert].Last.Value);
            
            return edgesStack.ToArray();
        }

        /// <summary>
        /// Konstruuje ścieżkę pomiędzy wskazaną parą wierzchołków
        /// </summary>
        /// <param name="s">Wierzchołek początkowy (źródło)</param>
        /// <param name="t">Wierzołek końcowy (cel)</param>
        /// <param name="pi">Tablica odległości</param>
        /// <returns>Szukana ścieżka</returns>
        /// <remarks>
        /// Ścieżka reprezentowana jest jako tablica krawędzi,
        /// kolejne elementy tej tablicy to kolejne krawędzie na ścieżce.<para/>
        /// Jeśli ścieżka nie istnieje metoda zwraca null.<para/>
        /// Jeśli wierzchołek końcowy jest równy początkowemu metoda zwraca pustą (zeroelementową) tablicę.
        /// </remarks>
        /// <seealso cref="ConstructPath(int,int,PathsInfo[])"/>
        /// <seealso cref="PathsInfo"/>
        /// <seealso cref="ASD.Graphs"/>
        public static Edge[] ConstructPath(int s, int t, PathsInfo[,] pi)
        {
            if (pi[s, t].Dist.IsNaN())
                return null;
            
            if (s == t)
                return new Edge[0];
            
            var edgesStack = new EdgesStack();
            for (var vert = t; vert != s; vert = pi[s, vert].Last.Value.From)
                edgesStack.Put(pi[s, vert].Last.Value);
            
            return edgesStack.ToArray();
        }
    }
}
