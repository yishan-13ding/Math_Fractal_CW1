using UnityEngine;

public class FractalConfig
{
    public string type;       // "koch" 表示科赫雪花
    public string initialShape; // "hexagon" 或 "triangle"
    public int iteration;
    public float size;
    public float centerX, centerY, centerZ;
    public float offsetAngle;
    public string drawMode;
}
