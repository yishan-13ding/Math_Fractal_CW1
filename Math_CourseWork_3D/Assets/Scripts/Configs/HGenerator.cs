using System.Collections.Generic;
using UnityEngine;

public static class HGenerator
{
    /// <param name="points">
    /// 输入线段点集：
    /// 1) 若只有 2 个点，视为单条线段。
    /// 2) 若数量为偶数，则按 (0,1),(2,3)... 配对作为初始线段集合。
    /// </param>
    /// <param name="iterations">迭代次数 (>=0)。为 0 时仅返回原始线段。</param>
    public static List<Vector3> Generate(List<Vector3> points, int iterations)
    {
        if (points == null || points.Count < 2) return new List<Vector3>();
        if (points.Count % 2 != 0)
        {
            Debug.LogWarning("HGenerator: 输入点数量不是偶数，忽略最后一个点。");
            points.RemoveAt(points.Count - 1);
        }

        // 当前代线段
        var currentSegments = new List<(Vector3 A, Vector3 B)>();
        for (int i = 0; i < points.Count; i += 2)
            currentSegments.Add((points[i], points[i + 1]));

        // 所有代的线段（最终输出用)
        var allSegments = new List<(Vector3 A, Vector3 B)>(currentSegments);

        for (int iter = 0; iter < iterations; iter++)
        {
            var nextGen = new List<(Vector3 A, Vector3 B)>();

            foreach (var seg in currentSegments)
            {
                Vector3 A = seg.A;
                Vector3 B = seg.B;
                Vector3 v = B - A;
                float baseLen = v.magnitude;
                if (baseLen <= 1e-6f) continue;

                // 计算垂直方向（XY 平面），这里使用 (-vy, vx)
                Vector3 perp = new Vector3(-v.y, v.x, 0f).normalized;
                float newLen = baseLen * 0.5f;         // 新线段总长度 = 原线段长度的一半
                float half = newLen * 0.5f;            // 半长度用于两侧扩展

                // 端点 A 处新垂直线段（居中于 A)
                Vector3 a1 = A - perp * half;
                Vector3 a2 = A + perp * half;
                nextGen.Add((a1, a2));

                // 端点 B 处新垂直线段
                Vector3 b1 = B - perp * half;
                Vector3 b2 = B + perp * half;
                nextGen.Add((b1, b2));
            }

            // 将新一代加入总集合
            allSegments.AddRange(nextGen);
            // 下一轮仅迭代这一代生成的线段
            currentSegments = nextGen;
        }

        // 输出扁平点列表（按成对顺序）
        var flat = new List<Vector3>(allSegments.Count * 2);
        foreach (var s in allSegments)
        {
            flat.Add(s.A);
            flat.Add(s.B);
        }
        return flat;
    }
}
