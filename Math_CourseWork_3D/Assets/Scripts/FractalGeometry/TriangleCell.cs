using System.Collections.Generic;
using UnityEngine;

public class TriangleCell : IFractalCell
{
    public Vector3 Center { get; }
    public float Size { get; } // 边长（假设等边）
    private readonly Vector3[] _verts;

    // 通过三个顶点直接构造
    public TriangleCell(Vector3 v0, Vector3 v1, Vector3 v2)
    {
        _verts = new[] { v0, v1, v2 };
        Center = (v0 + v1 + v2) / 3f;
        Size = (v0 - v1).magnitude;
    }

    public IReadOnlyList<Vector3> GetVertices() => _verts;

    public Mesh BuildMesh()
    {
        var list = new List<Vector3>(_verts);
        Mesh m = new Mesh { name = "TriangleCellMesh" };
        m.SetVertices(list);
        m.SetTriangles(new[] { 0, 1, 2 }, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}
