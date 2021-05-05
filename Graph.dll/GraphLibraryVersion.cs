namespace ASD.Graphs
{
    /// <summary>
    /// Numer wersji biblioteki
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class GraphLibraryVersion
    {
        /// <summary>
        /// Główny numer wersji
        /// </summary>
        /// <remarks>Główny numer wersji zmieniany jest w przypadku wprowadzenia istotnych zmian w bibliotece.</remarks>
        /// <seealso cref="GraphLibraryVersion"/>
        /// <seealso cref="ASD.Graphs"/>
        public static int Major => 7;

        /// <summary>
        /// Pomocniczy numer wersji
        /// </summary>
        /// <remarks>Pomocniczy numer wersji zmieniany jest co roku.</remarks>
        /// <seealso cref="GraphLibraryVersion"/>
        /// <seealso cref="ASD.Graphs"/>
        public static int Minor => 2;

        /// <summary>
        /// Numer rewizji
        /// </summary>
        /// <remarks>Numer rewizji zmieniany jest w przypadku wprowadzenia poprawek podczas trwania semestru.</remarks>
        /// <seealso cref="GraphLibraryVersion"/>
        /// <seealso cref="ASD.Graphs"/>
        public static int Revision => 3;
        
        /// <summary>
        /// Pełny numer wersji
        /// </summary>
        /// <seealso cref="GraphLibraryVersion"/>
        /// <seealso cref="ASD.Graphs"/>
        public static string FullNumber => $"{Major}.{Minor}.{Revision}";
    }
}
