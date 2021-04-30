using System;
using System.Collections;
using System.Collections.Generic;
using System.Xml;

namespace ASD.Graphs
{
    /// <summary>
    /// Abstrakcyjna klasa bazowa dla wszystkich klas implementujących grafy
    /// </summary>
    /// <remarks>
    /// Klasa definiuje metody i właściwości dostępne dla wszystkich grafów niezależnie od sposobu ich reprezentacji.<para/>
    /// Nie da się bezpośrednio tworzyć obiektów tej klasy, ale wszystkie klasy implementujące grafy muszą być jej klasami pochodnymi i muszą korzystać z jej konstruktorów.<para/>
    /// Każda klasa pochodna powinna też mieć konstruktor z parametrami (bool,int) (wymagany przy tworzeniu grafu na podstwie pliku XML).<para/>
    /// Klasa implementuje interfejs <see cref="IEnumerable{T}"/> jedynie formalnie.
    /// Jest to potrzebne aby można było inicjalizować grafy analogicznie jak inne kolekcje. Funcjonalność wymagana przez ten interfejs nie jest dostarczana.
    /// </remarks>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public abstract class Graph : IEnumerable<Edge>
    {

        private protected readonly int[] OutDegreeTable;
        private protected readonly int[] InDegreeTable;

        private protected Graph(bool directed, int vertCount)
        {
            Directed = directed;
            VerticesCount = vertCount;
            OutDegreeTable = new int[vertCount];
            InDegreeTable = new int[vertCount];
        }

        /// <summary>
        /// Informacja czy graf jest skierowany
        /// </summary>
        /// <remarks>Odpowiednia wartość ustawiana jest przy tworzeniu grafu (w konstruktorze), a potem jest niezmienna</remarks>
        /// <seealso cref="ASD.Graphs"/>
        /// <seealso cref="Graph"/>
        public bool Directed
        {
            get;
        }

        /// <summary>
        /// Liczba wierzchołków grafu
        /// </summary>
        /// <remarks>Odpowiednia wartość ustawiana jest przy tworzeniu grafu (w konstruktorze), a potem jest niezmienna</remarks>
        /// <seealso cref="ASD.Graphs"/>
        /// <seealso cref="Graph"/>
        public int VerticesCount
        {
            get;
        }

        /// <summary>
        /// Liczba krawędzi grafu
        /// </summary>
        /// <remarks>Liczba krawędzi grafu jest odpowiednio modyfikowana przez metody <see cref="AddEdge(Edge)"/>,
        /// <see cref="AddEdge(int,int,double)"/> i <see cref="DelEdge(Edge)"/>, <see cref="DelEdge(int,int)"/>.</remarks>
        /// <seealso cref="ASD.Graphs"/>
        /// <seealso cref="Graph"/>
        public int EdgesCount
        {
            get;
            private protected set;
        }

        /// <summary>
        /// Tworzy głęboką kopię bieżącego grafu
        /// </summary>
        /// <returns>Utworzona kopia grafu</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="ASD.Graphs"/>
        /// <seealso cref="Graph"/>
        public abstract Graph Clone();

        /// <summary>
        /// Tworzy graf o takiej samej liczbie wierzchołków i "skierowalności" jak bieżący, ale bez żadnych krawędzi
        /// </summary>
        /// <returns>Utworzony graf</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="IsolatedVerticesGraph(bool, int)"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public abstract Graph IsolatedVerticesGraph();

        /// <summary>
        /// Tworzy graf składający się ze wskazanej liczby izolowanych wierzchołków
        /// </summary>
        /// <param name="directed">Informacja czy graf jest skierowany</param>
        /// <param name="verticesCount">Liczba wierzchołków grafu</param>
        /// <returns>Utworzony graf</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="IsolatedVerticesGraph()"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public abstract Graph IsolatedVerticesGraph(bool directed, int verticesCount);

        /// <summary>
        /// Zwraca stopień wyjściowy wierzchołka grafu
        /// </summary>
        /// <param name="nr">Numer wierzchołka</param>
        /// <returns>Stopień wyjściowy wierzchołka</returns>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public int OutDegree(int nr)
        {
            return OutDegreeTable[nr];
        }

        /// <summary>
        /// Zwraca stopień wejściowy wierzchołka grafu
        /// </summary>
        /// <param name="nr">Numer wierzchołka</param>
        /// <returns>Stopień wejściowy wierzchołka</returns>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public int InDegree(int nr)
        {
            return InDegreeTable[nr];
        }

        /// <summary>
        /// Dodaje do grafu krawędź o wskazanej wadze
        /// </summary>
        /// <param name="from">Początkowy wierzchołek krawędzi</param>
        /// <param name="to">Końcowy wierzchołek krawędzi</param>
        /// <param name="weight">Waga krawędzi</param>
        /// <returns>Informacja czy dodawanie powiodło się</returns>
        /// <exception cref="ArgumentException">Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku</exception>
        /// <remarks>
        /// Metoda zwraca false gdy wskazana krawędź już wcześniej istniała w grafie.<para/>
        /// Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku.<para/>
        /// W przypadku grafów nieważonych parametr weight należy pominąć.
        /// </remarks>
        /// <seealso cref="AddEdge(Edge)"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public abstract bool AddEdge(int from, int to, double weight = 1.0);

        /// <summary>
        /// Dodaje do grafu wskazaną krawędź
        /// </summary>
        /// <param name="e">Dodawana krawędź</param>
        /// <returns>Informacja czy dodawanie powiodło się</returns>
        /// <remarks>
        /// Metoda zwraca false gdy wskazana krawędź już wcześniej istniała w grafie.<para/>
        /// Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku.
        /// </remarks>
        /// <seealso cref="AddEdge(int, int, double)"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool AddEdge(Edge e)
        {
            return AddEdge(e.From, e.To, e.Weight);
        }

        /// <summary>
        /// Usuwa krawędź z grafu
        /// </summary>
        /// <param name="from">Początkowy wierzchołek krawędzi</param>
        /// <param name="to">Końcowy wierzchołek krawędzi</param>
        /// <returns>Informacja czy usuwanie powiodło się</returns>
        /// <exception cref="ArgumentException">Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku</exception>
        /// <remarks>
        /// Metoda zwraca false gdy wskazanej krawędzi nie było w grafie.<para/>
        /// Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku.
        /// </remarks>
        /// <seealso cref="DelEdge(Edge)"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public abstract bool DelEdge(int from, int to);

        /// <summary>
        /// Usuwa krawędź z grafu
        /// </summary>
        /// <param name="e">Usuwana krawędź</param>
        /// <returns>Informacja czy usuwanie powiodło się</returns>
        /// <remarks>
        /// Metoda zwraca false gdy wskazanej krawędzi nie było w grafie.<para/>
        /// Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku.<para/>
        /// Waga krawędzi podana w parametrze e nie ma znaczenia, ważne są jedynie wierzchołki początkowy i końcowy.
        /// </remarks>
        /// <seealso cref="DelEdge(int, int)"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool DelEdge(Edge e)
        {
            return DelEdge(e.From, e.To);
        }

        /// <summary>
        /// Zwraca wagę wskazanej krawędzi grafu
        /// </summary>
        /// <param name="from">Początkowy wierzchołek krawędzi</param>
        /// <param name="to">Końcowy wierzchołek krawędzi</param>
        /// <returns>Waga krawędzi</returns>
        /// <remarks>
        /// Metoda zwraca wartość NaN gdy wskazanej krawędzi nie ma w grafie.<para/>
        /// Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku.<para/>
        /// Do sprawdzenia czy metoda zwróciła NaN należy używać metody IsNaN, użycie operatora == spowoduje błędną odpowiedź (zawsze false).<para/>
        /// Metody należy używać do dostępu do pojedynczych krawędzi.<para/>
        /// Do zbadania wszystkich krawędzi wychodzących z danego wierzchołka należy używać metody OutEdges.
        /// </remarks>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public abstract double GetEdgeWeight(int from, int to);

        /// <summary>
        /// Modyfikuje wagę wskazanej krawędzi grafu
        /// </summary>
        /// <param name="from">Początkowy wierzchołek krawędzi</param>
        /// <param name="to">Końcowy wierzchołek krawędzi</param>
        /// <param name="add">Wartość zwiększenia wagi krawędzi</param>
        /// <returns>Nowa (zmodyfikowana) waga krawędzi</returns>
        /// <remarks>
        /// Metoda zwraca wartość NaN gdy wskazanej krawędzi nie ma w grafie.<para/>
        /// Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku.<para/>
        /// Do sprawdzenia czy metoda zwróciła NaN należy używać metody <see cref="GraphHelperExtender.IsNaN"/>, użycie operatora == spowoduje błędną odpowiedź (zawsze false).
        /// </remarks>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public abstract double ModifyEdgeWeight(int from, int to, double add);

        /// <summary>
        /// Wylicza wszystkie krawędzie wychodzące z danego wierzchołka
        /// </summary>
        /// <param name="from">Numer wierzchołka</param>
        /// <returns>Żądane wyliczenie krawedzi</returns>
        /// <remarks>
        /// Metoda jest najczęściej używana w połaczeniu z instrukcją foreach.<para/>
        /// Metoda w rzeczywistości zwraca tablicę krawędzi wychodzącyh z danego wierzchołka.
        /// </remarks>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public abstract IEnumerable<Edge> OutEdges(int from);

        /// <summary>
        /// Dodaje do grafu wskazaną krawędź
        /// </summary>
        /// <param name="e">Dodawana krawędź</param>
        /// <remarks>
        /// Metoda przeznaczona jest do zastosowań wewnętrznych (ale niestety musi być publiczna), nie należy jej używać (należy używać metody <see cref="AddEdge(Edge)"/>).<para/>
        /// Podanie nieprawidłowych (nieistniejących) numerów wierzchołków powoduje zgłoszenie wyjątku.
        /// </remarks>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public void Add(Edge e)
        {
            AddEdge(e.From, e.To, e.Weight);
        }

        /// <summary>
        /// Metoda wymagana przez interfejs <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <returns>Nic (metoda zgłasza wyjątek)</returns>
        /// <exception cref="NotSupportedException">Zawsze zgłasza wyjątek</exception>
        /// <remarks>
        /// Metoda zgłasza wyjątek <see cref="NotSupportedException"/> (ponieważ klasa <see cref="Graph"/>
        /// tak naprawdę nie implementuje interfejsu <see cref="IEnumerable{T}"/>).
        /// </remarks>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        IEnumerator<Edge> IEnumerable<Edge>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        /// <summary>
        /// Metoda wymagana przez interfejs <see cref="IEnumerable{T}"/>
        /// </summary>
        /// <returns>Nic (metoda zgłasza wyjątek)</returns>
        /// <exception cref="NotSupportedException">Zawsze zgłasza wyjątek</exception>
        /// <remarks>
        /// Metoda zgłasza wyjątek <see cref="NotSupportedException"/> (ponieważ klasa <see cref="Graph"/>
        /// tak naprawdę nie implementuje interfejsu <see cref="IEnumerable{T}"/>).
        /// </remarks>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotSupportedException();
        }

