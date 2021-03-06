﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ASD.Graphs
{
    /// <summary>
    /// Rozszerzenie klasy <see cref="Graph"/> o algorytmy wyznaczania maksymalnego przepływu o minimalnym koszcie
    /// </summary>
    /// <seealso cref="ASD.Graphs"/>
    public static class MinCostFlowGraphExtender
    {
        
        private delegate bool FordBellmanShortestPaths(Graph g, int s, out PathsInfo[] d);
        
        /// <summary>
        /// Wyznacza maksymalny przepływ o minimalnym koszcie
        /// </summary>
        /// <param name="g">Graf przepustowości</param>
        /// <param name="c">Graf kosztów</param>
        /// <param name="source">Wierzchołek źródłowy</param>
        /// <param name="target">Wierzchołek docelowy</param>
        /// <param name="parallel">Informacja czy algorytm ma korzystać z równoległej wersji algorytmu Forda-Bellmana</param>
        /// <param name="mf">Metoda wyznaczania wstępnego maksymalnego przepływu (bez uwzględniania kosztów)</param>
        /// <param name="af">Metoda powiększania przepływu</param>
        /// <param name="matrixToAVL">Czy optymalizować sposób reprezentacji grafu rezydualnego</param>
        /// <returns>
        /// Krotka (value, cost, flow) składająca się z
        /// wartości maksymalnego przepływu, jego kosztu i grafu opisującego ten przepływ
        /// </returns>
        /// <exception cref="ArgumentException"></exception>
        /// <remarks>
        /// Można wybrać metodę wyznaczania wstępnego maksymalnego przepływu (parametr mf).<para/>
        /// Domyślna wartość mf (null) oznacza konstrukcję wstępnego przepływu z wykorzystaniem sztucznej krawędzi.<para/>
        /// Można wybrać metodę powiększania przepływu (parametr pf).<para/>
        /// Znaczenie tego parametru zależy od wybranej metody wyznaczania wstępnego maksymalnego przepływu
        /// (parametr mf), dla niektórych wartości parametru mf
        /// (np. <see cref="MaxFlowGraphExtender.FordFulkersonDinicMaxFlow"/>)
        /// jawne wskazanie metody powiększania przepływu jest konieczne
        /// (pozostawienie domyślnej wartości parametru pf (null)
        /// spowoduje zgłoszenie wyjątku <see cref="ArgumentException"/>).<para/>
        /// Metoda uruchomiona dla grafu nieskierowanego lub grafu
        /// z ujemnymi przepustowościami krawędzi zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Gdy grafy przepustowości i kosztów mają różną strukturę lub parametry
        /// source i target są równe metoda również zgłasza wyjątek <see cref="ArgumentException"/>.<para/>
        /// Gdy w grafie przepustowości istnieją krawędzie w obu kierunkach
        /// pomiędzy parą wierzchołków metoda również zgłasza wyjątek <see cref="ArgumentException"/>.
        /// </remarks>
        /// <seealso cref="MinCostFlowGraphExtender"/>
        /// <seealso cref="ASD.Graphs"/>
        public static (double value, double cost, Graph flow) MinCostFlow(this Graph g, Graph c, int source, int target, bool parallel = false, MaxFlow mf = null, AugmentFlow af = null, bool matrixToAVL = true)
        {
            if (!g.Directed)
                throw new ArgumentException("Undirected graphs are not allowed");
            if (source == target)
                throw new ArgumentException("Source and target must be different");
            if (g.VerticesCount != c.VerticesCount)
                throw new ArgumentException("Inconsistent capacity and cost graphs");

            var fordBellmanShortestPaths = parallel ? ShortestPathsGraphExtender.FordBellmanShortestPathsParallel : new FordBellmanShortestPaths(ShortestPathsGraphExtender.FordBellmanShortestPaths);

            var gOut = new HashSet<int>();
            var cOut = new HashSet<int>();
            
            for (var i = 0; i < g.VerticesCount; i++)
            {
                gOut.Clear();
                cOut.Clear();
                foreach (var edge in g.OutEdges(i))
                {
                    if (edge.Weight < 0.0) 
                        throw new ArgumentException("Negative capacity edges are not allowed");
                    if (!g.GetEdgeWeight(edge.To, edge.From).IsNaN())
                        throw new ArgumentException(
                            "Edges in both directions between pair of vertices are not allowed");
                    gOut.Add(edge.To);
                }

                foreach (var edge in c.OutEdges(i))
                    cOut.Add(edge.To);
                
                if (!gOut.SetEquals(cOut))
                    throw new ArgumentException("Inconsistent capacity and cost graphs");
            }
            
            var tempCost = double.NaN;
            var tempFlow = double.NaN;
            var maxFlow = double.NaN;
            Graph flow;
            if (mf != null)
                (maxFlow, flow) = mf(g, source, target, af, matrixToAVL);
            else
            {
                if (!(tempFlow = g.GetEdgeWeight(source, target)).IsNaN()) 
                    g.DelEdge(source, target);
                if (!(tempCost = c.GetEdgeWeight(source, target)).IsNaN()) 
                    c.DelEdge(source, target);
                flow = g.IsolatedVerticesGraph();
                var maxPossibleFlow = g.OutEdges(source).Sum(e => e.Weight);
                g.AddEdge(source, target, maxPossibleFlow + 1.0);
                flow.AddEdge(source, target, maxPossibleFlow + 1.0);
                var maxPossibleCost = 0.0;
                for (var i = 0; i < c.VerticesCount; i++)
                    foreach (var edge4 in c.OutEdges(i))
                    {
                        maxPossibleCost += Math.Abs(edge4.Weight);
                        flow.AddEdge(edge4.From, edge4.To, 0.0);
                    }
                c.AddEdge(source, target, maxPossibleCost + 1.0);
            }
            
            var residualFlow = flow.IsolatedVerticesGraph();
            var residualCost = flow.IsolatedVerticesGraph();
            for (var i = 0; i < flow.VerticesCount; i++)
                foreach (var edge in flow.OutEdges(i))
                {
                    var something = Math.Min(g.GetEdgeWeight(edge.From, edge.To), double.MaxValue) - edge.Weight;
                    if (something > 0.0)
                    {
                        residualFlow.AddEdge(edge.From, edge.To, something);
                        residualCost.AddEdge(edge.From, edge.To, c.GetEdgeWeight(edge.From, edge.To));
                    }

                    if (edge.Weight > 0.0)
                    {
                        residualFlow.AddEdge(edge.To, edge.From, edge.Weight);
                        residualCost.AddEdge(edge.To, edge.From, -c.GetEdgeWeight(edge.From, edge.To));
                    }
                }

            var foundFlow = 0.0;
            while (!fordBellmanShortestPaths(residualCost, target, out var pi))
            {
                var cycle = residualCost.FindNegativeCostCycle(pi).cycle;
                var cycleMaxFlow = double.PositiveInfinity;
                var flag = false;
                foreach (var edge in cycle)
                {
                    if (edge.From == target && edge.To == source) 
                        flag = true;
                    var weight = residualFlow.GetEdgeWeight(edge.From, edge.To);
                    if (cycleMaxFlow > weight) 
                        cycleMaxFlow = weight;
                }
                if (flag) foundFlow += cycleMaxFlow;
                foreach (var edge in cycle)
                {
                    if (flag = (flow.GetEdgeWeight(edge.To, edge.From) > 0.0))
                    {
                        var weight = flow.ModifyEdgeWeight(edge.To, edge.From, -cycleMaxFlow);
                        if (weight < 0.0)
                        {
                            flow.ModifyEdgeWeight(edge.To, edge.From, -weight);
                            flow.ModifyEdgeWeight(edge.From, edge.To, -weight);
                        }
                    }
                    else
                        flow.ModifyEdgeWeight(edge.From, edge.To, cycleMaxFlow);

                    if (residualFlow.ModifyEdgeWeight(edge.From, edge.To, -cycleMaxFlow) == 0.0)
                    {
                        residualFlow.DelEdge(edge);
                        residualCost.DelEdge(edge);
                    }
                    
                    if (residualFlow.ModifyEdgeWeight(edge.To, edge.From, cycleMaxFlow).IsNaN())
                    {
                        residualFlow.AddEdge(edge.To, edge.From, cycleMaxFlow);
                        var weight = (flag ? c.GetEdgeWeight(edge.To, edge.From) : c.GetEdgeWeight(edge.From, edge.To));
                        residualCost.AddEdge(edge.To, edge.From, flag ? weight : (-weight));
                    }
                }
            }
            if (mf == null)
            {
                g.DelEdge(source, target);
                c.DelEdge(source, target);
                flow.DelEdge(source, target);
                if (!tempFlow.IsNaN())
                {
                    foundFlow += tempFlow;
                    g.AddEdge(source, target, tempFlow);
                    c.AddEdge(source, target, tempCost);
                    flow.AddEdge(source, target, tempFlow);
                }
                maxFlow = foundFlow;
            }
            var cost = 0.0;
            for (var k = 0; k < flow.VerticesCount; k++)
                cost += flow.OutEdges(k).Sum(e => e.Weight * c.GetEdgeWeight(e.From, e.To));
            return (maxFlow, cost, flow);
        }
    }
}
