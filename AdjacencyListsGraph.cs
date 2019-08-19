using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Graf reprezentowany za pomocą list sąsiedztwa (list incydencji)
    /// </summary>
    /// <typeparam name="TAdjacencyList">Typ implementujący listy incydencji</typeparam>
    /// <seealso cref="ASD.Graphs"/>
    [Serializable]
    public sealed class AdjacencyListsGraph<TAdjacencyList> : Graph where TAdjacencyList : IAdjacencyList, new()
    {

        private readonly IAdjacencyList[] _adjacencyList;

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

        public AdjacencyListsGraph(Graph g) : this(g.Directed, g.VerticesCount)
        {
            Merge(g);
        }

        public override Graph Clone()
        {
            return new AdjacencyListsGraph<TAdjacencyList>(this);
        }

        public override Graph IsolatedVerticesGraph()
        {
            return new AdjacencyListsGraph<TAdjacencyList>(Directed, VerticesCount);
        }

        public override Graph IsolatedVerticesGraph(bool directed, int verticesCount)
        {
            return new AdjacencyListsGraph<TAdjacencyList>(directed, verticesCount);
        }

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

        public override double GetEdgeWeight(int from, int to)
        {
            return !_adjacencyList[from].Search(to, out var result) ? double.NaN : result;
        }

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
