using System;

namespace ASD.Graphs
{
    class Program
    {
        static void Main(string[] args)
        {
            RandomGraphGenerator generator = new RandomGraphGenerator(0);
            Graph g = generator.EulerGraph(typeof(AdjacencyListsGraph<AVLAdjacencyList>), true, 12, 0.75, 1, 100);
            var (weight, cycle) = g.BacktrackingTSP();
            Console.WriteLine(weight);
        }
    }
}