using System.Collections.Generic;
using UnityEngine;

public static class VicsekGenerator
{
    // 新增：直接返回单元列表
    public static List<SquareCell> GenerateCells(List<Vector3> input, int iterations)
    {
        var root = ExtractSquare(input);
        var current = new List<SquareCell> { root };
        for (int i = 0; i < iterations; i++)
            current = IterateOnce(current);
        return current;
    }

    public static List<Vector3> Generate(List<Vector3> input, int iterations)
    {
        var cells = GenerateCells(input, iterations);
        return CellsToPath(cells);
    }

    private static SquareCell ExtractSquare(List<Vector3> pts)
    {
        Vector3 min = pts[0], max = pts[0];
        foreach (var p in pts)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }
        Vector3 center = (min + max) * 0.5f;
        float size = Mathf.Abs(max.x - min.x);
        return new SquareCell(center, size);
    }

    // 修正：中心 + 四个角
    private static List<SquareCell> IterateOnce(List<SquareCell> old)
    {
        var next = new List<SquareCell>();
        foreach (var sq in old)
        {
            float childSize = sq.Size / 3f;
            float off = sq.Size / 3f;

            // 中心
            next.Add(new SquareCell(sq.Center, childSize));
            // 四个角 (NW, NE, SW, SE)
            next.Add(new SquareCell(sq.Center + new Vector3(-off,  off, 0), childSize));
            next.Add(new SquareCell(sq.Center + new Vector3( off,  off, 0), childSize));
            next.Add(new SquareCell(sq.Center + new Vector3(-off, -off, 0), childSize));
            next.Add(new SquareCell(sq.Center + new Vector3( off, -off, 0), childSize));
        }
        return next;
    }

    // line 模式需要的点串（每个方块闭合）
    private static List<Vector3> CellsToPath(List<SquareCell> cells)
    {
        var result = new List<Vector3>();
        foreach (var c in cells)
        {
            var v = c.GetVertices();
            result.Add(v[0]);
            result.Add(v[1]);
            result.Add(v[2]);
            result.Add(v[3]);
            result.Add(v[0]); // 闭合
        }
        return result;
    }
}
