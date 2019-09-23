namespace ASD.Graphs
{
    /// <summary>
    /// Formaty exportu grafów
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public enum ExportFormat
    {
        /// <summary>
        /// Graf nie jest eksportowany
        /// </summary>
        None,
        /// <summary>
        /// Export do formatu .dot pakietu Graphviz
        /// </summary>
        Dot,
        /// <summary>
        /// Export do formatu możliwego do wyświetlenia
        /// </summary>
        Image,
        /// <summary>
        /// Wyświetlenie wizualizacji grafu
        /// </summary>
        View
    }
}
