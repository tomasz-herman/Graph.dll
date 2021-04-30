using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASD.Graphs
{
    /// <summary>
    /// Graf o reprezentowany za pomocą macierzy sąsiedztwa
    /// </summary>
    /// <seealso cref="Graph"/>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public sealed class AdjacencyMatrixGraph : Graph
    {
        [NonSerialized]
        private double[,] _adjacencyMatrix;
        private List<Edge> _serializedAdjacencyMatrix;

        /// <summary>
        /// Tworzy graf składający się ze wskazanej liczby izolowanych wierzchołków
        /// </summary>
        /// <param name="directed">Informacja czy graf jest skierowany</param>
        /// <param name="vertCount">Liczba wierzchołków grafu</param>
        /// <seealso cref="AdjacencyMatrixGraph(Graph)"/>
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public AdjacencyMatrixGraph(bool directed, int vertCount) : base(directed, vertCount)
        {
            _adjacencyMatrix = new double[vertCount, vertCount];
            for (var i = 0; i < vertCount; i++) 
                for (var j = 0; j < vertCount; j++) 
                    _adjacencyMatrix[i, j] = double.NaN;
        }

        /// <summary>
        /// Tworzy kopię grafu będącego parametrem
        /// </summary>
        /// <param name="g">Kopiowany graf</param>
        /// <remarks>Wynikowy graf jest reprezentowany za pomocą macierzy sąsiedztwa,
        /// źródłowy graf g może być reprezentowany w inny sposób. Może więc nastąpić zmiana sposobu reprezentacji grafu.
        /// </remarks>
        /// <seealso cref="AdjacencyMatrixGraph(bool,int)"/>
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public AdjacencyMatrixGraph(Graph g) : this(g.Directed, g.VerticesCount)
        {
            Merge(g);
        }

        /// <summary>
        /// Tworzy głęboką kopię bieżącego grafu
        /// </summary>
        /// <returns>Utworzona kopia grafu</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override Graph Clone()
        {
            return new AdjacencyMatrixGraph(this);
        }

        /// <summary>
        /// Tworzy graf o takiej samej liczbie wierzchołków i "skierowalności" jak bieżący, ale bez żadnych krawędzi
        /// </summary>
        /// <returns>Utworzony graf</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="IsolatedVerticesGraph(bool, int)"/>
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override Graph IsolatedVerticesGraph()
        {
            return new AdjacencyMatrixGraph(Directed, VerticesCount);
        }

        /// <summary>
        /// Tworzy graf składający się ze wskazanej liczby izolowanych wierzchołków
        /// </summary>
        /// <param name="directed">Informacja czy graf jest skierowany</param>
        /// <param name="verticesCount">Liczba wierzchołków grafu</param>
        /// <returns>Utworzony graf</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="IsolatedVerticesGraph()"/>
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override Graph IsolatedVerticesGraph(bool directed, int verticesCount)
        {
            return new AdjacencyMatrixGraph(directed, verticesCount);
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
        /// <seealso cref="Graph.AddEdge(Edge)"/>
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override bool AddEdge(int from, int to, double weight = 1.0)
        {
            if (weight.IsNaN()) throw new ArgumentException("Invalid weight (NaN)");
            
            if (!_adjacencyMatrix[from, to].IsNaN()) return false;
            
            _adjacencyMatrix[from, to] = weight;
            OutDegreeTable[from]++;
            InDegreeTable[to]++;
            EdgesCount++;
            if (Directed || from == to) return true;
            _adjacencyMatrix[to, from] = weight;
            OutDegreeTable[to]++;
            InDegreeTable[from]++;
            return true;
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
        /// <seealso cref="Graph.DelEdge(Edge)"/>
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override bool DelEdge(int from, int to)
        {
            if (_adjacencyMatrix[from, to].IsNaN()) return false;
            
            _adjacencyMatrix[from, to] = double.NaN;
            OutDegreeTable[from]--;
            InDegreeTable[to]--;
            EdgesCount--;
            if (Directed) return true;
            if (from == to) return true;
            _adjacencyMatrix[to, from] = double.NaN;
            OutDegreeTable[to]--;
            InDegreeTable[from]--;
            return true;
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
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override double GetEdgeWeight(int from, int to)
        {
            return _adjacencyMatrix[from, to];
        }

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
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override double ModifyEdgeWeight(int from, int to, double add)
        {
            if (_adjacencyMatrix[from, to].IsNaN()) return double.NaN;
            
            var newWeight = _adjacencyMatrix[from, to] + add;
            if (newWeight.IsNaN()) throw new ArgumentException("Invalid modified weight value (NaN)");
            
            _adjacencyMatrix[from, to] = newWeight;
            if (!Directed && from != to) _adjacencyMatrix[to, from] = newWeight;
            
            return newWeight;
        }

        /// <summary>
        /// Wylicza wszystkie krawędzie wychodzące z danego wierzchołka
        /// </summary>
        /// <param name="from">Numer wierzchołka</param>
        /// <returns>Żądane wyliczenie krawedzi</returns>
        /// <remarks>
        /// Metoda jest najczęściej używana w połaczeniu z instrukcją foreach.<para/>
        /// Metoda w rzeczywistości zwraca tablicę krawędzi wychodzącyh z danego wierzchołka.
        /// </remarks>
        /// <seealso cref="AdjacencyMatrixGraph"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override IEnumerable<Edge> OutEdges(int from)
        {
            var array = new Edge[OutDegree(from)];
            var num = 0;
            for (var i = 0; i < VerticesCount; i++)
            {
                if (!_adjacencyMatrix[from, i].IsNaN())
                {
                    //yield return new Edge(from, i, _adjacencyMatrix[from, i]);??
                    array[num++] = new Edge(from, i, _adjacencyMatrix[from, i]);
                }
            }
            return array;
        }

        [OnSerializing]
        private void OnSerializing(StreamingContext streamingContext)
        {
            _serializedAdjacencyMatrix = new List<Edge>();
            for (var i = 0; i < VerticesCount; i++)
            {
                for (var j = Directed ? 0 : i; j < VerticesCount; j++)
                {
                    if (!_adjacencyMatrix[i, j].IsNaN())
                    {
                        _serializedAdjacencyMatrix.Add(new Edge(i, j, _adjacencyMatrix[i, j]));
                    }
                }
            }
        }

        [OnSerialized]
        private void OnSerialized(StreamingContext streamingContext)
        {
            _serializedAdjacencyMatrix = null;
        }

        [OnDeserialized]
        private void OnDeserialized(StreamingContext streamingContext)
        {
            _adjacencyMatrix = new double[VerticesCount, VerticesCount];
            for (var i = 0; i < VerticesCount; i++)
                 for (var j = 0; j < VerticesCount; j++)
                     _adjacencyMatrix[i, j] = double.NaN;

            if (Directed)
            {
                foreach (var edge in _serializedAdjacencyMatrix)
                    _adjacencyMatrix[edge.From, edge.To] = edge.Weight;
            }
            else
            {
                foreach (var edge in _serializedAdjacencyMatrix)
                {
                    _adjacencyMatrix[edge.From, edge.To] = edge.Weight;
                    _adjacencyMatrix[edge.To, edge.From] = edge.Weight;
                }
            }
            _serializedAdjacencyMatrix = null;
        }

    }
}
