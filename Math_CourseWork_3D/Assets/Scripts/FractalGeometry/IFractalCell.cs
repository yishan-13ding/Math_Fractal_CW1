using System.Collections.Generic;
using UnityEngine;

public interface IFractalCell
{
    Vector3 Center { get; }
    float Size { get; }
    IReadOnlyList<Vector3> GetVertices(); // 顶点顺序（不含闭合重复点）
    Mesh BuildMesh();
}