namespace ASD.Graphs
{
    /// <summary>
    /// Metoda wyboru wstawianego wierzchołka w algorytmie przyrostowym dla problemu komiwojażera
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public enum TSPIncludeVertexSelectionMethod
    {
        /// <summary>
        /// Wstawianie kolejnego wierzchołka
        /// </summary>
        Sequential,
        /// <summary>
        /// Wstawianie najbliższego wierzchołka
        /// </summary>
        Nearest,
        /// <summary>
        /// Wstawianie najdalszego wierzchołka
        /// </summary>
        Furthest,
        /// <summary>
        /// Wstawianie wierzchołka o najmniejszym koszcie wstawienia
        /// </summary>
        MinimalCost
    }
}
