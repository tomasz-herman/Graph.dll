using System;
using System.Linq;
using System.Threading.Tasks;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy Graph o algorytm badania izomorfizmu grafów
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class IsomorphismGraphExtender
    {
        /// <summary>
        /// Bada czy zadane mapowanie wierzchołków definiuje izomorfizm grafów
        /// </summary>
        /// <param name="g">Pierwszy badany graf</param>
        /// <param name="h">Drugi badany graf</param>
        /// <param name="map">Zadane mapowanie wierzchołków</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Mapowanie wierzchołków zdefiniowane jest w ten sposób,
        /// że wierzchołkowi v w grafie g odpowiada wierzchołek map[v] w grafie h.<para/>
        /// Badana jest struktura grafu, sposób reprezentacji nie ma znaczenia.
        /// </remarks>
        /// <seealso cref="IsomorphismGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool IsIsomorphic(this Graph g, Graph h, int[] map)
        {
            if (g.VerticesCount != h.VerticesCount || g.EdgesCount != h.EdgesCount) return false;
            if (g.Directed != h.Directed) return false;
            
            if (map == null) throw new ArgumentException("Invalid mapping");
            if (map.Length != g.VerticesCount) throw new ArgumentException("Invalid mapping");
            var used = new bool[g.VerticesCount];
            for (var i = 0; i < g.VerticesCount; i++)
            {
                if (map[i] < 0 || map[i] >= g.VerticesCount || used[map[i]])
                    throw new ArgumentException("Invalid mapping");
                used[map[i]] = true;
            }
            
            for (var i = 0; i < g.VerticesCount; i++)
                if (g.OutDegree(i) != h.OutDegree(map[i]) || g.InDegree(i) != h.InDegree(map[i])) return false;
            
            for (var i = 0; i < g.VerticesCount; i++)
                if (g.OutEdges(i).Any(edge => edge.Weight != h.GetEdgeWeight(map[edge.From], map[edge.To])))
                    return false;
            
            return true;
        }

        /// <summary>
        /// Bada czy zadane mapowanie wierzchołków definiuje izomorfizm grafów
        /// </summary>
        /// <param name="g">Pierwszy badany graf</param>
        /// <param name="h">Drugi badany graf</param>
        /// <param name="map">Zadane mapowanie wierzchołków</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Mapowanie wierzchołków zdefiniowane jest w ten sposób,
        /// że wierzchołkowi v w grafie g odpowiada wierzchołek map[v] w grafie h.<para/>
        /// Badana jest struktura grafu, sposób reprezentacji nie ma znaczenia.<para/>
        /// Metoda wykonuje obliczenia równolegle w wielu wątkach.
        /// </remarks>
        /// <seealso cref="IsomorphismGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static bool IsIsomorphicParallel(this Graph g, Graph h, int[] map)
        {
            if (g.VerticesCount != h.VerticesCount) return false;
            if (g.EdgesCount != h.EdgesCount) return false;
            if (g.Directed != h.Directed) return false;
            if (map == null) throw new ArgumentException("Invalid mapping");
            if (map.Length != g.VerticesCount) throw new ArgumentException("Invalid mapping");
            var used = new bool[g.VerticesCount];
            for (var i = 0; i < g.VerticesCount; i++)
            {
                if (map[i] < 0 || map[i] >= g.VerticesCount || used[map[i]])
                    throw new ArgumentException("Invalid mapping");
                used[map[i]] = true;
            }
            
            for (var i = 0; i < g.VerticesCount; i++)
                if (g.OutDegree(i) != h.OutDegree(map[i]) || g.InDegree(i) != h.InDegree(map[i])) 
                    return false;
            
            var result = Parallel.For(0, g.VerticesCount, (i, state) =>
            {
                foreach (var edge in g.OutEdges(i))
                    if (edge.Weight != h.GetEdgeWeight(map[edge.From], map[edge.To]))
                        state.Stop();
            });
            return result.IsCompleted;
        }

        /// <summary>
        /// Bada izomorfizm grafów metodą pełnego przeglądu (backtracking)
        /// </summary>
        /// <param name="g">Pierwszy badany graf</param>
        /// <param name="h">Drugi badany graf</param>
        /// <returns>Znalezione mapowanie wierzchołków</returns>
        /// <remarks>
        /// Jeśli grafy są izomorficzne metoda zwraca mapowanie wierzchołków grafu g na wierzchołki grafu h,
        /// w przeciwnym przypadku metoda zwraca null<para/>
        /// Odpowiedniość wierzchołków zdefiniowana jest w ten sposób,
        /// że wierzchołkowi v w grafie g odpowiada wierzchołek map[v] w grafie h.<para/>
        /// Badana jest struktura grafu, sposób reprezentacji nie ma znaczenia.
        /// </remarks>
        /// <seealso cref="IsomorphismGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static int[] Isomorphism(this Graph g, Graph h)
        {
            if (g.VerticesCount != h.VerticesCount || g.EdgesCount != h.EdgesCount ||
                g.Directed != h.Directed) return null;

            var vertCount = g.VerticesCount;
            var mapping = new int[vertCount];
            var mapped = new bool[vertCount];
            var int_2 = new int[vertCount];
            var int_3 = new int[vertCount];
            for (var i = 0; i < vertCount; i++)
                int_3[i] = -1;
            var order = 0;
            var visited = new bool[vertCount];

            bool PreVisitVertex(int i)
            {
                visited[i] = true;
                int_2[order++] = i;
                return true;
            }

            bool VisitEdge(Edge edge)
            {
                if (!visited[edge.To] && int_3[edge.To] == -1) int_3[edge.To] = edge.From;
                return true;
            }

            g.GeneralSearchAll<EdgesStack>(PreVisitVertex, null, VisitEdge, out _);

            bool FindIsomorphism(int vert)
            {
                (int int_0 , int int_1) s;
                s.int_1 = vert;
                if (s.int_1 == vertCount)
                    return true;
                
                s.int_0 = int_2[s.int_1];
                if (int_3[s.int_0] != -1)
                    return h.OutEdges(mapping[int_3[s.int_0]]).Any(edge => Check(edge.To, ref s));
                
                for (var i = 0; i < vertCount; i++)
                    if (Check(i, ref s))
                        return true;
                return false;
            }
            
            bool Check(int int_4, ref (int int_0 , int int_1) pair)
            {
                if (mapped[int_4] || g.OutDegree(pair.int_0) != h.OutDegree(int_4) ||
                    g.InDegree(pair.int_0) != h.InDegree(int_4)) return false;
                for (var i = 0; i < pair.int_1; i++)
                {
                    var w1 = g.GetEdgeWeight(int_2[i], pair.int_0);
                    var w2 = h.GetEdgeWeight(mapping[int_2[i]], int_4);
                    if (w1 != w2 && (!w1.IsNaN() || !w2.IsNaN()))
                        return false;
                    
                    w1 = g.GetEdgeWeight(pair.int_0, int_2[i]);
                    w2 = h.GetEdgeWeight(int_4, mapping[int_2[i]]);
                    if (w1 != w2 && (!w1.IsNaN() || !w2.IsNaN()))
                        return false;
                    
                }
                mapped[int_4] = true;
                mapping[pair.int_0] = int_4;
                if (FindIsomorphism(pair.int_1 + 1))
                    return true;
                
                mapped[int_4] = false;
                return false;
            }
            
            return FindIsomorphism(0) ? mapping : null;
        }
        
    }
}
