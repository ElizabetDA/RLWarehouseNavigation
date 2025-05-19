using System.Numerics;
using AStarSearch;
using NUnit.Framework;

namespace AStarSearchTests
{
    [TestFixture]
    public class AStarTests
    {
        [Test]
        public void AStar_StraightLinePath_ReturnsCorrectPath()
        {
            // Arrange
            int[][] environment = [
                [0, 0, 0, 0, 0],
                [0, 0, 0, 0, 0],
                [0, 0, 0, 0, 0],
                [0, 0, 0, 0, 0],
                [0, 0, 0, 0, 0]
            ];
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(4, 4);

            // Act
            var result = AStarSearch.AStarSearch.AStar(environment, start, end);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(9));
            Assert.That(result[0], Is.EqualTo(0)); // start (0,0)
            Assert.That(result[8], Is.EqualTo(24)); // end (4,4)
        }
        
        [Test]
        public void AStar_StraightLinePath_ReturnsCorrectPath_1()
        {
            // Arrange
            int[][] environment = [
                [0, 0, 0, 0, 0, 0, 1, 0],
                [0, 1, 1, 1, 1, 0, 0, 0],
                [0, 1, 0, 0, 0, 0, 1, 0],
                [0, 1, 0, 1, 1, 1, 1, 0],
                [0, 1, 0, 0, 0, 0, 0, 0],
                [0, 1, 1, 1, 1, 0, 1, 0],
                [0, 0, 0, 0, 0, 0, 0, 0]
                
            ];
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(6, 4);

            // Act
            var result = AStarSearch.AStarSearch.AStar(environment, start, end);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(13));
            Assert.That(result[0], Is.EqualTo(0)); // start (0,0)
            Assert.That(result[12], Is.EqualTo(38)); // end (6,4)
        }
        
        [Test]
        public void AStar_StraightLinePath_ReturnsCorrectPath_2()
        {
            // Arrange
            int[][] environment = [
                [0, 0, 0, 0, 0, 0, 1, 0],
                [0, 1, 1, 1, 1, 0, 0, 0],
                [0, 1, 0, 0, 0, 0, 1, 0],
                [0, 1, 0, 1, 1, 1, 1, 0],
                [0, 1, 0, 0, 0, 0, 0, 0],
                [0, 1, 1, 1, 1, 0, 1, 0],
                [0, 0, 0, 0, 0, 0, 0, 0]
                
            ];
            Vector2 start = new Vector2(2, 2);
            Vector2 end = new Vector2(0, 3);

            // Act
            var result = AStarSearch.AStarSearch.AStar(environment, start, end);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(14));
            Assert.That(result[0], Is.EqualTo(18)); // start (0,0)
            Assert.That(result[13], Is.EqualTo(24)); // end (6,4)
        }

        [Test]
        public void AStar_NoPath_ReturnsNull()
        {
            // Arrange
            int[][] environment = [
                [0, 1, 1, 1, 1],
                [1, 1, 1, 1, 1],
                [1, 1, 1, 1, 1],
                [1, 1, 1, 1, 1],
                [1, 1, 1, 1, 0]
            ];
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(4, 4);

            // Act
            var result = AStarSearch.AStarSearch.AStar(environment, start, end);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void AStar_ObstaclePath_ReturnsCorrectPath()
        {
            // Arrange
            int[][] environment = [
                [0, 0, 0, 0, 0],
                [1, 1, 1, 0, 0],
                [0, 0, 0, 0, 0],
                [0, 1, 1, 1, 1],
                [0, 0, 0, 0, 0]
            ];
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(4, 4);

            // Act
            var result = AStarSearch.AStarSearch.AStar(environment, start, end);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain(0)); // start
            Assert.That(result, Does.Contain(24)); // end
        }

        [Test]
        public void Node_Equals_SameCoordinates_ReturnsTrue()
        {
            // Arrange
            var node1 = new Node(1, 1);
            var node2 = new Node(1, 1);

            // Act & Assert
            Assert.That(node1.Equals(node2), Is.True);
            Assert.That(node1 == node2, Is.True);
            Assert.That(node1 != node2, Is.False);
        }

        [Test]
        public void Node_Equals_DifferentCoordinates_ReturnsFalse()
        {
            // Arrange
            var node1 = new Node(1, 1);
            var node2 = new Node(2, 2);

            // Act & Assert
            Assert.That(node1.Equals(node2), Is.False);
            Assert.That(node1 == node2, Is.False);
            Assert.That(node1 != node2, Is.True);
        }

        [Test]
        public void Node_CompareOperators_CorrectComparison()
        {
            // Arrange
            var endNode = new Node(5, 5);
            var node1 = new Node(1, 1, endNode, null); // f = 8
            var node2 = new Node(2, 2, endNode, null); // f = 6

            // Act & Assert
            Assert.That(node1 > node2, Is.True);
            Assert.That(node2 < node1, Is.True);
        }

        [Test]
        public void Node_RecountF_UpdatesValuesCorrectly()
        {
            // Arrange
            var endNode = new Node(5, 5);
            var parentNode = new Node(1, 1, endNode, null);
            var node = new Node(2, 2, endNode, parentNode);

            // Act
            node.RecountF(parentNode);

            // Assert
            Assert.That(node.G(), Is.EqualTo(parentNode.G() + 1));
            Assert.That(node.Parent, Is.EqualTo(parentNode));
        }

        [Test]
        public void AStar_StartEqualsEnd_ReturnsSinglePointPath()
        {
            // Arrange
            int[][] environment = [
                [0, 0, 0],
                [0, 0, 0],
                [0, 0, 0]
            ];
            Vector2 start = new Vector2(1, 1);
            Vector2 end = new Vector2(1, 1);

            // Act
            var result = AStarSearch.AStarSearch.AStar(environment, start, end);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0], Is.EqualTo(4)); // (1,1) in 3x3 grid
        }

        [Test]
        public void AStar_OutOfBoundsStart_ReturnsNull()
        {
            // Arrange
            int[][] environment = [
                [0, 0, 0],
                [0, 0, 0],
                [0, 0, 0]
            ];
            Vector2 start = new Vector2(5, 5); // Out of bounds
            Vector2 end = new Vector2(1, 1);

            // Act
            var result = AStarSearch.AStarSearch.AStar(environment, start, end);

            // Assert
            Assert.That(result, Is.Null);
        }

        [Test]
        public void AStar_ComplexEnvironment_ReturnsValidPath()
        {
            // Arrange
            int[][] environment = [
                [0, 1, 0, 0, 0],
                [0, 1, 0, 1, 0],
                [0, 1, 0, 1, 0],
                [0, 1, 0, 1, 0],
                [0, 0, 0, 1, 0]
            ];
            Vector2 start = new Vector2(0, 0);
            Vector2 end = new Vector2(4, 4);

            // Act
            var result = AStarSearch.AStarSearch.AStar(environment, start, end);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result, Does.Contain(0)); // start
            Assert.That(result, Does.Contain(24)); // end
            Assert.That(result.Count, Is.LessThan(25)); // path should be less than full grid
        }

        [Test]
        public void Node_GetHashCode_SameCoordinates_SameHashCode()
        {
            // Arrange
            var node1 = new Node(2, 3);
            var node2 = new Node(2, 3);

            // Act & Assert
            Assert.That(node1.GetHashCode(), Is.EqualTo(node2.GetHashCode()));
        }
    }
}