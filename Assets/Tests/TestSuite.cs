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
        int[][] environment = {
            new[] {0, 0, 0},
            new[] {0, 0, 0},
            new[] {0, 0, 0}
        };
        var (path, _) = ExecuteTest(environment, new Vector2(0, 0), new Vector2(2, 2));
        Assert.AreEqual(5, path.Count);
    }

    [Test]
    public void AStar_SpiralMaze_FindsPath()
    {
        int size = 9;
        var env = CreateSpiralMaze(size);
        var (path, _) = ExecuteTest(env, new Vector2(0, 0), new Vector2(size-1, size-1));
        Assert.IsNotNull(path);
    }

    [Test]
    public void AStar_MultipleDeadEnds_FindsPath()
    {
        var env = CreateDeadEndMaze(11, 11);
        var (path, _) = ExecuteTest(env, new Vector2(0, 0), new Vector2(10, 10));
        Assert.IsNotNull(path);
    }

    [Test]
    public void AStar_RandomObstacles_FindsPath()
    {
        var env = CreateRandomObstacles(20, 20, 0.25f);
        var (path, map) = ExecuteTest(env, new Vector2(0, 0), new Vector2(19, 19));
        Assert.IsNotNull(path);
    }

    #endregion

    #region Map Generators

    private int[][] CreateDeadEndMaze(int width, int height)
    {
        int[][] env = new int[height][];
        for (int y = 0; y < height; y++)
        {
            env[y] = new int[width];
            for (int x = 0; x < width; x++)
            {
                bool isWall = (x % 3 != 0) && (y % 3 != 0);
                env[y][x] = isWall ? 1 : 0;
            }
        }
        return env;
    }

private int[][] CreateSpiralMaze(int size)
{
    int[][] env = new int[size][];
    for (int y = 0; y < size; y++)
    {
        env[y] = new int[size];
        for (int x = 0; x < size; x++)
        {
            // Создаем спираль с явным проходом
            bool isWall = (x > 0 && x < size-1 && y > 0 && y < size-1) && 
                         !(x == 1 && y < size-2) && 
                         !(y == size-2 && x < size-2) && 
                         !(x == size-2 && y > 1);
            env[y][x] = isWall ? 1 : 0;
        }
    }
    return env;
}
private int[][] CreateRandomObstacles(int width, int height, float density)
{
    var rand = new System.Random();
    int[][] env = new int[height][];
    
    // Гарантированный проход
    for (int y = 0; y < height; y++)
    {
        env[y] = new int[width];
        for (int x = 0; x < width; x++)
        {
            // Явный проход по краям
            bool isBorder = x == 0 || y == 0 || x == width-1 || y == height-1;
            env[y][x] = isBorder ? 0 : (rand.NextDouble() < density ? 1 : 0);
        }
    }
    
    // Дополнительный центральный проход
    for (int i = 0; i < width; i++)
    {
        env[width/2][i] = 0;
        env[i][height/2] = 0;
    }
    
    return env;
}

    #endregion

    #region Test Execution

    private (List<int> path, int[][] env) ExecuteTest(int[][] environment, Vector2 start, Vector2 end)
    {
        // Гарантия свободных старта и финиша
        environment[(int)start.y][(int)start.x] = 0;
        environment[(int)end.y][(int)end.x] = 0;

        var path = AStarSearch.AStarSearch.AStar(environment, start, end);
        ValidatePath(environment, start, end, path);
        return (path, environment);
    }

    private void ValidatePath(int[][] grid, Vector2 start, Vector2 end, List<int> path)
    {
        if (path == null) return;

        int width = grid[0].Length;
        
        // Проверка начальной и конечной точек
        Assert.AreEqual(0, grid[(int)start.y][(int)start.x], "Start blocked");
        Assert.AreEqual(0, grid[(int)end.y][(int)end.x], "End blocked");

        // Проверка непрерывности пути
        for (int i = 1; i < path.Count; i++)
        {
            Vector2 prev = IndexToPos(path[i-1], width);
            Vector2 curr = IndexToPos(path[i], width);
            float dist = Vector2.Distance(prev, curr);
            Assert.AreEqual(1f, dist, $"Invalid step: {prev} -> {curr}");
        }

        // Проверка препятствий
        foreach (int index in path)
        {
            Vector2 pos = IndexToPos(index, width);
            Assert.AreEqual(0, grid[(int)pos.y][(int)pos.x], $"Path through wall at {pos}");
        }
    }

    #endregion

    #region Helpers

    private int PosToIndex(Vector2 pos, int width) => (int)(pos.x + pos.y * width);
    private Vector2 IndexToPos(int index, int width) => new Vector2(index % width, index / width);

    #endregion
}