namespace ASD.Graphs
{
    /// <summary>
    /// Wariant metody znajdowania przybliżonego rozwiązania problemu komiwojażera na podstawie drzewa rozpinającego
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public enum TSPTreeBasedVersion
    {
        /// <summary>
        /// Wersja podstawowa algorytmu
        /// </summary>
        Simple,
        /// <summary>
        /// Algorytm Christofidesa
        /// </summary>
        Christofides,
        /// <summary>
        /// Zmodyfikowany algorytm Christofidesa
        /// </summary>
        ModifiedChristofides
    }
}
