using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.Windows;
//创建初始图案
public class ShapeGenerator
{
    // 根据名称生成初始形状
    public static List<Vector3> GenerateInitialShape(string shapeName, float size, float offsetAngleDegree)
    {
        switch (shapeName.ToLower())
        {
            case "triangle":
                return CreatePolygon(3, size, offsetAngleDegree);
            case "hexagon":
                return CreatePolygon(6, size, offsetAngleDegree);
            case "vicsek":
                return CreateSquare(size, offsetAngleDegree);
            case "sierpinski":
                return Createtriangle(size, offsetAngleDegree);
            case "hilbert":
                return CreateHilbertBase(4, size, offsetAngleDegree);
            case "circle":
                return CreateCircle(4, size, offsetAngleDegree);
            case "line":
                return CreateLine(size, offsetAngleDegree);
            default:
                Debug.LogWarning("Unknown shape, default to triangle");
                return CreatePolygon(3, size, offsetAngleDegree);
        }
    }

    //创建正多边形
    private static List<Vector3> CreatePolygon(int sides, float radius, float offsetAngleDegree)
    {
        List<Vector3> points = new List<Vector3>();
        float angleStep = 2 * Mathf.PI / sides; // 每个角之间的弧度间隔

        for (int i = 0; i < sides; i++)
        {
            //float mapped = Map(sides, 3f, 6f, Mathf.PI / 6f, 0f);
            float angle = i * angleStep - Mathf.PI / offsetAngleDegree;
            //float angle = i * angleStep - Mathf.PI / 6f;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            points.Add(new Vector3(x, y, 0));
        }

        // 最后闭合图形（连接回起点）
        points.Add(points[0]);
        return points;
    }
    private static List<Vector3> CreateHilbertBase(int sides, float size, float offsetAngleDegreefloat)
    {
        List<Vector3> points = new List<Vector3>();
        float half = size / 2f;

        // 定义四个点，构成一个 "U" 形
        Vector3 p0 = new Vector3(-half,  half, 0);
        Vector3 p1 = new Vector3(-half, -half, 0);
        Vector3 p2 = new Vector3(half,  -half, 0);
        Vector3 p3 = new Vector3(half,  half, 0);

        points.Add(p0);
        points.Add(p1);
        points.Add(p2);
        points.Add(p3);
        return points;
    }

    private static List<Vector3> CreateCircle(int sides, float size, float offsetAngleDegreefloat)
    {

        int segments = 64;
        float radius = 0.5f;// Mathf.Abs(1);
        float angleOffset = offsetAngleDegreefloat * Mathf.Deg2Rad;
        float angleStep = 2f * Mathf.PI / segments;

        List<Vector3> points = new List<Vector3>(segments + 1);
        for (int i = 0; i < segments; i++)
        {
            float angle = angleOffset + i * angleStep;
            float x = Mathf.Cos(angle) * radius;
            float y = Mathf.Sin(angle) * radius;
            points.Add(new Vector3(x, y, 0f));
        }

        // 闭合到起点，便于作为单条折线渲染完整圆周
        points.Add(points[0]);
        return points;
    }

    // 基于 Mesh 生成正方形网格（中心在原点，Z=0）
    // 说明：为与其它形状保持一致，size 视为“到顶点的半径”，因此半边长 = size / sqrt(2)
    public static Mesh CreateSquareMesh(float size, float offsetAngleDegree)
    {
        float half = Mathf.Abs(size) / Mathf.Sqrt(2f); // 若想把 size 当作边长，改为 size * 0.5f

        // 顶点按绕序定义（未旋转前：左下、左上、右上、右下）
        Vector3[] v =
        {
            new Vector3(-half, -half, 0f),
            new Vector3(-half,  half, 0f),
            new Vector3( half,  half, 0f),
            new Vector3( half, -half, 0f),
        };

        // 绕 Z 轴旋转 offsetAngleDegree
        float rad = offsetAngleDegree * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad);
        float s = Mathf.Sin(rad);
        for (int i = 0; i < v.Length; i++)
        {
            var p = v[i];
            v[i] = new Vector3(p.x * c - p.y * s, p.x * s + p.y * c, p.z);
        }

