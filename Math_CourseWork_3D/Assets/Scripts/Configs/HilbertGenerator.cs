using System.Collections.Generic;
using UnityEngine;

public static class HilbertGenerator
{
    // iterations: 外部传入的“迭代次数” (0 显示阶数1)
    public static List<Vector3> Generate(List<Vector3> sourcePoints, int iterations)
    {
        // 将外部迭代次数映射到真实 Hilbert 阶数
        //int order = Mathf.Max(1, iterations + 1); // 迭代0 -> 阶数1
        int order = iterations + 1;
        // 计算包围盒（若传入为空则用默认 0..1）
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        if (sourcePoints != null && sourcePoints.Count >= 2)
        {
            foreach (var p in sourcePoints)
            {
                if (p.x < min.x) min.x = p.x;
                if (p.y < min.y) min.y = p.y;
                if (p.x > max.x) max.x = p.x;
                if (p.y > max.y) max.y = p.y;
            }
        }
        else
        {
            min = Vector2.zero;
            max = Vector2.one;
        }

        float width = max.x - min.x;
        float height = max.y - min.y;
        if (width <= 0f) width = 1f;
        if (height <= 0f) height = 1f;

        string sequence = ExpandLSystem(order);
        int side = 1 << order;               // 网格边长 = 2^order
        float dx = width / (side - 1);
        float dy = height / (side - 1);

        List<Vector3> result = new List<Vector3>(side * side);
        Vector2Int pos = new Vector2Int(0, 0);
        Vector2Int dir = new Vector2Int(1, 0); // 初始朝向右

        // 起点
        result.Add(new Vector3(min.x, min.y, 0f));

        foreach (char c in sequence)
        {
            switch (c)
            {
                case 'F':
                    pos += dir;
                    result.Add(new Vector3(min.x + pos.x * dx, min.y + pos.y * dy, 0f));
                    break;
                case '+': // 左转90°
                    dir = new Vector2Int(-dir.y, dir.x);
                    break;
                case '-': // 右转90°
                    dir = new Vector2Int(dir.y, -dir.x);
                    break;
                // L/R 跳过
            }
        }

        // 在包围盒中心旋转 180°（等价于中心对称）
        Vector2 center = new Vector2((min.x + max.x) * 0.5f, (min.y + max.y) * 0.5f);
        for (int i = 0; i < result.Count; i++)
        {
            var p = result[i];
            result[i] = new Vector3(
                2f * center.x - p.x,
                2f * center.y - p.y,
                p.z
            );
        }

        return result;
    }

    // L-System: Axiom L; L -> +RF-LFL-FR+ ; R -> -LF+RFR+FL-
    private static string ExpandLSystem(int order)
    {
        string current = "L";
        for (int i = 0; i < order; i++)
        {
            System.Text.StringBuilder sb = new System.Text.StringBuilder(current.Length * 7);
            foreach (char c in current)
            {
                switch (c)
                {
                    case 'L':
                        sb.Append("+RF-LFL-FR+");
                        break;
                    case 'R':
                        sb.Append("-LF+RFR+FL-");
                        break;
                    default:
                        sb.Append(c);
                        break;
                }
            }
            current = sb.ToString();
        }
        return current;
    }
}
