using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3D T-Square 分形：每次把父立方体划分为 3x3x3 的 27 子立方体，仅保留 8 个角立方体。
/// 这与 2D T-Square（每次在四个角生成方块）一致的 3D 扩展（八个角）。
/// </summary>
public static class TSquare3DGenerator
{
    /// <param name="keepAllGenerations">为 true 时，返回包含历代所有生成的立方体；为 false 时仅返回当前迭代生成的立方体。</param>
    public static List<CubeCell> GenerateCells(Vector3 rootCenter, float rootSize, int iterations)
    {
        var root = new CubeCell(rootCenter, rootSize);
        var current = new List<CubeCell> { root };
        var all = new List<CubeCell>(current);

        for (int i = 0; i < iterations; i++)
        {
            current = IterateOnce(current);
            all.AddRange(current);
        }

        return all;
    }

    private static List<CubeCell> IterateOnce(List<CubeCell> parents)
    {
        var next = new List<CubeCell>(parents.Count * 8);
        foreach (var p in parents)
            next.AddRange(SplitCubeCorners(p));
        return next;
    }

    // 父立方体 → 8 个角立方体（子边长 = 父边长 / 3，子中心位于 (±1, ±1, ±1) * childSize 处）
    private static IEnumerable<CubeCell> SplitCubeCorners(CubeCell parent)
    {
        float s = parent.Size;
        float childSize = s / 3f;
        float offset = s * 0.5f;
        Vector3 c = parent.Center;

        for (int dx = -1; dx <= 1; dx += 2)
        for (int dy = -1; dy <= 1; dy += 2)
        for (int dz = -1; dz <= 1; dz += 2)
        {
            Vector3 corner = c + new Vector3(dx * offset, dy * offset, dz * offset);
            yield return new CubeCell(corner, childSize);
        }
    }
}