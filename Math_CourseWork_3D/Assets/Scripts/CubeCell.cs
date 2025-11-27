using UnityEngine;
using System.Collections.Generic;
public class CubeCell
{
    public Vector3 Center { get; }
    public float Size { get; }

    public CubeCell(Vector3 center, float size)
    {
        Center = center;
        Size = size;
    }

    // 返回 List<Vector3>，方便直接传给 Mesh.SetVertices(List<T>)
    public List<Vector3> GetVertices()
    {
        float h = Size * 0.5f;
        return new List<Vector3>(8)
        {
            Center + new Vector3(-h,-h,-h),
            Center + new Vector3(-h, h,-h),
            Center + new Vector3( h, h,-h),
            Center + new Vector3( h,-h,-h),
            Center + new Vector3(-h,-h, h),
            Center + new Vector3(-h, h, h),
            Center + new Vector3( h, h, h),
            Center + new Vector3( h,-h, h),
        };
    }

    public Mesh BuildMesh()
    {
        var verts = GetVertices();
        // 立方体 12 三角形
        int[] tris =
        {
            // 后面(-z)
            0,1,2, 0,2,3,
            // 前面(+z)
            4,6,5, 4,7,6,
            // 左(-x)
            0,4,5, 0,5,1,
            // 右(+x)
            3,2,6, 3,6,7,
            // 下(-y)
            0,3,7, 0,7,4,
            // 上(+y)
            1,5,6, 1,6,2
        };

        // 构建 Mesh
        var mesh = new Mesh { name = "CubeCell" };
        mesh.SetVertices(verts);
        mesh.SetTriangles(tris, 0);

        // 简单 UV：投影到 xy
        var uvList = new List<Vector2>(verts.Count);
        for (int i = 0; i < verts.Count; i++)
            uvList.Add(new Vector2(verts[i].x, verts[i].y));
        mesh.SetUVs(0, uvList);

        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }
}