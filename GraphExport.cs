using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;

namespace ASD.Graphs
{
    /// <summary>
    /// "Eksporter Grafów"
    /// </summary>
    /// <remarks>Wymaga zainstalowania pakietu Graphviz (www.graphviz.org).</remarks>
    /// <seealso cref="ASD.Graphs"/>
    public class GraphExport
    {
        public GraphExport(bool showWeights = true, string weightsFormat = null, string graphvizPath = null)
        {
            Format = "gif";
            ShowWeights = showWeights;
            WeightsFormat = weightsFormat;
            if (graphvizPath == null)
            {
                foreach (var path in Directory.GetDirectories("c:\\", "Program Files*"))
                {
                    var dir = Directory.GetDirectories(path, "Graphviz*");
                    if (dir.Length == 0) continue;
                    graphvizPath = dir[0] + "\\bin\\dot.exe";
                    break;
                }
            }
            if (!File.Exists(graphvizPath))
                throw new FileNotFoundException("File dot.exe from Graphviz package not found. Install Graphiz package in standard location or pass full path to the dot.exe program as GraphExport constructor parameter.");
            GraphvizPath = graphvizPath;
        }
        
        public string Format { get; set; }
        
        public string GraphvizPath { get; set; }
        
        public bool ShowWeights { get; set; }
        
        public string WeightsFormat { get; set; }
        
        public void Export(Graph g, string fileName = null, string[] verticesDescriptions = null, ExportFormat format = ExportFormat.View)
        {
            if (format == ExportFormat.None) return;
            
            if (format != ExportFormat.Dot && format != ExportFormat.Image && format != ExportFormat.View)
                throw new ArgumentException("Invalid export format");
            
            if (verticesDescriptions == null)
            {
                verticesDescriptions = new string[g.VerticesCount];
                for (var i = 0; i < g.VerticesCount; i++)
                    verticesDescriptions[i] = $"v{i}";
            }
            
            else if (verticesDescriptions.Length != g.VerticesCount)
                throw new ArgumentException("Invalid verticesDescriptions parameter length");
            
            if (fileName == null)
            {
                var random = new Random();
                fileName = "temp." + random.Next(100000, 999999);
            }
            
            CreateDotFile(g, verticesDescriptions, fileName);
            
            if (format == ExportFormat.Dot)
                return;

            var args = fileName + ".dot -T" + Format + " -o" + fileName + "." + Format;
            var process = Process.Start(new ProcessStartInfo(GraphvizPath, args)
            {
                WindowStyle = ProcessWindowStyle.Hidden
            });
            process?.WaitForExit();
            process?.Dispose();
            if (format == ExportFormat.Image) return;

            using (Process.Start("iexplore", "file:///" + Path.GetFullPath(fileName) + "." + Format))
            {
            }
        }

        private void CreateDotFile(Graph g, IReadOnlyList<string> verticesDescriptions, string fileName)
        {
            using (var streamWriter = new StreamWriter(fileName + ".dot"))
            {
                var format = " [label=\"{0" + WeightsFormat + "}\"]";
                streamWriter.WriteLine(g.Directed ? "digraph" : "graph");
                streamWriter.WriteLine("{");
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    streamWriter.WriteLine("\tnode [shape=circle, label=\"{0}\"] {1};", verticesDescriptions[i], i);
                }
                for (var i = 0; i < g.VerticesCount; i++)
                {
                    foreach (var edge in g.OutEdges(i))
                    {
                        if (!g.Directed && edge.From > edge.To) continue;
                        streamWriter.Write("\t");
                        streamWriter.Write("{0}", edge.From);
                        streamWriter.Write(g.Directed ? " -> " : " -- ");
                        streamWriter.Write("{0}", edge.To);
                        if (ShowWeights)
                        {
                            IFormatProvider invariantCulture = CultureInfo.InvariantCulture;
                            streamWriter.Write(string.Format(invariantCulture, format, edge.Weight));
                        }
                        streamWriter.WriteLine();
                    }
                }
                streamWriter.WriteLine("}");
            }
        }
    }
}
