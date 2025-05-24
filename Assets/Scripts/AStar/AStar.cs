using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Utils;

namespace AStarSearch
{
    public static class AStarSearch
    {
        public static List<int> AStar(int[][] environmentMatrix, Vector2 start, Vector2 end)
        {
            Node endCell = new Node((int)end.x, (int)end.y);
            Node startCell = new Node((int)start.x, (int)start.y, endCell, null);

            PriorityQueue<Node, int>
                openList = new PriorityQueue<Node, int>(); // Очередь с приоритетом для открытых узлов
            Dictionary<Node, Node> openListSet = new Dictionary<Node, Node>(); // Множество открытых узлов
            openList.Enqueue(startCell, startCell.LenPathThrough);
            openListSet.Add(startCell, startCell);

            HashSet<Node> closedList = new HashSet<Node>(); // Множество закрытых (проверенных узлов)
            while (openList.Count > 0)
            {
                Node currentNode = openList.Dequeue();
                openListSet.Remove(currentNode);

                if (currentNode == endCell) // Если текущий узел конечный, то восстанавливаем путь
                {
                    List<int> path = new List<int>();
                    while (currentNode != null)
                    {
                        int coord = currentNode.X + currentNode.Y * environmentMatrix[0].Length;
                        path.Add(coord);
                        currentNode = currentNode.Parent;
                    }

                    path.Reverse();
                    return path;
                }

                closedList.Add(currentNode); //  Иначе этот узел убираем в проверенные и обрабатываем его соседей

                List<Node> neighbors = new List<Node>();
                for (int i = -2; i <= 2; i++) // Генерация всех допустимых соседей
                {
                    if (i == 0)
                    {
                        continue;
                    }

                    int x = currentNode.X + (i / 2);
                    int y = currentNode.Y + (i % 2);

                    if (x < 0 || x >= environmentMatrix[0].Length || y < 0 || y >= environmentMatrix.Length ||
                        environmentMatrix[y][x] != 0)
                    {
                        continue;
                    }

                    Node neighbor = new Node(x, y, endCell, currentNode);
                    neighbors.Add(neighbor);
                }

                foreach (Node t in neighbors) // Проверка соседей
                {
                    if (closedList.Contains(t))
                    {
                        continue;
                    }

                    int newG = currentNode.DistanceFromStart;
                    if (openListSet.TryGetValue(t, out Node element)) // Если сосед уже находится в открытых узлах
                    {
                        if (newG < element.DistanceFromStart) // и если длина пройденного пути предыдущего родителя была больше
                        {
                            element.ChangeParent(currentNode); // то изменяем родителя и длину пути соответствующего узла
                            UpdateElement(openList, element);
                        }
                    }
                    else // Иначе просто добавляем в открытые узлы
                    {
                        openList.Enqueue(t, t.LenPathThrough);
                        openListSet.Add(t, t);
                    }
                }
            }

            return null; // Возвращается если в цикле не была достигнута конечная клетка
        }

        private static void UpdateElement(PriorityQueue<Node, int> queue, Node elementToUpdate) // Обновление очереди в
        {
            List<(Node Element, int Priority)> items = queue.UnorderedItems.ToList();  // связи с изменением родителя
            queue.Clear();                                                             // одного из элементов
            foreach ((Node Element, int Priority) item in items)
            {
                if (!item.Element.Equals(elementToUpdate))
                {
                    queue.Enqueue(item.Element, item.Priority);
                }
                else
                {
                    queue.Enqueue(elementToUpdate, elementToUpdate.LenPathThrough);
                }
            }
        }
    }

    class Node
    {
        public int X { get; } // Координата по абсциссе
        public int Y { get; } // Координата по ординате
        public int DistanceFromStart { get; private set; } // Длина маршрута пройденного от начала до этой клетки
        private int _distanceToEnd; // Минимально возможная длина пути от этой до конечной клетки (Без учёта препятствий)
        public int LenPathThrough { get; private set; } // Предполагаемая длина пути от начальной до конечной клетки
        public Node Parent; // Предыдущая клетка пути начальная - эта клетка

        public Node(int x, int y)
        {
            X = x;
            Y = y;
            Parent = null;
        }

        public Node(int x, int y, Node end, Node parent)
        {
            X = x;
            Y = y;
            _distanceToEnd = Mathf.Abs(end.X - x) + Mathf.Abs(end.Y - y);
            DistanceFromStart = parent is null ? 0 : parent.DistanceFromStart + 1;
            Parent = parent;
            LenPathThrough = _distanceToEnd + DistanceFromStart;
        }

        public override bool Equals(object obj)
        {
            if (obj is null || GetType() != obj.GetType())
            {
                return false;
            }

            Node other = (Node)obj;
            return X == other.X && Y == other.Y;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(X, Y);
        }


        public static bool operator >(Node n1, Node n2)
        {
            return n1.LenPathThrough > n2.LenPathThrough;
        }

        public static bool operator <(Node n1, Node n2)
        {
            return n2 > n1;
        }

        public static bool operator ==(Node n1, Node n2)
        {
            if (n1 is null && n2 is null)
            {
                return true;
            }

            if (n1 is null || n2 is null)
            {
                return false;
            }

            return n1.X == n2.X && n1.Y == n2.Y;
        }

        public static bool operator !=(Node n1, Node n2)
        {
            return !(n1 == n2);
        }

        public void ChangeParent(Node parent) // Изменение родителя клетки на parent и перерасчёт DistanceFromStart и LenPathThrough
        {
            DistanceFromStart = parent.DistanceFromStart + 1;
            Parent = parent;
            LenPathThrough = DistanceFromStart + _distanceToEnd;
        }
    }
}