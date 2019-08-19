using System;

namespace ASD.Graphs
{
    /// <summary>
    /// Pomocnicza struktura ułatwiająca konstruowanie ścieżek w grafach
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public struct PathsInfo
    {
        public double Dist;
        public Edge? Last;
        
        public static Edge[] ConstructPath(int s, int t, PathsInfo[] pi)
        {
            if (pi[s].Dist != 0.0 || pi[s].Last != null)
                throw new ArgumentException("Incorrect paths infos (invalid source vertex)");
            
            if (pi[t].Dist.IsNaN())
                return null;
            
            if (s == t)
                return new Edge[0];
            
            var edgesStack = new EdgesStack();
            
            for (var vert = t; vert != s; vert = pi[vert].Last.Value.From)
                edgesStack.Put(pi[vert].Last.Value);
            
            return edgesStack.ToArray();
        }

        public static Edge[] ConstructPath(int s, int t, PathsInfo[,] pi)
        {
            if (pi[s, t].Dist.IsNaN())
                return null;
            
            if (s == t)
                return new Edge[0];
            
            var edgesStack = new EdgesStack();
            for (var vert = t; vert != s; vert = pi[s, vert].Last.Value.From)
                edgesStack.Put(pi[s, vert].Last.Value);
            
            return edgesStack.ToArray();
        }
    }
}
