using System.Collections.Generic;
using UnityEngine;

public class GosperGenerator
{
    // 迭代多次
    public static List<Vector3> Generate(List<Vector3> points, int iterations)
    {
        for (int i = 0; i < iterations; i++)
        {
            points = Iterate(points);
        }
        return points;
    }

    // 单次迭代
    private static List<Vector3> Iterate(List<Vector3> oldPoints)
    {
        List<Vector3> newPoints = new List<Vector3>();

        for (int i = 0; i < oldPoints.Count - 1; i++)
        {
            Vector3 A = oldPoints[i];
            Vector3 B = oldPoints[i + 1];
            Vector3 P1;
            Vector3 P2;

            Vector3 v = -B + A;
            Vector3 dir = v.normalized;

            float angle = -Mathf.PI / 9.42f;
            float fixedLength = v.magnitude * 0.378f;

            Vector3 rotatedDir = Rotate(dir, angle);
            P1 = B + rotatedDir * fixedLength;
            P2 = A - rotatedDir * fixedLength;

            newPoints.Add(A);
            newPoints.Add(P2);
            newPoints.Add(P1);
        }

        newPoints.Add(oldPoints[oldPoints.Count - 1]);
        return newPoints;
    }

    // 旋转（保持 z 分量）
    private static Vector3 Rotate(Vector3 v, float radians)
    {
        float c = Mathf.Cos(radians);
        float s = Mathf.Sin(radians);
        return new Vector3(v.x * c - v.y * s, v.x * s + v.y * c, v.z);
    }
}