        // UV（简单平铺）
        Vector2[] uv =
        {
            new Vector2(0,0),
            new Vector2(0,1),
            new Vector2(1,1),
            new Vector2(1,0),
        };
        // 三角形索引（顺时针，面向 +Z）
        int[] tris = { 0, 2, 1, 0, 3, 2 };

        Mesh mesh = new Mesh
        {
            name = "SquareMesh"
        };
        mesh.SetVertices(v);
        mesh.SetUVs(0, uv);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Debug.LogWarning("成功运行mesh了");
        return mesh;
    }

    // 供现有管线使用：从 Mesh 顶点提取正方形轮廓（闭合）
    private static List<Vector3> CreateSquare(float size, float offsetAngleDegree)
    {
        Mesh m = CreateSquareMesh(size, offsetAngleDegree);
        var v = m.vertices; // 顺序：左下、左上、右上、右下（已旋转）
        return new List<Vector3> { v[0], v[1], v[2], v[3], v[0] }; // 闭合
    }

    //生成等边三角形 Mesh（中心在原点，size 视为外接圆半径，绕 Z 旋转 offsetAngleDegree）
    public static Mesh CreateTriangleMesh(float size, float offsetAngleDegree)
    {
        float R = Mathf.Abs(size);
        float baseAngle = offsetAngleDegree * Mathf.Deg2Rad;

        // 等边三角形三个顶点（以外接圆半径 R 放置在 90°, 210°, 330° 方向）
        float a0 = Mathf.PI / 2f + baseAngle;
        float a1 = 210f * Mathf.Deg2Rad + baseAngle;
        float a2 = 330f * Mathf.Deg2Rad + baseAngle;

        Vector3[] verts =
        {
            new Vector3(Mathf.Cos(a0) * R, Mathf.Sin(a0) * R, 0f),
            new Vector3(Mathf.Cos(a1) * R, Mathf.Sin(a1) * R, 0f),
            new Vector3(Mathf.Cos(a2) * R, Mathf.Sin(a2) * R, 0f),
        };

        // 简单 UV（顶点映射到[0,1]范围内的典型布局）
        Vector2[] uvs =
        {
            new Vector2(0.5f, 1f),
            new Vector2(0f,   0f),
            new Vector2(1f,   0f),
        };

        int[] tris = { 0, 1, 2 }; // 顺时针朝 +Z

        Mesh mesh = new Mesh { name = "EquilateralTriangleMesh" };
        mesh.SetVertices(verts);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        Debug.LogWarning("成功运行mesh了");
        return mesh;
    }

    // 返回三角形的轮廓点（闭合），供现有 LineRenderer 管线使用
    private static List<Vector3> Createtriangle(float size, float offsetAngleDegree)
    {
        Mesh m = CreateTriangleMesh(size, offsetAngleDegree);
        var v = m.vertices; // 0,1,2
        return new List<Vector3> { v[0], v[1], v[2], v[0] }; // 闭合轮廓
    }

    // 生成一条线段：长度=size，中心在原点；先平行X轴，再按offsetAngleDegree绕Z旋转
    private static List<Vector3> CreateLine(float size, float offsetAngleDegree)
    {
        float half = Mathf.Abs(size) * 0.5f;

        // 初始端点（平行X轴）
        Vector3 a = new Vector3(-half, 0f, 0f);
        Vector3 b = new Vector3(half, 0f, 0f);

        // 绕Z轴旋转
        float rad = offsetAngleDegree * Mathf.Deg2Rad;
        float c = Mathf.Cos(rad);
        float s = Mathf.Sin(rad);

        Vector3 ra = new Vector3(a.x * c - a.y * s, a.x * s + a.y * c, 0f);
        Vector3 rb = new Vector3(b.x * c - b.y * s, b.x * s + b.y * c, 0f);

        // 线段不闭合，返回两个端点即可
        return new List<Vector3> { ra, rb };
    }
}
