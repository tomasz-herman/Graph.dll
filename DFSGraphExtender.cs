using System;
using System.Linq;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy Graph o przeszukiwanie w głąb (rekurencyjne)
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class DFSGraphExtender
    {
        /// <summary>
        /// Przeszukuje cały graf w głąb (rekurencyjne)
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="preVisit">Metoda wywoływana przy pierwszym wejściu do wierzchołka</param>
        /// <param name="postVisit">Metoda wywoływana przy ostatecznym opuszczaniu wierzchołka</param>
        /// <param name="cc">Liczba "spójnych składowych" grafu (parametr wyjściowy)</param>
        /// <param name="nr">Tablica kolejności "wierzchołków startowych"</param>
        /// <returns>Informacja czy zbadano wszystkie wierzchołki grafu</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Metoda odwiedza wszystkie wierzchołki grafu wykorzystując metodę
        /// <see cref="DFSearchFrom"/> (w razie potrzeby wywołuje ją wielokrotnie).<para/>
        /// Jako wierzchołek startowy dla metody <see cref="DFSearchFrom"/> wybierany jest
        /// pierwszy nieodwiedzony wierzchołek nr[i], gdzie tablica nr przeglądana
        /// jest w kierunku rosnących indeksów.<para/>
        /// Domyślna wartość parametru nr (null) oznacza, że tablica nr zostanie
        /// automatycznie wypełniona wartościami nr[i]=i (kolejność domyślna).<para/>
        /// Wartością parametru wyjściowego cc jest wykonana liczba wywołań metody <see cref="DFSearchFrom"/>
        /// (dla grafów nieskierowanych jest to liczba spójnych składowych grafu,
        /// dla grafów skierowanych oczywiście nie ma takiej zależności).<para/>
        /// Wartość parametru cc jest inkrementowana przed każdym wywołaniem metody <see cref="DFSearchFrom"/>.<para/>
        /// Metoda oczywiście odwiedza wierzchołki jednokrotnie, zwrot "ostateczne opuszczenie wierzchołka"
        /// oznacza moment, w którym mechamizm rekursji opuszcza wierzchołek.<para/>
        /// Parametrem metod preVisit i postVisit jest numer odwiedzanego wierzchołka.
        /// Wyniki zwracane przez metody preVisit i postVisit oznaczają
        /// żądanie kontynuowania (true) lub przerwania (false) obliczeń.<para/>
        /// Parametry preVisit i postVisit mogą mieć wartość null,
        /// co oznacza metodę pustą (nie wykonującą żadnych działań, zwracającą true).<para/>
        /// Jeśli zostały zbadane wszystkie wierzchołki grafu metoda zwraca true,
        /// jeśli obliczenia zostały przerwane przez metody związane
        /// z parametrami preVisit lub postVisit metoda zwraca false.<para/>
        /// Uwaga:<para/>
        /// Dla dużych grafów metoda może spowodować przepełnienie stosu
        /// (z powodu zbyt dużej liczby zagnieżdżonych wywołań rekurencyjnych).<para/>
        /// W takim przypadku można zastosować metodę
        /// <see cref="GeneralSearchGraphExtender.GeneralSearchAll{TEdgesContainer}"/>
        /// z wykorzystaniem kontenera <see cref="EdgesStack"/> (jawnego stosu).
        /// </remarks>
        /// <seealso cref="DFSGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool DFSearchAll(this Graph g, Predicate<int> preVisit, Predicate<int> postVisit, out int cc, int[] nr = null)
        {
            if (nr != null)
            {
                if (nr.Length != g.VerticesCount)
                    throw new ArgumentException("Invalid order table");
                
                var used = new bool[g.VerticesCount];
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    if (nr[i] < 0 || nr[i] >= g.VerticesCount || used[nr[i]])
                        throw new ArgumentException("Invalid order table");
                    used[nr[i]] = true;
                }
            }
            else
            {
                nr = new int[g.VerticesCount];
                for (var i = 0; i < g.VerticesCount; i++) nr[i] = i;
            }
            var visitedVertices = new bool[g.VerticesCount];
            cc = 0;
            for (var j = 0; j < g.VerticesCount; j++)
            {
                if (visitedVertices[nr[j]]) continue;
                cc++;
                if (!DFSearchFrom(g, nr[j], preVisit, postVisit, visitedVertices))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="from">Wierzchołek startowy</param>
        /// <param name="preVisit">Metoda wywoływana przy pierwszym wejściu do wierzchołka</param>
        /// <param name="postVisit">Metoda wywoływana przy ostatecznym opuszczaniu wierzchołka</param>
        /// <param name="visitedVertices">Tablica odwiedzonych wierzchołków</param>
        /// <returns>Informacja czy zbadano wszystkie wierzchołki osiągalne z wierzchołka startowego</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Metoda odwiedza jedynie te wierzchołki, dla których elementy tablicy visitedVertices mają wartość false
        /// (i zmienia wpisy w visitedVertices na true).<para/>
        /// Tablica visitedVertices musi mieć rozmiar zgodny z liczbą wierzchołków grafu g.<para/>
        /// Domyślna wartość null parametru visitedVertices oznacza tablicę wypełnioną
        /// wartościami false (zostanie ona utworzona wewnątrz metody).<para/>
        /// Metoda oczywiście odwiedza wierzchołki jednokrotnie, zwrot
        /// "ostateczne opuszczenie wierzchołka" oznacza moment,
        /// w którym mechamizm rekursji opuszcza wierzchołek.<para/>
        /// Parametrem metod preVisit i postVisit jest numer odwiedzanego wierzchołka.<para/>
        /// Wyniki zwracane przez metody preVisit i postVisit oznaczają
        /// żądanie kontynuowania (true) lub przerwania (false) obliczeń.<para/>
        /// Parametry preVisit i postVisit mogą mieć wartość null, co oznacza metodę pustą
        /// (nie wykonującą żadnych działań, zwracającą true).<para/>
        /// Jeśli zostały zbadane wszystkie wierzchołki osiągalne z wierzchołka startowego metoda zwraca true,
        /// jeśli obliczenia zostały przerwane przez metody związane z parametrami preVisit lub postVisit
        /// metoda zwraca false.<para/>
        /// Wynik true nie oznacza, że metoda odwiedziła wszystkie wierzchołki grafu
        /// (oznacza jedynie, że odwiedziła wszystkie wierzchołki osiągalne z wierzchołka startowego).<para/>
        /// Uwaga:<para/>
        /// Dla dużych grafów metoda może spowodować przepełnienie stosu
        /// (z powodu zbyt dużej liczby zagnieżdżonych wywołań rekurencyjnych).<para/>
        /// W takim przypadku można zastosować metodę
        /// <see cref="GeneralSearchGraphExtender.GeneralSearchAll{TEdgesContainer}"/> z wykorzystaniem
        /// kontenera <see cref="EdgesStack"/> (jawnego stosu).<para/>
        /// </remarks>
        /// <seealso cref="DFSGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool DFSearchFrom(this Graph g, int from, Predicate<int> preVisit, Predicate<int> postVisit, bool[] visitedVertices = null)
        {
            if (visitedVertices == null)
                visitedVertices = new bool[g.VerticesCount];
            
            else if (visitedVertices.Length != g.VerticesCount)
                throw new ArgumentException("Invalid visitedVertices length");
            
            if (visitedVertices[from])
                throw new ArgumentException("Start vertex is already visited");

            if (preVisit == null)
                preVisit = i => true;
            
            if (postVisit == null)
                postVisit = i => true;
            
            visitedVertices[from] = true;
            
            if (!preVisit(from))
                return false;
            
            return !g.OutEdges(from).Any(edge => !visitedVertices[edge.To] && !DFSearchFrom(g, edge.To, preVisit, postVisit, visitedVertices)) && postVisit(from);
        }
    }
}
