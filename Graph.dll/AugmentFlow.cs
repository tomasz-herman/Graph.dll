namespace ASD.Graphs
{
    /// <summary>
    /// Delegacja opisująca metody powiekszania przepływu
    /// </summary>
    /// <param name="g">Badany graf (sieć przepływowa)</param>
    /// <param name="s">Wierzchołek źródłowy</param>
    /// <param name="t">Wierzchołek docelowy</param>
    /// <returns>Krotka (augmentingValue, augmentingFlow) składająca się z wartości przepływu powiekszającego i grafu opisującego ten przepływ</returns>
    /// <remarks>
    /// Przepływ powiekszający może (ale nie musi) redukować się do ścieżki powiększającej.<para/>
    /// Przepływ powiekszający zawsze (nawet gdy wiadomo, że jest pojedynczą ścieżką) jest reprezantowany jako graf.
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    public delegate (double augmentingValue, Graph augmentingFlow) AugmentFlow(Graph g, int s, int t);
}
