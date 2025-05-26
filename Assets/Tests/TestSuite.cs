using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;
using AStarSearch;

public class AStarTests
{
    #region Test Cases

    [Test]
    public void AStar_SimplePathNoObstacles_CorrectPath()
    {
        int[,] environment = {
            {0, 0, 0},
            {0, 0, 0},
            {0, 0, 0}
        };
        var (path, _) = ExecuteTest(environment, new Vector2Int(0, 0), new Vector2Int(2, 2));
        Assert.AreEqual(5, path.Count);
    }

    [Test]
    public void AStar_SpiralMaze_FindsPath()
    {
        int size = 9;
        var env = CreateSpiralMaze(size);
        var (path, _) = ExecuteTest(env, new Vector2Int(0, 0), new Vector2Int(size-1, size-1));
        Assert.IsNotNull(path);
    }

    [Test]
    public void AStar_MultipleDeadEnds_FindsPath()
    {
        var env = CreateDeadEndMaze(11, 11);
        var (path, _) = ExecuteTest(env, new Vector2Int(0, 0), new Vector2Int(10, 10));
        Assert.IsNotNull(path);
    }

    [Test]
    public void AStar_RandomObstacles_FindsPath()
    {
        var env = CreateRandomObstacles(20, 20, 0.25f);
        var (path, _) = ExecuteTest(env, new Vector2Int(0, 0), new Vector2Int(19, 19));
        Assert.IsNotNull(path);
    }

    #endregion

    #region Map Generators

    private int[,] CreateDeadEndMaze(int width, int height)
    {
        int[,] env = new int[height, width];
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isWall = (x % 3 != 0) && (y % 3 != 0);
                env[y, x] = isWall ? 1 : 0;
            }
        }
        return env;
    }

    private int[,] CreateSpiralMaze(int size)
    {
        int[,] env = new int[size, size];
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                bool isWall = (x > 0 && x < size-1 && y > 0 && y < size-1) && 
                             !(x == 1 && y < size-2) && 
                             !(y == size-2 && x < size-2) && 
                             !(x == size-2 && y > 1);
                env[y, x] = isWall ? 1 : 0;
            }
        }
        return env;
    }

    private int[,] CreateRandomObstacles(int width, int height, float density)
    {
        var rand = new System.Random();
        int[,] env = new int[height, width];
        
        for (int y = 0; y < height; y++)
        {
            for (int x = 0; x < width; x++)
            {
                bool isBorder = x == 0 || y == 0 || x == width-1 || y == height-1;
                env[y, x] = isBorder ? 0 : (rand.NextDouble() < density ? 1 : 0);
            }
        }
        
        for (int i = 0; i < width; i++)
        {
            env[width/2, i] = 0;
            env[i, height/2] = 0;
        }
        
        return env;
    }

    #endregion

    #region Test Execution

    private (List<Vector2Int> path, int[,] env) ExecuteTest(int[,] environment, Vector2Int start, Vector2Int end)
    {
        environment[start.y, start.x] = 0;
        environment[end.y, end.x] = 0;

        var path = AStarSearch.AStarSearch.AStar(environment, start, end);
        ValidatePath(environment, start, end, path);
        return (path, environment);
    }

    private void ValidatePath(int[,] grid, Vector2Int start, Vector2Int end, List<Vector2Int> path)
    {
        if (path == null) return;

        int width = grid.GetLength(1);
        int height = grid.GetLength(0);

        Assert.AreEqual(0, grid[start.y, start.x], "Start blocked");
        Assert.AreEqual(0, grid[end.y, end.x], "End blocked");

        for (int i = 1; i < path.Count; i++)
        {
            Vector2Int prev = path[i-1];
            Vector2Int curr = path[i];
            int dx = Mathf.Abs(curr.x - prev.x);
            int dy = Mathf.Abs(curr.y - prev.y);
            Assert.IsTrue(dx + dy == 1, $"Invalid step: {prev} -> {curr}");
        }

        foreach (Vector2Int pos in path)
        {
            Assert.AreEqual(0, grid[pos.y, pos.x], $"Path through wall at {pos}");
        }
    }

    #endregion
}