using System;

namespace ASD.Graphs
{
    public static class BacktrackingTSPGraphExtender
    {
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
