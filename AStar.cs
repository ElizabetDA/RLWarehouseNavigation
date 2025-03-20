using System.Numerics;

namespace AStarSearch
{
    public static class AStarSearch
    {
        public static List<int>? AStar(int[][] environmentMatrix, Vector2 start, Vector2 end)
        {
            var endCell = new Node((int)end[0], (int)end[1]);
            var startCell = new Node((int)start[0], (int)start[1], endCell, null);


            var openList = new PriorityQueue<Node, int>();
            var openListSet = new Dictionary<Node, Node>();
            openList.Enqueue(startCell, startCell.F());
            openListSet.Add(startCell, startCell);

            var closedList = new HashSet<Node>();
            while (openList.Count > 0)
            {
                var currentNode = openList.Dequeue();
                openListSet.Remove(currentNode);

                if (currentNode == endCell)
                {
                    var path = new List<int>();
                    while (currentNode != null)
                    {
                        var coord = currentNode.X() + currentNode.Y() * environmentMatrix[0].Length;
                        path.Add(coord);
                        currentNode = currentNode.Parent;
                    }

                    path.Reverse();
                    return path;
                }

                closedList.Add(currentNode);

                var neighbors = new List<Node>();
                for (var i = -2; i <= 2; i++)
                {
                    if (i == 0)
                    {
                        continue;
                    }

                    var x = currentNode.X() + (i / 2);
                    var y = currentNode.Y() + (i % 2);

                    if (x < 0 || x >= environmentMatrix[0].Length || y < 0 || y >= environmentMatrix.Length)
                    {
                        continue;
                    }

                    if (environmentMatrix[y][x] != 0)
                    {
                        continue;
                    }

                    var neighbor = new Node(x, y, endCell, currentNode);
                    neighbors.Add(neighbor);
                }

                foreach (var t in neighbors)
                {
                    if (closedList.Contains(t))
                    {
                        continue;
                    }

                    var newG = currentNode.G();
                    if (openListSet.TryGetValue(t, out var element))
                    {
                        if (newG < element.G())
                        {
                            element.RecountF(currentNode);
                            UpdateElement(openList, element);
                        }
                    }
                    else
                    {
                        openList.Enqueue(t, t.F());
                        openListSet.Add(t, t);
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
                if (!item.Element.Equals(elementToUpdate))
                {
                    queue.Enqueue(item.Element, item.Priority);
                }
                else
                {
                    queue.Enqueue(elementToUpdate, elementToUpdate.F());
                }
            }
        }
    }

    class Node
    {
        private readonly int x;
        private readonly int y;
        private int g;
        private int h;
        private int f;
        public Node? Parent;

        public Node(int x, int y)
        {
            this.x = x;
            this.y = y;
            Parent = null;
        }

        public Node(int x, int y, Node end, Node? parent)
        {
            this.x = x;
            this.y = y;
            h = Math.Abs(end.x - x) + Math.Abs(end.y - y);
            g = parent is null ? 0 : parent.g + 1;
            this.Parent = parent;
            f = h + g;
        }

        public override bool Equals(object? obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            var other = (Node)obj;
            return x == other.x && y == other.y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(x, y);
        }


        public static bool operator >(Node n1, Node n2)
        {
            return n1.f > n2.f;
        }

        public static bool operator <(Node n1, Node n2)
        {
            return n2 > n1;
        }

        public static bool operator ==(Node? n1, Node? n2)
        {
            if (n1 is null && n2 is null)
            {
                return true;
            }

            if (n1 is null || n2 is null)
            {
                return false;
            }

            return n1.x == n2.x && n1.y == n2.y;
        }

        public static bool operator !=(Node? n1, Node? n2)
        {
            return !(n1 == n2);
        }

        public int F()
        {
            return f;
        }

        public void RecountF(Node parent)
        {
            g = parent.g + 1;
            this.Parent = parent;
            f = g + h;
        }

        public int X()
        {
            return x;
        }

        public int Y()
        {
            return y;
        }

        public int G()
        {
            return g;
        }
    }
}