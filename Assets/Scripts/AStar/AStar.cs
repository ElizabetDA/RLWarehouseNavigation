using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace AStarSearch
{
    public static class AStarSearch
    {
        public static List<Vector2Int> AStar(int[,] environmentMatrix, Vector2Int start, Vector2Int end)
        {
            Node endCell = new Node(end.x, end.y);
            Node startCell = new Node(start.x, start.y, endCell, null);

            PriorityQueue<Node, int> openList = new PriorityQueue<Node, int>();
            Dictionary<Node, Node> openListSet = new Dictionary<Node, Node>();
            openList.Enqueue(startCell, startCell.LenPathThrough);
            openListSet.Add(startCell, startCell);

            HashSet<Node> closedList = new HashSet<Node>();
            
            int matrixWidth = environmentMatrix.GetLength(1);
            int matrixHeight = environmentMatrix.GetLength(0);

            while (openList.Count > 0)
            {
                Node currentNode = openList.Dequeue();
                openListSet.Remove(currentNode);

                if (currentNode == endCell)
                {
                    List<Vector2Int> path = new List<Vector2Int>();
                    while (currentNode != null)
                    {
                        path.Add(new Vector2Int(currentNode.X, currentNode.Y));
                        currentNode = currentNode.Parent;
                    }
                    path.Reverse();
                    return path;
                }

                closedList.Add(currentNode);

                List<Node> neighbors = new List<Node>();
                Vector2Int[] directions = {
                    new Vector2Int(-1, 0),
                    new Vector2Int(1, 0),
                    new Vector2Int(0, 1),
                    new Vector2Int(0, -1)
                };

                foreach (Vector2Int dir in directions)
                {
                    int x = currentNode.X + dir.x;
                    int y = currentNode.Y + dir.y;

                    if (x >= 0 && x < matrixWidth && 
                        y >= 0 && y < matrixHeight && 
                        environmentMatrix[y, x] == 0)
                    {
                        neighbors.Add(new Node(x, y, endCell, currentNode));
                    }
                }

                foreach (Node neighbor in neighbors)
                {
                    if (closedList.Contains(neighbor)) continue;

                    int newG = currentNode.DistanceFromStart;
                    if (openListSet.TryGetValue(neighbor, out Node existingNode))
                    {
                        if (newG < existingNode.DistanceFromStart)
                        {
                            existingNode.ChangeParent(currentNode);
                            UpdateElement(openList, existingNode);
                        }
                    }
                    else
                    {
                        openList.Enqueue(neighbor, neighbor.LenPathThrough);
                        openListSet.Add(neighbor, neighbor);
                    }
                }
            }
            return null;
        }

        private static void UpdateElement(PriorityQueue<Node, int> queue, Node elementToUpdate)
        {
            var items = queue.UnorderedItems.ToList();
            queue.Clear();
            foreach (var item in items)
            {
                queue.Enqueue(
                    item.Element.Equals(elementToUpdate) ? elementToUpdate : item.Element,
                    item.Element.Equals(elementToUpdate) ? elementToUpdate.LenPathThrough : item.Priority
                );
            }
        }
    }

    internal class Node
    {
        public int X { get; }
        public int Y { get; }
        public int DistanceFromStart { get; private set; }
        private readonly int _distanceToEnd;
        public int LenPathThrough { get; private set; }
        public Node Parent;

        public Node(int x, int y, Node end = null, Node parent = null)
        {
            X = x;
            Y = y;
            Parent = parent;
            
            if (end != null)
            {
                _distanceToEnd = Math.Abs(end.X - x) + Math.Abs(end.Y - y);
                DistanceFromStart = parent?.DistanceFromStart + 1 ?? 0;
                LenPathThrough = _distanceToEnd + DistanceFromStart;
            }
        }

        public override bool Equals(object obj) => obj is Node n && X == n.X && Y == n.Y;
        public override int GetHashCode() => HashCode.Combine(X, Y);
        public static bool operator ==(Node a, Node b) => a?.Equals(b) ?? b is null;
        public static bool operator !=(Node a, Node b) => !(a == b);

        public void ChangeParent(Node newParent)
        {
            Parent = newParent;
            DistanceFromStart = newParent.DistanceFromStart + 1;
            LenPathThrough = DistanceFromStart + _distanceToEnd;
        }
    }
}