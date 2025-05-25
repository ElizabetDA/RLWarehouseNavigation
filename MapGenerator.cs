using System.Collections.Generic;
using UnityEngine;

public class MapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int width = 10;
    public int height = 10;
    public int seed = 12345;
    public int offsetX = 0;
    public int offsetY = 0;
    [Range(0f, 1f)]
    public float wallDensity = 0.3f;
    public int agentAmount = 1;

    [Header("Prefabs for Visualization")]
    public GameObject floorPrefab; 
    public GameObject wallPrefab;  
    public GameObject agentPrefab; 
    public GameObject pathPrefab;  

    [Header("Pathfinding")]
    public Pathfinder pathfinder;           
    public Vector2Int startPosition;        // корды старта (x, y)
    public Vector2Int finishPosition;       // корды финиша (x, y)

    public int[][] environmentMatrix;

    void Start()
    {
        GenerateMap();
        DrawMap();

        if (pathfinder != null)
        {
            List<Vector2Int> path = pathfinder.FindPath(
                environmentMatrix, 
                startPosition.x, startPosition.y, 
                finishPosition.x, finishPosition.y
            );

            for (int i = 0; i < path.Count; i++)
            {
                Vector2Int pos = path[i];
                environmentMatrix[pos.y][pos.x] = 3;
            }

            DrawMap();
        }
    }


    /////// 0 = пустая клетка, 1 = стена, 2 = робот.

    public void GenerateMap()
    {

        environmentMatrix = new int[height][];
        for (int y = 0; y < height; y++)
        {
            environmentMatrix[y] = new int[width];
        }

        System.Random rand = new System.Random(seed);

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                double val = rand.NextDouble();
                if (val < wallDensity)
                {
                    environmentMatrix[y][x] = 1; 
                }
                else
                {
                    environmentMatrix[y][x] = 0; 
                }
            }
        }
        for (int i = 0; i < agentAmount; i++)
        {
            int rx, ry;
            do
            {
                rx = rand.Next(width);
                ry = rand.Next(height);
            }
            while (environmentMatrix[ry][rx] != 0);

            environmentMatrix[ry][rx] = 2;
        }
    }

    public void DrawMap()
    {

        foreach (Transform child in transform)
        {
            Destroy(child.gameObject);
        }

        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                int cellValue = environmentMatrix[y][x];

                GameObject prefabToSpawn = null;

                if (cellValue == 0)
                {
                    prefabToSpawn = floorPrefab;
                }
                else if (cellValue == 1)
                {
                    prefabToSpawn = wallPrefab;
                }
                else if (cellValue == 2)
                {
                    prefabToSpawn = agentPrefab;
                }
                else if (cellValue == 3)
                {
                    prefabToSpawn = pathPrefab;
                }

                if (prefabToSpawn != null)
                {
                    Vector3 position = new Vector3(x, -y, 0f);
                    GameObject obj = Instantiate(prefabToSpawn, position, Quaternion.identity);
                    obj.transform.SetParent(this.transform);
                    obj.name = $"Cell_{x}_{y}_{cellValue}";
                }
            }
        }
    }
}
