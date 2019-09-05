namespace ASD.Graphs
{
    /// <summary>
    /// Interfejs opisujący kontenery na krawędzie
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public interface IEdgesContainer
    {
        /// <summary>
        /// Informacja czy kontener jest pusty (właściwość tylko do odczytu)
        /// </summary>
        /// <seealso cref="IEdgesContainer"/>'
        /// <seealso cref="ASD.Graphs"/>
        bool Empty { get; }
        
        /// <summary>
        /// Liczba elementów znajdujących się w kontenerze (właściwość tylko do odczytu)
        /// </summary>
        /// <remarks>
        /// Liczba elementów kontenera jest odpowiednio modyfikowana przez metody <see cref="Put"/> i <see cref="Get"/>.
        /// </remarks>
        /// <seealso cref="IEdgesContainer"/>'
        /// <seealso cref="ASD.Graphs"/>
        int Count { get; }
        
        /// <summary>
        /// Wstawia element do kontenera
        /// </summary>
        /// <param name="e">Wstawiany element</param>
        /// <seealso cref="IEdgesContainer"/>'
        /// <seealso cref="ASD.Graphs"/>
        void Put(Edge e);
        
        /// <summary>
        /// Pobiera element z kontenera
        /// </summary>
        /// <returns>Pobrany element</returns>
        /// <remarks>Pobrany element jest usuwany z kontenera.</remarks>
        /// <seealso cref="IEdgesContainer"/>'
        /// <seealso cref="ASD.Graphs"/>
        Edge Get();
        
        /// <summary>
        /// Podgląda pierwszy element kontenera
        /// </summary>
        /// <returns>Pierwszy (do pobrania) element kontenera</returns>
        /// <remarks>Element pozostaje w kontenerze.</remarks>
        /// <seealso cref="IEdgesContainer"/>'
        /// <seealso cref="ASD.Graphs"/>
        Edge Peek();
        
        /// <summary>
        /// Kopiuje elementy kontenera do tablicy
        /// </summary>
        /// <returns>Tablica zawierająca wszystkie elementy kontenera</returns>
        /// <seealso cref="IEdgesContainer"/>'
        /// <seealso cref="ASD.Graphs"/>
        Edge[] ToArray();
    }
}
