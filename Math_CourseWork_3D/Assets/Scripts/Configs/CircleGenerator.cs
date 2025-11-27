using System.Collections.Generic;
using UnityEngine;

public static class CircleGenerator
{
    // 原有：扁平化所有圆（仍保留，避免其它旧调用出问题）
    public static List<Vector3> Generate(List<Vector3> circleTemplate, int iterations)
    {
        //if (circleTemplate == null || circleTemplate.Count < 2)
            //circleTemplate = BuildFallbackTemplate(64);

        float templateRadius = ComputeTemplateRadius(circleTemplate);
        if (templateRadius <= 0f) templateRadius = 1f;

        List<Vector3> output = new List<Vector3>();
        Vector3 rootCenter = Vector3.zero;
        float rootRadius = 3f;

        // 旧递归：仍会产生“连接线”用在需要单条折线的场景
        RecursiveFlat(circleTemplate, templateRadius, output, rootCenter, rootRadius, iterations);
        return output;
    }

    // 新增：分离圆，返回每个圆自己的点列表（闭合）
    public static List<List<Vector3>> GenerateLoops(List<Vector3> circleTemplate, int iterations, float rootRadius)
    {
        //if (circleTemplate == null || circleTemplate.Count < 2)
            //circleTemplate = BuildFallbackTemplate(64);

        float templateRadius = ComputeTemplateRadius(circleTemplate);
        if (templateRadius <= 0f) templateRadius = 1f;

        var loops = new List<List<Vector3>>();
        Vector3 rootCenter = Vector3.zero;

        RecursiveSeparated(circleTemplate, templateRadius, loops, rootCenter, rootRadius, iterations);
        return loops;
    }

    // 扁平版本递归（旧）
    private static void RecursiveFlat(List<Vector3> template, float templateRadius, List<Vector3> output,
        Vector3 center, float radius, int depth)
    {
        AddOneCircleFlat(template, templateRadius, output, center, radius);
        if (depth <= 0) return;

        float childR = radius / 3f;
        float offset = radius; // 圆心在父圆圆周上

        RecursiveFlat(template, templateRadius, output, center + new Vector3(0, offset, 0), childR, depth - 1);
        RecursiveFlat(template, templateRadius, output, center + new Vector3(offset, 0, 0), childR, depth - 1);
        RecursiveFlat(template, templateRadius, output, center + new Vector3(0, -offset, 0), childR, depth - 1);
        RecursiveFlat(template, templateRadius, output, center + new Vector3(-offset, 0, 0), childR, depth - 1);
    }

    // 分离版本递归（新）
    private static void RecursiveSeparated(List<Vector3> template, float templateRadius, List<List<Vector3>> loops,
        Vector3 center, float radius, int depth)
    {
        loops.Add(BuildOneCircle(template, templateRadius, center, radius));
        if (depth <= 0) return;

        float childR = radius / 3f;
        float offset = radius;

        RecursiveSeparated(template, templateRadius, loops, center + new Vector3(0, offset, 0), childR, depth - 1);
        RecursiveSeparated(template, templateRadius, loops, center + new Vector3(offset, 0, 0), childR, depth - 1);
        RecursiveSeparated(template, templateRadius, loops, center + new Vector3(0, -offset, 0), childR, depth - 1);
        RecursiveSeparated(template, templateRadius, loops, center + new Vector3(-offset, 0, 0), childR, depth - 1);
    }

    // 旧：直接追加到扁平数组（会形成连接线）
    private static void AddOneCircleFlat(List<Vector3> template, float templateRadius, List<Vector3> output,
        Vector3 center, float radius)
    {
        float scale = radius / templateRadius;
        int start = output.Count;
        for (int i = 0; i < template.Count; i++)
            output.Add(template[i] * scale + center);

        if (!ApproximatelyEqual(output[start], output[output.Count - 1]))
            output.Add(output[start]); // 闭合

        // 分隔点（仍无法真正断开渲染折线，只是重复，保留兼容）
        output.Add(output[output.Count - 1]);
    }

    // 新：构建单个圆的点列表（闭合）
    private static List<Vector3> BuildOneCircle(List<Vector3> template, float templateRadius,
        Vector3 center, float radius)
    {
        float scale = radius / templateRadius;
        var circle = new List<Vector3>(template.Count + 1);
        for (int i = 0; i < template.Count; i++)
            circle.Add(template[i] * scale + center);

        if (!ApproximatelyEqual(circle[0], circle[circle.Count - 1]))
            circle.Add(circle[0]); // 闭合
        return circle;
    }

    private static float ComputeTemplateRadius(List<Vector3> template)
    {
        float r = 0f;
        foreach (var p in template)
        {
            float d = Mathf.Sqrt(p.x * p.x + p.y * p.y);
            if (d > r) r = d;
        }
        return r;
    }

    private static bool ApproximatelyEqual(Vector3 a, Vector3 b)
    {
        const float eps = 1e-5f;
        return Mathf.Abs(a.x - b.x) < eps &&
               Mathf.Abs(a.y - b.y) < eps &&
               Mathf.Abs(a.z - b.z) < eps;
    }

    //private static List<Vector3> BuildFallbackTemplate(int segments)
    //{
    //    List<Vector3> pts = new List<Vector3>(segments + 1);
    //    float step = 2f * Mathf.PI / segments;
    //    for (int i = 0; i < segments; i++)
    //    {
    //        float ang = i * step;
    //        pts.Add(new Vector3(Mathf.Cos(ang), Mathf.Sin(ang), 0f));
    //    }
    //    pts.Add(pts[0]); // 闭合
    //    return pts;
    //}
}

