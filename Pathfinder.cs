using System.Collections.Generic;
using UnityEngine;

public class Pathfinder : MonoBehaviour
{
    public List<Vector2Int> FindPath(int[][] matrix, int startX, int startY, int finishX, int finishY)
    {
        // ...  границы
        if (matrix == null || matrix.Length == 0 || matrix[0].Length == 0) 
            return new List<Vector2Int>();

        int height = matrix.Length;
        int width = matrix[0].Length;

        if (!IsInside(startX, startY, width, height)) return new List<Vector2Int>();
        if (!IsInside(finishX, finishY, width, height)) return new List<Vector2Int>();

        Node[][] nodes = new Node[height][];
        for (int y = 0; y < height; y++)
        {
            nodes[y] = new Node[width];
            for (int x = 0; x < width; x++)
            {
                nodes[y][x] = new Node
                {
                    x = x,
                    y = y,
                    gCost = float.MaxValue,
                    hCost = 0,
                    parent = null
                };
            }
        }

        Node startNode = nodes[startY][startX];
        startNode.gCost = 0;
        startNode.hCost = Heuristic(startX, startY, finishX, finishY);

        List<Node> openSet = new List<Node> { startNode };
        HashSet<Node> closedSet = new HashSet<Node>();

        while (openSet.Count > 0)
        {
            Node current = openSet[0];
            for (int i = 1; i < openSet.Count; i++)
            {
                Node candidate = openSet[i];
                float cf = candidate.fCost;
                float mf = current.fCost;

                if (cf < mf || (Mathf.Approximately(cf, mf) && candidate.hCost < current.hCost))
                {
                    current = candidate;
                }
            }

            if (current.x == finishX && current.y == finishY)
            {
                return ReconstructPath(current);
            }

            openSet.Remove(current);
            closedSet.Add(current);

            for (int dir = 0; dir < 4; dir++)
            {
                int nx = current.x;
                int ny = current.y;

                if (dir == 0) ny -= 1;  // вверх
                if (dir == 1) ny += 1;  // вниз 
                if (dir == 2) nx -= 1;  // влево <-
                if (dir == 3) nx += 1;  // враво ->

                if (!IsInside(nx, ny, width, height))
                    continue;

                if (matrix[ny][nx] == 1 || matrix[ny][nx] == 2)
                    continue;

                Node neighborNode = nodes[ny][nx];
                if (closedSet.Contains(neighborNode))
                    continue;

                float tentativeG = current.gCost + 1;

                if (!openSet.Contains(neighborNode))
                {
                    openSet.Add(neighborNode);
                }
                else if (tentativeG >= neighborNode.gCost)
                {
                    continue;
                }

                neighborNode.gCost = tentativeG;
                neighborNode.hCost = Heuristic(nx, ny, finishX, finishY);
                neighborNode.parent = current;
            }
        }


        return new List<Vector2Int>();
    }
    float Heuristic(int x1, int y1, int x2, int y2)
    {
        return Mathf.Abs(x1 - x2) + Mathf.Abs(y1 - y2);
    }
    bool IsInside(int x, int y, int width, int height)
    {
        return x >= 0 && y >= 0 && x < width && y < height;
    }
    
    List<Vector2Int> ReconstructPath(Node endNode)
    {
        List<Vector2Int> path = new List<Vector2Int>();
        Node current = endNode;
        while (current != null)
        {
            path.Add(new Vector2Int(current.x, current.y));
            current = current.parent;
        }
        path.Reverse();
        return path;
    }

    class Node
    {
        public int x, y;
        public float gCost;
        public float hCost;
        public float fCost => gCost + hCost;
        public Node parent;
    }
}
