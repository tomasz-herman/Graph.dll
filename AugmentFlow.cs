namespace ASD.Graphs
{
    public delegate (double augmentingValue, Graph augmentingFlow) AugmentFlow(Graph g, int s, int t);
}
