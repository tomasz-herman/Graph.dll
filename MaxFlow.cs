namespace ASD.Graphs
{
    public delegate (double value, Graph flow) MaxFlow(Graph g, int s, int t, AugmentFlow af = null, bool MatrixToAVL = true);
}
