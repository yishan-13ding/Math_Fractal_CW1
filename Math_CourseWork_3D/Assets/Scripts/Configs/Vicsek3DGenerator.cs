using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// 3D Vicsek 分型：每次把父立方体划分为 3x3x3 = 27 个子立方体，保留一部分形成下一代。
/// 这里实现两种常见选择模式：
/// PatternMode.UnionFaceRules:
///     把每个面的 2D Vicsek(中心+四角)投影到 3D 后合并，得到：
///     - 8 个角
///     - 6 个面中心
///     - 1 个总体中心   => 共 15 个子立方体
/// PatternMode.Standard7:
///     标准 3D Vicsek：仅保留总体中心 + 六个面中心 => 7 个子立方体
/// 默认使用 UnionFaceRules，以更贴近你“每个面仍按 2D Vicsek” 的描述。
/// </summary>
public static class Vicsek3DGenerator
{
    public enum PatternMode
    {
        UnionFaceRules, // 15 子块（角 + 面中心 + 体中心）
        Standard7       // 7 子块（体中心 + 六面中心）
    }

    public static List<CubeCell> GenerateCells(Vector3 rootCenter, float rootSize, int iterations, PatternMode mode = PatternMode.UnionFaceRules, bool keepAllGenerations = false)
    {
        var root = new CubeCell(rootCenter, rootSize);
        var current = new List<CubeCell> { root };
        var all = new List<CubeCell>(current);

        for (int i = 0; i < iterations; i++)
        {
            current = IterateOnce(current, mode);
            //if (keepAllGenerations) all.AddRange(current);
        }
        return current;
        //return keepAllGenerations ? all : current;
    }

    private static List<CubeCell> IterateOnce(List<CubeCell> parents, PatternMode mode)
    {
        var next = new List<CubeCell>();
        foreach (var p in parents)
            next.AddRange(SplitCube(p, mode));
        return next;
    }

    /// <summary>
    /// 按模式从父立方体产生子立方体集合
    /// </summary>
    private static IEnumerable<CubeCell> SplitCube(CubeCell parent, PatternMode mode)
    {
        float s = parent.Size;
        float childSize = s / 3f;
        float offset = childSize; // 子中心偏移量（父中心 ± childSize，±3*childSize/??? -> 3 等分网格中坐标 -1,0,1 乘 childSize）
        Vector3 c = parent.Center;

        // 预先计算 27 个可能子中心（三个方向 -1,0,1）
        // 只挑选需要的坐标集合
        var result = new List<CubeCell>();

        // 体中心
        Vector3 center = c;

        // 六个面中心
        Vector3 fxP = c + new Vector3(+offset, 0, 0);
        Vector3 fxN = c + new Vector3(-offset, 0, 0);
        Vector3 fyP = c + new Vector3(0, +offset, 0);
        Vector3 fyN = c + new Vector3(0, -offset, 0);
        Vector3 fzP = c + new Vector3(0, 0, +offset);
        Vector3 fzN = c + new Vector3(0, 0, -offset);

        if (mode == PatternMode.Standard7)
        {
            result.Add(new CubeCell(center, childSize));
            result.Add(new CubeCell(fxP, childSize));
            result.Add(new CubeCell(fxN, childSize));
            result.Add(new CubeCell(fyP, childSize));
            result.Add(new CubeCell(fyN, childSize));
            result.Add(new CubeCell(fzP, childSize));
            result.Add(new CubeCell(fzN, childSize));
            return result;
        }

        // UnionFaceRules：体中心 + 六面中心 + 八角
        result.Add(new CubeCell(center, childSize));
        result.Add(new CubeCell(fxP, childSize));
        result.Add(new CubeCell(fxN, childSize));
        result.Add(new CubeCell(fyP, childSize));
        result.Add(new CubeCell(fyN, childSize));
        result.Add(new CubeCell(fzP, childSize));
        result.Add(new CubeCell(fzN, childSize));

        // 八个角 (±1, ±1, ±1)
        float cornerOffset = offset;
        for (int dx = -1; dx <= 1; dx += 2)
        {
            for (int dy = -1; dy <= 1; dy += 2)
            {
                for (int dz = -1; dz <= 1; dz += 2)
                {
                    Vector3 corner = c + new Vector3(dx * cornerOffset, dy * cornerOffset, dz * cornerOffset);
                    result.Add(new CubeCell(corner, childSize));
                }
            }
        }

        return result;
    }
}