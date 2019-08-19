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

        public bool Directed
        {
            get; 
            private set;
        }

        public int VerticesCount
        {
            get;
            private set;
        }

        public int EdgesCount
        {
            get;
            private protected set;
        }

        public abstract Graph Clone();

        public abstract Graph IsolatedVerticesGraph();

        public abstract Graph IsolatedVerticesGraph(bool directed, int verticesCount);

        public int OutDegree(int nr)
        {
            return OutDegreeTable[nr];
        }

        public int InDegree(int nr)
        {
            return InDegreeTable[nr];
        }

        public abstract bool AddEdge(int from, int to, double weight = 1.0);

        public bool AddEdge(Edge e)
        {
            return AddEdge(e.From, e.To, e.Weight);
        }

        public abstract bool DelEdge(int from, int to);

        public bool DelEdge(Edge e)
        {
            return DelEdge(e.From, e.To);
        }

        public abstract double GetEdgeWeight(int from, int to);

        public abstract double ModifyEdgeWeight(int from, int to, double add);

        public abstract IEnumerable<Edge> OutEdges(int from);

        public void Add(Edge e)
        {
            AddEdge(e.From, e.To, e.Weight);
        }

        IEnumerator<Edge> IEnumerable<Edge>.GetEnumerator()
        {
            throw new NotSupportedException();
        }

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
