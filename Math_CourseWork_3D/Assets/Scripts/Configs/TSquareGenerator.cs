using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// T-Square 分形生成：
/// 初始输入为一个正方形轮廓点集合（格式：p0 p1 p2 p3 p0）。
/// 每次迭代：对上一代的每个正方形，在其四个顶点处生成边长为父正方形一半的新正方形（不保留父正方形，不生成中心方块）。
/// 提供：
/// — GenerateCells：返回结构化 SquareCell 列表（适合 mesh 模式）。
/// — Generate：返回折线路径点（适合 line 模式）。
/// </summary>
public static class TSquareGenerator
{
    /// <summary>生成单元列表（用于 mesh 绘制）</summary>
    public static List<SquareCell> GenerateCells(List<Vector3> input, int iterations)
    {
        var root = ExtractSquare(input);
        var current = new List<SquareCell> { root };

        var all = new List<SquareCell>(current);

        for (int i = 0; i < iterations; i++)
        {
            current = IterateOnce(current);
            all.AddRange(current);
        }

        return all;
    }

    /// <summary>生成折线路径（用于 LineRenderer）</summary>
    public static List<Vector3> Generate(List<Vector3> input, int iterations)
    {
        var cells = GenerateCells(input, iterations);
        return CellsToPath(cells);
    }

    // 从轮廓点提取初始正方形（假设输入闭合：v0 v1 v2 v3 v0）
    private static SquareCell ExtractSquare(List<Vector3> pts)
    {
        if (pts == null || pts.Count < 4)
        {
            // 回退一个默认正方形
            return new SquareCell(Vector3.zero, 10f);
        }

        Vector3 min = pts[0];
        Vector3 max = pts[0];
        foreach (var p in pts)
        {
            min = Vector3.Min(min, p);
            max = Vector3.Max(max, p);
        }

        Vector3 center = (min + max) * 0.5f;
        float size = Mathf.Abs(max.x - min.x); // 边长
        if (size <= 0f) size = 1f;
        return new SquareCell(center, size);
    }

    // 单次迭代：每个父方块 → 四个角方块（边长一半，中心在父方块每个顶点）
    private static List<SquareCell> IterateOnce(List<SquareCell> old)
    {
        var next = new List<SquareCell>(old.Count * 4);
        foreach (var sq in old)
        {
            float parentSize = sq.Size;
            float childSize = parentSize * 0.5f;
            float h = parentSize * 0.5f; // 父半边长

            // 父方块四角坐标
            Vector3 c = sq.Center;
            Vector3 bl = c + new Vector3(-h, -h, 0);
            Vector3 tl = c + new Vector3(-h,  h, 0);
            Vector3 tr = c + new Vector3( h,  h, 0);
            Vector3 br = c + new Vector3( h, -h, 0);

            next.Add(new SquareCell(bl, childSize));
            next.Add(new SquareCell(tl, childSize));
            next.Add(new SquareCell(tr, childSize));
            next.Add(new SquareCell(br, childSize));
        }
        return next;
    }

    // 将单元列表转换为折线路径（每个正方形闭合）
    private static List<Vector3> CellsToPath(List<SquareCell> cells)
    {
        var result = new List<Vector3>(cells.Count * 5);
        foreach (var c in cells)
        {
            var v = c.GetVertices(); // 4 顶点：左下 左上 右上 右下
            result.Add(v[0]);
            result.Add(v[1]);
            result.Add(v[2]);
            result.Add(v[3]);
            result.Add(v[0]); // 闭合
        }
        return result;
    }
}
