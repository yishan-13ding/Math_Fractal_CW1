using System.Collections.Generic;
using UnityEngine;

public static class SierpinskiGenerator
{
    // mesh 模式：返回三角单元
    public static List<TriangleCell> GenerateCells(List<Vector3> seed, int iterations)
    {
        TriangleCell root = ExtractRoot(seed);
        var current = new List<TriangleCell> { root };
        for (int i = 0; i < iterations; i++)
            current = Subdivide(current);
        return current;
    }

    // line 模式兼容：输出所有三角轮廓（闭合）
    public static List<Vector3> Generate(List<Vector3> seed, int iterations)
    {
        var cells = GenerateCells(seed, iterations);
        var result = new List<Vector3>(cells.Count * 4);
        foreach (var c in cells)
        {
            var v = c.GetVertices();
            result.Add(v[0]); result.Add(v[1]); result.Add(v[2]); result.Add(v[0]);
        }
        return result;
    }

    private static TriangleCell ExtractRoot(List<Vector3> pts)
    {
        if (pts == null || pts.Count < 3)
        {
            // 若初始形状不合法，构造一个默认等边三角
            float size = 10f;
            float h = Mathf.Sqrt(3f) / 2f * size;
            Vector3 v0 = new Vector3(0, 2f / 3f * h, 0);
            Vector3 v1 = new Vector3(-size / 2f, -h / 3f, 0);
            Vector3 v2 = new Vector3(size / 2f, -h / 3f, 0);
            return new TriangleCell(v0, v1, v2);
        }
        // 直接取前三个点（确保你的初始 shape 返回的是等边三角）
        return new TriangleCell(pts[0], pts[1], pts[2]);
    }

    // 每个父三角 → 3 个角三角（不保留中间倒置三角）
    private static List<TriangleCell> Subdivide(List<TriangleCell> src)
    {
        var dst = new List<TriangleCell>(src.Count * 3);
        foreach (var tri in src)
        {
            var v = tri.GetVertices();
            Vector3 v0 = v[0];
            Vector3 v1 = v[1];
            Vector3 v2 = v[2];

            // 中点
            Vector3 m01 = (v0 + v1) * 0.5f;
            Vector3 m12 = (v1 + v2) * 0.5f;
            Vector3 m20 = (v2 + v0) * 0.5f;

            // 3 个角三角
            dst.Add(new TriangleCell(v0, m01, m20));
            dst.Add(new TriangleCell(m01, v1, m12));
            dst.Add(new TriangleCell(m20, m12, v2));
        }
        return dst;
    }
}