        internal static Graph Create(bool directed, int vertCount, Type type)
        {
            if (!type.IsSubclassOf(typeof(Graph)))
            {
                throw new ArgumentException("Passed type isn't derived from class Graph");
            }
            var constructor = type.GetConstructor(new[]
            {
                typeof(bool),
                typeof(int)
            });
            var args = new object[2];
            args[0] = directed;
            args[1] = vertCount;
            return constructor?.Invoke(args) as Graph;
        }

        private protected void Merge(Graph g)
        {
            for (var i = 0; i < g.VerticesCount; i++)
                foreach (var e in g.OutEdges(i))
                    AddEdge(e);
        }

        /// <summary>
        /// Zapisuje graf do pliku
        /// </summary>
        /// <param name="g">Wskazany graf</param>
        /// <param name="path">Nazwa pliku</param>
        /// <remarks>
        /// Graf zapisany jest w postaci XML.<para/>
        /// Nie jest stosowana serializacja, dzięki temu graf może być odtworzony w innej reprezentacji
        /// (jako obiekt innego typu - przy serializacji odtwarzany jest obiekt tego samego typu).
        /// </remarks>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        internal static void Write(Graph g, string path)
        {
            var xmlWriterSettings = new XmlWriterSettings {Indent = true, IndentChars = "    "};
            using (var xmlWriter = XmlWriter.Create(path, xmlWriterSettings))
            {
                xmlWriter.WriteStartDocument();
                xmlWriter.WriteStartElement("Graph");
                xmlWriter.WriteAttributeString("directed", $"{g.Directed}");
                xmlWriter.WriteAttributeString("verticescount", $"{g.VerticesCount}");
                xmlWriter.WriteStartElement("Edges");
                for (var i = 0; i < g.VerticesCount; i++)
                    foreach (var edge in g.OutEdges(i))
                    {
                        xmlWriter.WriteStartElement("Edge");
                        xmlWriter.WriteAttributeString("from", $"{edge.From}");
                        xmlWriter.WriteAttributeString("to", $"{edge.To}");
                        xmlWriter.WriteAttributeString("weight", $"{edge.Weight:R}");
                        xmlWriter.WriteEndElement();
                    }
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndElement();
                xmlWriter.WriteEndDocument();
            }
        }

