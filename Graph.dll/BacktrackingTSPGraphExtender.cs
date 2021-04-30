using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o rozwiązywanie problemu komiwojażera metodą pełnego przeglądu (backtracking)
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class BacktrackingTSPGraphExtender
    {
        /// <summary>
        /// Znajduje rozwiązanie dokładne problemu komiwojażera metodą pełnego przeglądu (backtracking)
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <returns>
        /// Krotka (weight, cycle) składająca się z długości (sumy wag krawędzi)
        /// znalezionego cyklu i tablicy krawędzi tworzących ten cykl)
        /// </returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu zawierającego krawędź o wadze ujemnej</exception>
        /// <remarks>
        /// Metoda przeznaczona jest dla grafów z nieujemnymi wagami krawędzi.<para/>
        /// Uruchomiona dla grafu zawierającego krawędź o wadze ujemnej zgłasza wyjątek ArgumentException.
        /// (Warunek ten sprawdzany jest w sposób przybliżony,
        /// jedynie przy próbie dodania krawędzi o ujemnej wadze do konstruowanego cyklu).<para/>
        /// Elementy (krawędzie) umieszczone są w tablicy cycle
        /// w kolejności swojego następstwa w znalezionym cyklu Hamiltona.<para/>
        /// Jeśli w badanym grafie nie istnieje cykl Hamiltona metoda zwraca krotkę (NaN,null).<para/>
        /// Metodę można stosować dla grafów skierowanych i nieskierowanych.
        /// </remarks>
        /// <seealso cref="BacktrackingTSPGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double weight, Edge[] cycle) BacktrackingTSP(this Graph g)
        {
            if (g.VerticesCount <= (g.Directed ? 1 : 2))
                return (double.NaN, null);

            Edge[] bestCycle = null;
            var bestWeight = double.PositiveInfinity;
            var tempCycle = new Edge[g.VerticesCount];
            var visited = new bool[g.VerticesCount];

            void Rec(int currVertex, int i, double currWeight)
            {
                if (currWeight >= bestWeight)
                    return;
                if (i == g.VerticesCount - 1)
                {
                    var edgeWeight = g.GetEdgeWeight(currVertex, 0);
                    if (!(currWeight + edgeWeight < bestWeight)) return;
                    if (edgeWeight < 0.0)
                        throw new ArgumentException("Negative weights are not allowed");
                    bestWeight = currWeight + edgeWeight;
                    tempCycle[i] = new Edge(currVertex, 0, edgeWeight);
                    bestCycle = (Edge[])tempCycle.Clone();
                    return;
                }
                visited[currVertex] = true;
                foreach (var edge in g.OutEdges(currVertex))
                {
                    if (visited[edge.To]) continue;
                    if (edge.Weight < 0.0)
                        throw new ArgumentException("Negative weights are not allowed");
                    tempCycle[i] = edge;
                    Rec(edge.To, i + 1, currWeight + edge.Weight);
                }
                visited[currVertex] = false;
            }
            
            Rec(0,0,0.0);
            
            return double.IsPositiveInfinity(bestWeight) ? (double.NaN, null) : (bestWeight, bestCycle);
        }
    }
}
