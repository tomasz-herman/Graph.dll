namespace ASD.Graphs
{
    /// <summary>
    /// Delegacja opisująca metody wyznaczania maksymalnego przepływu
    /// </summary>
    /// <param name="g">Badany graf (sieć przepływowa)</param>
    /// <param name="s">Wierzchołek źródłowy</param>
    /// <param name="t">Wierzchołek docelowy</param>
    /// <param name="af">Metoda powiększania przepływu</param>
    /// <param name="MatrixToAVL">Czy optymalizować sposób reprezentacji grafu rezydualnego</param>
    /// <returns>Krotka (value, flow) składająca się z wartości maksymalnego przepływu i grafu opisującego ten przepływ</returns>
    /// <remarks>
    /// Do tej delegacji pasują wszystkie metody wyznaczania maksymalnego przepływu.<para/>
    /// Znaczenie parametru fi zależy od konkretnego algorytmu wyznaczania maksymalnego przepływu.
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    public delegate (double value, Graph flow) MaxFlow(Graph g, int s, int t, AugmentFlow af = null, bool MatrixToAVL = true);
}