        /// <summary>
        /// Odtwarza graf na podstawie zapisu w pliku
        /// </summary>
        /// <param name="type">Wskazuje typ tworzonego grafu</param>
        /// <param name="path">Nazwa pliku</param>
        /// <returns>Odtworzony graf</returns>
        /// <exception cref="FormatException"></exception>
        /// <remarks>Metoda wykorzystuje konstruktor <see cref="Graph(bool, int)"/> i dlatego
        /// jest on wymagany w każdej klasie pochodenej od klasy <see cref="Graph"/>.
        /// </remarks>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        internal static Graph Read(Type type, string path)
        {
            Graph graph = null;
            using (var xmlReader = XmlReader.Create(path))
            {
                while (xmlReader.Read())
                {
                    if (xmlReader.NodeType != XmlNodeType.Element) continue;
                    if (xmlReader.Name != "Graph")
                    {
                        if (xmlReader.Name != "Edge") continue;
                        if (!int.TryParse(xmlReader.GetAttribute("from"), out var from))
                        {
                            throw new FormatException("Invalid graph format in file " + path);
                        }

                        if (!int.TryParse(xmlReader.GetAttribute("to"), out var to))
                        {
                            throw new FormatException("Invalid graph format in file " + path);
                        }

                        if (!double.TryParse(xmlReader.GetAttribute("weight"), out var weight))
                        {
                            throw new FormatException("Invalid graph format in file " + path);
                        }

                        graph?.AddEdge(from, to, weight);
                    }
                    else
                    {
                        if (!bool.TryParse(xmlReader.GetAttribute("directed"), out var flag))
                        {
                            throw new FormatException("Invalid graph format in file " + path);
                        }

                        if (!int.TryParse(xmlReader.GetAttribute("verticescount"), out var num))
                        {
                            throw new FormatException("Invalid graph format in file " + path);
                        }
                        graph = Create(flag, num, type);
                    }
                }
            }
            return graph;
        }
    }
}
