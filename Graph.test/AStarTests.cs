using FluentAssertions;
using NUnit.Framework;

namespace ASD.Graphs
{
    public class AStarTests
    {
        [Test]
        public void AStarTest()
        {
            Graph g = new AdjacencyMatrixGraph(false, 4);
            g.Add(new Edge(0, 1, 13));
            g.Add(new Edge(0, 2, 1));
            g.Add(new Edge(2, 1, 2));
            g.Add(new Edge(0, 3, 11));
            g.Add(new Edge(1, 3, 7));
            
            var exists = g.AStar(0, 3, out var edges);
            
            exists.Should().BeTrue();
            edges.Should().ContainInOrder(new Edge(0, 2, 1), new Edge(2, 1, 2), new Edge(1, 3, 7));
        }
    }
}