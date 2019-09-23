using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Graf reprezentowany za pomocą list sąsiedztwa (list incydencji)
    /// </summary>
    /// <typeparam name="TAdjacencyList">Typ implementujący listy incydencji</typeparam>
    /// <seealso cref="Graph"/>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public sealed class AdjacencyListsGraph<TAdjacencyList> : Graph where TAdjacencyList : IAdjacencyList, new()
    {

        private readonly IAdjacencyList[] _adjacencyList;

        /// <summary>
        /// Tworzy graf składający się ze wskazanej liczby izolowanych wierzchołków
        /// </summary>
        /// <param name="directed">Informacja czy graf jest skierowany</param>
        /// <param name="vertCount">Liczba wierzchołków grafu</param>
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}(Graph)"/>
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public AdjacencyListsGraph(bool directed, int vertCount):base(directed, vertCount)
        {
            _adjacencyList = new IAdjacencyList[vertCount];
            for (var i = 0; i < _adjacencyList.Length; i++)
            {
                _adjacencyList[i] = Activator.CreateInstance<TAdjacencyList>();
                IAbstractDictionary<int, double> abstractDictionary = _adjacencyList[i];
                abstractDictionary.SetAccess(CMonDoSomething.Nothing);
            }
        }

        /// <summary>
        /// Tworzy kopię grafu będącego parametrem
        /// </summary>
        /// <param name="g">Kopiowany graf</param>
        /// <remarks>Wynikowy graf jest reprezentowany za pomocą list sąsiedztwa (incydencji),
        /// źródłowy graf g może być reprezentowany w inny sposób. Może więc nastąpić zmiana sposobu reprezentacji grafu.
        /// </remarks>
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}(bool,int)"/>
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public AdjacencyListsGraph(Graph g) : this(g.Directed, g.VerticesCount)
        {
            Merge(g);
        }

        /// <summary>
        /// Tworzy głęboką kopię bieżącego grafu
        /// </summary>
        /// <returns>Utworzona kopia grafu</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override Graph Clone()
        {
            return new AdjacencyListsGraph<TAdjacencyList>(this);
        }

        /// <summary>
        /// Tworzy graf o takiej samej liczbie wierzchołków i "skierowalności" jak bieżący, ale bez żadnych krawędzi
        /// </summary>
        /// <returns>Utworzony graf</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="IsolatedVerticesGraph(bool, int)"/>
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override Graph IsolatedVerticesGraph()
        {
            return new AdjacencyListsGraph<TAdjacencyList>(Directed, VerticesCount);
        }
        
        /// <summary>
        /// Tworzy graf składający się ze wskazanej liczby izolowanych wierzchołków
        /// </summary>
        /// <param name="directed">Informacja czy graf jest skierowany</param>
        /// <param name="verticesCount">Liczba wierzchołków grafu</param>
        /// <returns>Utworzony graf</returns>
        /// <remarks>Tworzony graf jest takiego samego typu jak bieżący, metoda to "wirtualny konstruktor".</remarks>
        /// <seealso cref="IsolatedVerticesGraph()"/>
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override Graph IsolatedVerticesGraph(bool directed, int verticesCount)
        {
            return new AdjacencyListsGraph<TAdjacencyList>(directed, verticesCount);
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
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override bool AddEdge(int from, int to, double weight = 1.0)
        {
            if (weight.IsNaN()) throw new ArgumentException("Invalid weight (NaN)");
            
            if (!_adjacencyList[from].Insert(to, weight)) return false;

            OutDegreeTable[from]++;
            InDegreeTable[to]++;
            EdgesCount++;
            
            if (Directed) return true;
            if (from == to) return true;
            
            _adjacencyList[to].Insert(from, weight);
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
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override bool DelEdge(int from, int to)
        {
            if (!_adjacencyList[from].Remove(to)) return false;
            
            OutDegreeTable[from]--;
            InDegreeTable[to]--;
            EdgesCount--;
            if (Directed || from == to) return true;
            _adjacencyList[to].Remove(from);
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
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override double GetEdgeWeight(int from, int to)
        {
            return !_adjacencyList[from].Search(to, out var result) ? double.NaN : result;
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
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override double ModifyEdgeWeight(int from, int to, double add)
        {
            if (!_adjacencyList[from].Search(to, out var weight)) return double.NaN;
            
            weight += add;
            if (weight.IsNaN()) throw new ArgumentException("Invalid modified weight value (NaN)");
            
            _adjacencyList[from].Modify(to, weight);
            if (!Directed && from != to)
            {
                _adjacencyList[to].Modify(from, weight);
            }
            return weight;
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
        /// <seealso cref="AdjacencyListsGraph{TAdjacencyList}"/>
        /// <seealso cref="Graph"/>
        /// <seealso cref="ASD.Graphs"/>
        public override IEnumerable<Edge> OutEdges(int from)
        {
            var array = new Edge[OutDegree(from)];
            var num = 0;
            foreach (var keyValuePair in _adjacencyList[from])
            {
                array[num++] = new Edge(from, keyValuePair.Key, keyValuePair.Value);
            }
            return array;
        }

    }
}
