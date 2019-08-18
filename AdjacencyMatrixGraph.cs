using System;
using System.Collections.Generic;
using System.Runtime.Serialization;

namespace ASD.Graphs
{
    [Serializable]
    public sealed class AdjacencyMatrixGraph : Graph
    {
        [NonSerialized]
        private double[,] _adjacencyMatrix;
        private List<Edge> _serializedAdjacencyMatrix;

        public AdjacencyMatrixGraph(bool directed, int vertCount) : base(directed, vertCount)
        {
            _adjacencyMatrix = new double[vertCount, vertCount];
            for (var i = 0; i < vertCount; i++) 
                for (var j = 0; j < vertCount; j++) 
                    _adjacencyMatrix[i, j] = double.NaN;
        }

        public AdjacencyMatrixGraph(Graph g) : this(g.Directed, g.VerticesCount)
        {
            Merge(g);
        }

        public override Graph Clone()
        {
            return new AdjacencyMatrixGraph(this);
        }

        public override Graph IsolatedVerticesGraph()
        {
            return new AdjacencyMatrixGraph(Directed, VerticesCount);
        }

        public override Graph IsolatedVerticesGraph(bool directed, int verticesCount)
        {
            return new AdjacencyMatrixGraph(directed, verticesCount);
        }

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

        public override double GetEdgeWeight(int from, int to)
        {
            return _adjacencyMatrix[from, to];
        }

        public override double ModifyEdgeWeight(int from, int to, double add)
        {
            if (_adjacencyMatrix[from, to].IsNaN()) return double.NaN;
            
            var newWeight = _adjacencyMatrix[from, to] + add;
            if (newWeight.IsNaN()) throw new ArgumentException("Invalid modified weight value (NaN)");
            
            _adjacencyMatrix[from, to] = newWeight;
            if (!Directed && from != to) _adjacencyMatrix[to, from] = newWeight;
            
            return newWeight;
        }

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
