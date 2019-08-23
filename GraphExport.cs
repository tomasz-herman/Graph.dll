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
        /// <summary>
        /// Tworzy obiekt "Eksportera Grafów"
        /// </summary>
        /// <param name="showWeights">Informacja czy pokazywać wagi krawędzi</param>
        /// <param name="weightsFormat">Ciąg formatujący dla opisu wag krawędzi</param>
        /// <param name="graphvizPath">Ścieżka do programu dot.exe z pakietu GraphViz</param>
        /// <exception cref="FileNotFoundException"></exception>
        /// <remarks>
        /// Jako parametr weightsFormat należy podać ciąg formatujący
        /// zgodny z zasadami formatowania liczb typu double w języku C#.<para/>
        /// Ciąg pusty lub null (wartość domyślna) oznacza formatowanie domyślne.<para/>
        /// Jako parametr pathGraphVizDot należy podać pełną ścieżkę (wraz z nazwą pliku)
        /// np. h:\graphviz\bin\dot.exe<para/>
        /// Domyślna wartość parametru pathGraphVizDot (null) oznacza, że w celu znalezienia programu dot.exe
        /// z pakietu GraphViz zostanią przeszukane katalogi "Program Files" i "Program Files (86)" na dysku C.
        /// </remarks>
        /// <seealso cref="GraphExport"/>
        /// <seealso cref="ASD.Graphs"/>
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
        
        /// <summary>
        /// Format wyeksportowanych grafów
        /// </summary>
        /// <remarks>Domyślnie "gif".</remarks>
        /// <seealso cref="GraphExport"/>
        /// <seealso cref="ASD.Graphs"/>
        public string Format { get; set; }
        
        /// <summary>
        /// Ścieżka do programu dot.exe z pakietu GraphViz
        /// </summary>
        /// <seealso cref="GraphExport"/>
        /// <seealso cref="ASD.Graphs"/>
        public string GraphvizPath { get; set; }
        
        /// <summary>
        /// Informacja czy pokazywać wagi krawędzi
        /// </summary>
        /// <seealso cref="GraphExport"/>
        /// <seealso cref="ASD.Graphs"/>
        public bool ShowWeights { get; set; }
        
        /// <summary>
        /// Ciąg formatujący dla opisu wag krawędzi
        /// </summary>
        /// <remarks>
        /// Ciąg formatujący musi być zgodny z zasadami formatowania liczb typu double w języku C#.<para/>
        /// Ciąg pusty lub null oznacza formatowanie domyślne.
        /// </remarks>
        /// <seealso cref="GraphExport"/>
        /// <seealso cref="ASD.Graphs"/>
        public string WeightsFormat { get; set; }
        
        /// <summary>
        /// Exportuje graf do zadanego formatu
        /// </summary>
        /// <param name="g">Dany graf</param>
        /// <param name="fileName">Nazwa pliku wynikowego</param>
        /// <param name="verticesDescriptions">Opisy wierzchołków grafu</param>
        /// <param name="format">Format pliku wynikowego</param>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Domyślna wartość parametru fileName (null) oznacza, że zostanie wygenerowana losowa nazwa pliku.<para/>
        /// Domyślna wartość parametru verticesDescriptions (null) oznacza,
        /// że jako opisy przyjęte zostaną numery wierzchołków w grafie.<para/>
        /// Znaczenie parametru format opisane jest w opisie wyliczenia ExportFormat.<para/>
        /// </remarks>
        /// <seealso cref="GraphExport"/>
        /// <seealso cref="ASD.Graphs"/>
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
