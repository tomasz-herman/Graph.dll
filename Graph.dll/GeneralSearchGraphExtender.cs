using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o ogólne algorytmy przeszukiwania (nie rekurencyjne)
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class GeneralSearchGraphExtender
    {
        /// <summary>
        /// Przeszukuje cały graf
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="preVisitVertex">Metoda wywoływana przy pierwszym wejściu do wierzchołka</param>
        /// <param name="postVisitVertex">Metoda wywoływana przy ostatecznym opuszczaniu wierzchołka</param>
        /// <param name="visitEdge">Metoda wywoływana dla odwiedzanych krawędzi</param>
        /// <param name="cc">Liczba "spójnych składowych" grafu (parametr wyjściowy)</param>
        /// <param name="nr">Tablica kolejności "wierzchołków startowych"</param>
        /// <typeparam name="TEdgesContainer">Typ kolekcji przechowującej krawędzie</typeparam>
        /// <returns>Informacja czy zbadano wszystkie krawędzie grafu</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Metoda odwiedza wszystkie wierzchołki grafu wykorzystując metodę
        /// <see cref="GeneralSearchFrom{TEdgesContainer}"/> (w razie potrzeby wywołuje ją wielokrotnie).<para/>
        /// Jako wierzchołek startowy dla metody <see cref="GeneralSearchFrom{TEdgesContainer}"/>
        /// wybierany jest pierwszy nieodwiedzony wierzchołek nr[i],
        /// gdzie tablica nr przeglądana jest w kierunku rosnących indeksów.<para/>
        /// Domyślna wartość parametru nr (null) oznacza, że tablica nr zostanie automatycznie
        /// wypełniona wartościami nr[i]=i (kolejność domyślna).<para/>
        /// Wartością parametru wyjściowego cc jest wykonana liczba wywołań metody
        /// <see cref="GeneralSearchFrom{TEdgesContainer}"/> (dla grafów nieskierowanych jest to
        /// liczba spójnych składowych grafu, dla grafów skierowanych oczywiście nie ma takiej zależności).<para/>
        /// Wartość parametru cc jest inkrementowana przed każdym wywołaniem metody
        /// <see cref="GeneralSearchFrom{TEdgesContainer}"/>.<para/>
        /// Parametrem metod preVisitVertex i postVisitVertex jest numer odwiedzanego wierzchołka.<para/>
        /// Metoda preVisitVertex jest wywoływana gdy przeszukiwanie grafu osiąga dany wierzchołek
        /// (jest on już oznaczony jako odwiedzony, ale wychodzące z
        /// niego krawędzie nie są jeszcze dodane do kolekcji.<para/>
        /// Metoda postVisitVertex jest wywoływana gdy z kolekcji została usunięta
        /// (i w pełni przetworzona) ostatnia krawędź wychodząca z danego wierzchołka.<para/>
        /// Korzystanie z metody postVisitVertex jest dozwolone jedynie
        /// gdy kontenerem przechowującym krawędzie jest <see cref="EdgesStack"/> czyli
        /// dla przeszukiwania w głąb (DFS) z jawnym stosem
        /// (w innych przypadkach metoda zgłasza wyjątek <see cref="ArgumentException"/>).<para/>
        /// Metoda visitEdge jest wywoływana dla każdej krawędzi rozważanej
        /// podczas przeszukiwania (nawet jeśli nie została wykorzystana),
        /// krawędź ta jest parametrem metody visitEdge.<para/>
        /// Wyniki zwracane przez metody preVisitVertex, postVisitVertex
        /// i visitEdge oznaczają żądanie kontynuowania (true) lub przerwania (false) obliczeń.<para/>
        /// Parametry preVisitVertex, postVisitVertex i visitEdge mogą mieć wartość null,
        /// co oznacza metodę pustą (nie wykonującą żadnych działań, zwracającą true).<para/>
        /// Jeśli zostały zbadane wszystkie krawędzie grafu metoda zwraca true,
        /// jeśli obliczenia zostały przerwane przez metodę związaną z parametrem preVisitVertex,
        /// postVisitVertex lub visitEdge metoda zwraca false.
        /// </remarks>
        /// <seealso cref="GeneralSearchGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool GeneralSearchAll<TEdgesContainer>(this Graph g, Predicate<int> preVisitVertex, Predicate<int> postVisitVertex, Predicate<Edge> visitEdge, out int cc, int[] nr = null) where TEdgesContainer : IEdgesContainer, new()
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
            for (var i = 0; i < g.VerticesCount; i++)
            {
                if (visitedVertices[nr[i]]) continue;
                cc++;
                if (!g.GeneralSearchFrom<TEdgesContainer>(nr[i], preVisitVertex, postVisitVertex, visitEdge, visitedVertices))
                    return false;
            }
            return true;
        }

        /// <summary>
        /// Przeszukuje graf począwszy od wskazanego wierzchołka
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <param name="from">Wierzchołek startowy</param>
        /// <param name="preVisitVertex">Metoda wywoływana przy pierwszym wejściu do wierzchołka</param>
        /// <param name="postVisitVertex">Metoda wywoływana przy ostatecznym opuszczaniu wierzchołka</param>
        /// <param name="visitEdge">Metoda wywoływana dla odwiedzanych krawędzi</param>
        /// <param name="visitedVertices">Tablica odwiedzonych wierzchołków</param>
        /// <typeparam name="TEdgesContainer">Typ kolekcji przechowującej krawędzie</typeparam>
        /// <returns>Informacja czy zbadano wszystkie krawędzie osiągalne z wierzchołka startowego</returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Metoda odwiedza jedynie te wierzchołki, dla których elementy tablicy visitedVertices
        /// mają wartość false (i zmienia wpisy w visitedVertices na true).<para/>
        /// Tablica visitedVertices musi mieć rozmiar zgodny z liczbą wierzchołków grafu g.<para/>
        /// Domyślna wartość null parametru visitedVertices oznacza tablicę wypełnioną wartościami
        /// false (zostanie ona utworzona wewnątrz metody).<para/>
        /// Parametrem metod preVisitVertex i postVisitVertex jest numer odwiedzanego wierzchołka.<para/>
        /// Metoda preVisitVertex jest wywoływana gdy przeszukiwanie grafu osiąga dany wierzchołek
        /// (jest on już ozmaczony jako odwiedzony, ale wychodzące z niego krawędzie
        /// nie są jeszcze dodane do kolekcji.<para/>
        /// Metoda postVisitVertex jest wywoływana gdy z kolekcji została usunięta
        /// (i w pełni przetworzona) ostatnia krawędź wychodząca z danego wierzchołka.<para/>
        /// Korzystanie z metody postVisitVertex jest dozwolone jedynie
        /// gdy kontenerem przechowującym krawędzie jest <see cref="EdgesStack"/> czyli dla
        /// przeszukiwania w głąb (DFS) z jawnym stosem (w innych przypadkach
        /// metoda zgłasza wyjątek <see cref="ArgumentException"/>).<para/>
        /// Metoda visitEdge jest wywoływana dla każdej krawędzi rozważanej
        /// podczas przeszukiwania (nawet jeśli nie została wykorzystana),
        /// krawędź ta jest parametrem metody visitEdge.<para/>
        /// Wyniki zwracane przez metody preVisitVertex, postVisitVertex
        /// i visitEdge oznaczają żądanie kontynuowania (true) lub przerwania (false) obliczeń.<para/>
        /// Parametry preVisitVertex, postVisitVertex i visitEdge mogą mieć wartość null,
        /// co oznacza metodę pustą (nie wykonującą żadnych działań, zwracającą true).<para/>
        /// Jeśli zostały zbadane wszystkie krawędzie osiągalne z wierzchołka startowego metoda zwraca true,
        /// jeśli obliczenia zostały przerwane przez metodę związaną z parametrem preVisitVertex,
        /// postVisitVertex lub visitEdge metoda zwraca false.<para/>
        /// Wynik true nie oznacza, że metoda odwiedziła wszystkie krawędzie grafu
        /// (oznacza jedynie, że odwiedziła wszystkie krawędzie osiągalne z wierzchołka startowego).
        /// </remarks>
        /// <seealso cref="GeneralSearchGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool GeneralSearchFrom<TEdgesContainer>(this Graph g, int from, Predicate<int> preVisitVertex, Predicate<int> postVisitVertex, Predicate<Edge> visitEdge, bool[] visitedVertices = null) where TEdgesContainer : IEdgesContainer, new()
        {
            if (postVisitVertex != null && typeof(TEdgesContainer) != typeof(EdgesStack))
                throw new ArgumentException("Parameter postVisitVertex must be null for containers other than EdgesStack");
            
            var stack = new Stack<int>();
            var edgesContainer = Activator.CreateInstance<TEdgesContainer>();
            
            if (visitedVertices == null)
                visitedVertices = new bool[g.VerticesCount];
            else if (visitedVertices.Length != g.VerticesCount)
                throw new ArgumentException("Invalid visitedVertices length");
            
            if (visitedVertices[from])
                throw new ArgumentException("Start vertex is already visited");
            
            if (visitEdge == null)
                visitEdge = edge => true;
            
            if (preVisitVertex == null)
                preVisitVertex = i => true;
            
            visitedVertices[from] = true;
            stack.Push(from);
            
            if (!preVisitVertex(from))
                return false;
            
            if (postVisitVertex != null && g.OutDegree(from) == 0)
                return postVisitVertex(from);
            
            foreach (var edge in g.OutEdges(from))
                edgesContainer.Put(edge);
            
            while (true)
            {
                if (edgesContainer.Empty)
                    return true;
                
                var edge = edgesContainer.Get();
                
                if (!visitEdge(edge))
                    return false;
                
                if (!visitedVertices[edge.To])
                {
                    visitedVertices[edge.To] = true;
                    stack.Push(edge.To);
                    
                    if (!preVisitVertex(edge.To))
                        break;
                    
                    foreach (var e in g.OutEdges(edge.To))
                        edgesContainer.Put(e);
                }

                if (postVisitVertex == null) continue;
                while (edgesContainer.Empty || stack.Peek() != edgesContainer.Peek().From)
                {
                    if (!postVisitVertex(stack.Pop()))
                        return false;
                    if (stack.Count == 0)
                        break;
                }
            }
            return false;
        }
    }
}
