using System.Collections.Generic;
using UnityEngine;

public class SquareCell : IFractalCell
{
    public Vector3 Center { get; }
    public float Size { get; } // 边长

    public SquareCell(Vector3 center, float size)
    {
        Center = center;
        Size = size;
    }

    public IReadOnlyList<Vector3> GetVertices()
    {
        float h = Size * 0.5f;
        return new[]
        {
            Center + new Vector3(-h,-h,0),
            Center + new Vector3(-h, h,0),
            Center + new Vector3( h, h,0),
            Center + new Vector3( h,-h,0)
        };
    }

    public Mesh BuildMesh()
    {
        var vertsReadOnly = GetVertices();

        // 拷贝到 List<Vector3> 以满足 Mesh.SetVertices 的签名
        var verts = new List<Vector3>(vertsReadOnly.Count);
        for (int i = 0; i < vertsReadOnly.Count; i++)
            verts.Add(vertsReadOnly[i]);

        Mesh m = new Mesh { name = "VicsekSquareCell" };
        m.SetVertices(verts);
        m.SetTriangles(new []{0,2,1,0,3,2}, 0);
        m.RecalculateNormals();
        m.RecalculateBounds();
        return m;
    }
}