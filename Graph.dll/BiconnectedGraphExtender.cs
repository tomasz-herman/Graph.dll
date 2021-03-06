﻿using System;
using System.Collections.Generic;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o algorytm wyznaczania punktów artykulacji i składowych dwuspójnych
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class BiconnectedGraphExtender
    {
        /// <summary>
        /// Wyznacza składowe dwuspójne
        /// </summary>
        /// <param name="g">Badany graf</param>
        /// <returns>
        /// Krotka (count, bcc, ap) składająca się z liczby składowych dwuspójnych,
        /// grafu opisującego te składowe i tablicy zawierającej punkty artykulacji
        /// </returns>
        /// <exception cref="ArgumentException">Gdy uruchomiona dla grafu skierowanego</exception>
        /// <remarks>
        /// Wagi krawędzi w grafie opisującym składowe dwuspójne (bcc) odpowiadają numerom tych składowych.<para/>
        /// Metoda uruchomiona dla grafu skierowanego zgłasza wyjątek ArgumentException.
        /// </remarks>
        /// <seealso cref="BiconnectedGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (int count, Graph bcc, int[] ap) BiconnectedComponents(this Graph g)
        {
            if (g.Directed)
                throw new ArgumentException("Directed graphs are not allowed");
            
            var bcc = g.IsolatedVerticesGraph();
            var edgesStack = new EdgesStack();
            var discovery = new int[g.VerticesCount];
            var low = new int[g.VerticesCount];
            var visited = new bool[g.VerticesCount];
            var isArticulation = new bool[g.VerticesCount];
            var count = 0;
            var time = 0;
            
            
            int GetArticulationPoints(int i, int d)
            {
                var children = 0;
                visited[i] = true;
                low[i] = discovery[i] = time++;
                foreach (var edge in g.OutEdges(i))
                {
                    edgesStack.Put(edge);
                    if (!visited[edge.To])
                    {
                        var ap = GetArticulationPoints(edge.To, d);
                        if (low[i] > ap)
                        {
                            low[i] = ap;
                        }

                        if ((i == d || discovery[i] > ap) && i != d) continue;
                        if (i != d || ++children > 1)
                        {
                            isArticulation[i] = true;
                        }
                        Edge e;
                        do
                        {
                            e = edgesStack.Get();
                            bcc.AddEdge(e.From, e.To, count);
                        }
                        while (e.From != i);
                        count++;
                    }
                    else if (low[i] > discovery[edge.To])
                    {
                        low[i] = discovery[edge.To];
                    }
                }
                return low[i];
            }
            
            
            for (var i = 0; i < g.VerticesCount; i++)
            {
                if (!visited[i])
                {
                    GetArticulationPoints(i, i);
                }
            }
            
            var list = new List<int>();
            for (var j = 0; j < g.VerticesCount; j++)
            {
                if (isArticulation[j])
                {
                    list.Add(j);
                }
            }
            
            return (count, bcc, list.ToArray());
        }
        
    }
}
