using System.Collections.Generic;
using UnityEngine;

public class KochGenerator
{
    // 进行多次迭代
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

            Vector3 v = B - A;
            Vector3 P1 = A + v / 3f;
            Vector3 P2 = A + v * 2f / 3f;

            // 旋转 60° 形成顶点 P3
            
            //float angle = Mathf.PI / 12f;
            float angle = -Mathf.PI / 3f;
            Vector3 dir = P2 - P1;
            Vector3 P3 = new Vector3(
                P1.x + dir.x * Mathf.Cos(angle) - dir.y * Mathf.Sin(angle),
                P1.y + dir.x * Mathf.Sin(angle) + dir.y * Mathf.Cos(angle),
                P1.z // 保持 z 分量不变（2D 旋转）
            );

            // 依次加入新点
            newPoints.Add(A);
            newPoints.Add(P1);
            newPoints.Add(P3);
            newPoints.Add(P2);
        }

        // 闭合形状
        newPoints.Add(oldPoints[oldPoints.Count - 1]);
        return newPoints;
    }
}

