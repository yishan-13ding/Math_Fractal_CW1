using System.Collections.Generic;
using UnityEngine;

public static class Sierpinski3DGenerator
{
    // 小型内部结构，仅在生成过程中使用，不暴露到外部
    private struct Tetra
    {
        public Vector3 a, b, c, d;
        public Tetra(Vector3 a, Vector3 b, Vector3 c, Vector3 d) { this.a = a; this.b = b; this.c = c; this.d = d; }
    }

    // 对外接口：直接返回每个子四面体的 Mesh
    public static List<Mesh> GenerateMeshes(Vector3 center, float size, int iterations, bool keepAllGenerations = false)
    {
        var root = BuildInitialTetra(center, size);
        var current = new List<Tetra> { root };
        var all = new List<Tetra>(current);

        for (int i = 0; i < iterations; i++)
        {
            current = IterateOnce(current);
            if (keepAllGenerations) all.AddRange(current);
        }

        var src = keepAllGenerations ? all : current;
        var result = new List<Mesh>(src.Count);
        foreach (var t in src)
            result.Add(BuildTetraMesh(t.a, t.b, t.c, t.d));
        return result;
    }

    // 规则：每条边取中点，保留四个“顶点子四面体”
    private static List<Tetra> IterateOnce(List<Tetra> parents)
    {
        var next = new List<Tetra>(parents.Count * 4);
        foreach (var p in parents)
        {
            Vector3 m01 = (p.a + p.b) * 0.5f;
            Vector3 m02 = (p.a + p.c) * 0.5f;
            Vector3 m03 = (p.a + p.d) * 0.5f;
            Vector3 m12 = (p.b + p.c) * 0.5f;
            Vector3 m13 = (p.b + p.d) * 0.5f;
            Vector3 m23 = (p.c + p.d) * 0.5f;

            next.Add(new Tetra(p.a, m01, m02, m03)); // 顶点 a
            next.Add(new Tetra(p.b, m01, m12, m13)); // 顶点 b
            next.Add(new Tetra(p.c, m02, m12, m23)); // 顶点 c
            next.Add(new Tetra(p.d, m03, m13, m23)); // 顶点 d
        }
        return next;
    }

    // 构造一个重心在 center 的“正四面体”（近似），按 size 缩放
    private static Tetra BuildInitialTetra(Vector3 center, float size)
    {
        float half = size * 0.5f;
        Vector3 a = center + (new Vector3(+1, +1, +1).normalized * half);
        Vector3 b = center + (new Vector3(+1, -1, -1).normalized * half);
        Vector3 c = center + (new Vector3(-1, +1, -1).normalized * half);
        Vector3 d = center + (new Vector3(-1, -1, +1).normalized * half);
        return new Tetra(a, b, c, d);
    }

    private static Mesh BuildTetraMesh(Vector3 a, Vector3 b, Vector3 c, Vector3 d)
    {
        var verts = new List<Vector3> { a, b, c, d };
        // 四面体的四个三角面（顶点顺序保证法线外朝）
        int[] tris = { 0,1,2, 0,3,1, 0,2,3, 1,3,2 };

        var m = new Mesh { name = "SierpinskiTetra" };
        m.SetVertices(verts);
        m.SetTriangles(tris, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}