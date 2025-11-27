using System.Collections.Generic;
using System.IO;
using UnityEngine;



public class FractalManager : MonoBehaviour
{
    public enum ConfigFilePreset
    {
        KochTriangle,
        KochHexagon,
        SierpinskiTriangle,
        Vicsek,
        HilbertCurve,
        Circle,
        H,
        TSquare,
    }

    [Header("配置文件预设")]
    public ConfigFilePreset configPreset;

    [Range(0, 4)]
    public int iteration = 0; // 当前迭代层数
    public int maxIteration = 4; // 最大迭代次数

    [Header("运行时可调尺寸")]
    [Range(1f, 6.0f)]
    public float size = 3f;

    [Header("Vicsek 显示模式")]
    [SerializeField] public bool vicsek3DEnabled = false;



    private FractalConfig config;
    private List<Vector3> points;
    [SerializeField] private LineRenderer linePrefab;
    [SerializeField] private float lineWidth = 0.05f;
    [SerializeField] private Material meshMaterial; // 新增：mesh 模式的材质
    private Material _runtimeSharedMat;
    public string CurrentType => config?.type;

    private int lastIteration = -1;
    private ConfigFilePreset lastPreset;
    private List<Vector3> baseShape;
    private Coroutine _spinRoutine;

    // 已实例化的绘制对象
    private readonly List<LineRenderer> activeLines = new List<LineRenderer>();
    private readonly List<GameObject> activeMeshes = new List<GameObject>();
    private readonly List<GameObject> activeCellMeshes = new List<GameObject>();

    void Start()
    {
        iteration = 0;
        InitializeFractal(force: true);

        //size = 3;
        configPreset = ConfigFilePreset.KochTriangle;

    }

    void Update()
    {
        if (iteration != lastIteration)
        {
            UpdateFractal();
            lastIteration = iteration;
        }
        if (configPreset != lastPreset)
        {
            InitializeFractal(force: true);
        }
    }
    private void InitializeFractal(bool force)
    {
        lastPreset = configPreset;
        lastIteration = iteration;

        ClearFractal();

        string path = ResolveConfigPath(configPreset);
        config = ConfigLoader.Load(path);
        if (config == null)
        {
            Debug.LogError("加载配置失败: " + path);
            return;
        }

        _runtimeSharedMat = meshMaterial != null ? meshMaterial : new Material(Shader.Find("Standard"));

        config.size = size;
        baseShape = ShapeGenerator.GenerateInitialShape(config.initialShape, size, config.offsetAngle);
        if (baseShape == null || baseShape.Count == 0)
        {
            Debug.LogError("生成初始形状失败");
            return;
        }

        points = GenerateByType(new List<Vector3>(baseShape), iteration, config.type);
        DrawByMode(points, config.drawMode);
        Debug.Log($"Fractal 初始化完成: preset={configPreset}, type={config.type}, iteration={iteration}");
    }

    void UpdateFractal()
    {
        if (baseShape == null)
        {
            Debug.LogError("baseShape 未初始化，无法更新分型");
            return;
        }

        List<Vector3> currentPoints = GenerateByType(new List<Vector3>(baseShape), iteration, config?.type);
        DrawByMode(currentPoints, config?.drawMode);
        Debug.Log($"当前迭代次数: {iteration}, type: {config?.type}");
    }

    private List<Vector3> GenerateByType(List<Vector3> inputShape, int iterations, string type)
    {
        if (string.IsNullOrWhiteSpace(type))
        {
            Debug.LogWarning("未指定生成器 type，使用 Koch");
            return KochGenerator.Generate(inputShape, iterations);
        }

        string mode = config?.drawMode?.ToLowerInvariant();
        switch (type.ToLowerInvariant())
        {
            case "vicsek":
                if (mode == "mesh") return new List<Vector3>();
                return VicsekGenerator.Generate(inputShape, iterations);
            case "sierpinski": return SierpinskiGenerator.Generate(inputShape, iterations);
            case "gosper": return GosperGenerator.Generate(inputShape, iterations);
            case "koch": return KochGenerator.Generate(inputShape, iterations);
            case "hilbert": return HilbertGenerator.Generate(inputShape, iterations);
            case "circle": return CircleGenerator.Generate(inputShape, iterations);
            case "h": return HGenerator.Generate(inputShape, iterations);
            case "t":
                if (mode == "mesh") return new List<Vector3>();
                return TSquareGenerator.Generate(inputShape, iterations);

            default:
                Debug.LogWarning($"未知生成器 {type}，回退 Koch");
                return KochGenerator.Generate(inputShape, iterations);
        }
    }

    private void DrawByMode(List<Vector3> pts, string drawMode)
    {
        string mode = string.IsNullOrWhiteSpace(drawMode) ? "line" : drawMode.ToLowerInvariant();
        ClearFractal();
        string type = config.type?.ToLowerInvariant();

        // H 分形：每条线段独立（不连接）
        if (config.type != null && config.type.ToLowerInvariant() == "h")
        {
            DrawSegments(pts);
            return;
        }

        // Circle 特殊：分离圆的线模式（不连接）
        if (config.type != null && config.type.ToLowerInvariant() == "circle" && mode == "line")
        {
            var loops = CircleGenerator.GenerateLoops(baseShape, iteration, size);
            DrawSeparateLoops(loops);
            return;
        }

        if (type == "vicsek" && mode == "mesh")
        {
            var cells = VicsekGenerator.GenerateCells(baseShape, iteration);
            DrawSquareCells(cells);
            return;
        }

        // Vicsek：根据 toggle 决定 2D(mesh) 或 3D
        if (type == "vicsek")
        {
            if (vicsek3DEnabled)
            {
                var cells3d = Vicsek3DGenerator.GenerateCells(Vector3.zero, size, iteration,
                    Vicsek3DGenerator.PatternMode.UnionFaceRules, keepAllGenerations: false);
                DrawCubeCells(cells3d);
                return;
            }
            else
            {
                var cells = VicsekGenerator.GenerateCells(baseShape, iteration);
                DrawSquareCells(cells);
                return;
            }
        }

        // TSquare：toggle 控制 2D(mesh)/3D
        if (type == "tsquare" || type == "t")
        {
            if (vicsek3DEnabled)
            {
                var cells3d = TSquare3DGenerator.GenerateCells(Vector3.zero, size, iteration);
                DrawCubeCells(cells3d);
                return;
            }
            else
            {
                var cells = TSquareGenerator.GenerateCells(baseShape, iteration);
                DrawSquareCells(cells);
                return;
            }
        }

        // Sierpinski：toggle 控制 2D(mesh)/3D
        if (type == "sierpinski")
        {
            if (vicsek3DEnabled)
            {
                var meshes = Sierpinski3DGenerator.GenerateMeshes(Vector3.zero, size, iteration, keepAllGenerations: false);
                DrawMeshes(meshes); // 新增的统一 Mesh 绘制方法
                return;
            }
            else
            {
                var cells = SierpinskiGenerator.GenerateCells(baseShape, iteration);
                DrawTriangleCells(cells);
                return;
            }
        }

        if (mode == "mesh")
        {
            if (!TryDrawMesh(pts))
            {
                Debug.LogWarning("Mesh 模式下点集不闭合或无法生成 Mesh，回退 LineRenderer。");
                DrawLine(pts);
            }
        }
        else
        {
            DrawLine(pts);
        }
    }

    private void DrawLine(List<Vector3> pts)
    {
        if (pts == null || pts.Count == 0) return;
        if (linePrefab == null)
        {
            Debug.LogError("linePrefab 未在 Inspector 中设置");
            return;
        }

        LineRenderer line = Instantiate(linePrefab);
        line.transform.SetParent(transform, false);  // 关键：挂到 FractalManager 下
        line.useWorldSpace = false;                  // 关键：局部坐标，受父旋转影响

        line.positionCount = pts.Count;
        // 统一宽度（如需）
        // line.startWidth = lineWidth;
        // line.endWidth = lineWidth;

        for (int i = 0; i < pts.Count; i++)
            line.SetPosition(i, new Vector3(pts[i].x, pts[i].y, 0));
        activeLines.Add(line);
    }

    private void DrawSegments(List<Vector3> pts)
    {
        if (pts == null || pts.Count < 2) return;
        if (linePrefab == null)
        {
            Debug.LogError("linePrefab 未设置");
            return;
        }
        int usable = pts.Count - (pts.Count % 2);
        for (int i = 0; i < usable; i += 2)
        {
            var lr = Instantiate(linePrefab);
            lr.transform.SetParent(transform, false); // 子物体
            lr.useWorldSpace = false;                 // 局部坐标

            lr.positionCount = 2;
            // lr.startWidth = lineWidth;
            // lr.endWidth = lineWidth;

            lr.SetPosition(0, new Vector3(pts[i].x, pts[i].y, 0));
            lr.SetPosition(1, new Vector3(pts[i + 1].x, pts[i + 1].y, 0));
            activeLines.Add(lr);
        }
    }

    private void DrawSeparateLoops(List<List<Vector3>> loops)
    {
        if (linePrefab == null)
        {
            Debug.LogError("linePrefab 未在 Inspector 中设置");
            return;
        }
        foreach (var loop in loops)
        {
            if (loop == null || loop.Count < 2) continue;
            var lr = Instantiate(linePrefab);
            lr.transform.SetParent(transform, false); 
            lr.useWorldSpace = false;                 

            lr.positionCount = loop.Count;

            for (int i = 0; i < loop.Count; i++)
                lr.SetPosition(i, new Vector3(loop[i].x, loop[i].y, 0));
            activeLines.Add(lr);
        }
    }

    // Mesh 也挂到 FractalManager 下，让整体旋转生效
    private bool TryDrawMesh(List<Vector3> pts)
    {
        if (pts == null || pts.Count < 4) return false;
        if (!IsClosed(pts)) return false;

        int n = pts.Count - 1;
        var poly = new List<Vector3>(n);
        for (int i = 0; i < n; i++) poly.Add(new Vector3(pts[i].x, pts[i].y, 0f));

        Mesh mesh = BuildTriangleFanMesh(poly);
        if (mesh == null) return false;

        var go = new GameObject("FractalMesh");
        go.transform.SetParent(transform, false); 
        var mf = go.AddComponent<MeshFilter>();
        var mr = go.AddComponent<MeshRenderer>();
        mf.sharedMesh = mesh;
        mr.sharedMaterial = _runtimeSharedMat;

        activeMeshes.Add(go);
        return true;
    }

    private void DrawTriangleCells(List<TriangleCell> cells)
    {
        foreach (var c in cells)
        {
            Mesh m = c.BuildMesh();
            var go = new GameObject("TriangleCell");
            go.transform.SetParent(transform, false); // 关键
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = m;
            mr.sharedMaterial = _runtimeSharedMat; // 复用材质
            activeCellMeshes.Add(go);
        }
    }

    private void DrawSquareCells(List<SquareCell> cells)
    {
        if (cells == null || cells.Count == 0)
        {
            Debug.LogWarning("Vicsek cells 为空");
            return;
        }

        foreach (var cell in cells)
        {
            Mesh m = cell.BuildMesh();
            var go = new GameObject("VicsekCell");
            go.transform.SetParent(transform, false); // 关键
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = m;
            mr.sharedMaterial = _runtimeSharedMat; // 复用材质
            activeCellMeshes.Add(go);
        }
    }

    // 三角扇：中心点 + 多边形外圈顺序顶点
    private Mesh BuildTriangleFanMesh(List<Vector3> poly)
    {
        if (poly == null || poly.Count < 3) return null;

        // 计算中心
        Vector3 center = Vector3.zero;
        for (int i = 0; i < poly.Count; i++) center += poly[i];
        center /= poly.Count;

        int vertexCount = poly.Count + 1;
        Vector3[] vertices = new Vector3[vertexCount];
        vertices[0] = center;
        for (int i = 0; i < poly.Count; i++) vertices[i + 1] = poly[i];

        // 生成三角形索引
        int triCount = poly.Count;
        int[] tris = new int[triCount * 3];
        int t = 0;
        for (int i = 1; i < poly.Count; i++)
        {
            tris[t++] = 0;
            tris[t++] = i;
            tris[t++] = i + 1;
        }
        // 最后一扇片闭合回第一个顶点
        tris[t++] = 0;
        tris[t++] = poly.Count;
        tris[t++] = 1;

        // 生成简单 UV（按包围盒归一化）
        Vector2 min = new Vector2(float.MaxValue, float.MaxValue);
        Vector2 max = new Vector2(float.MinValue, float.MinValue);
        for (int i = 1; i < vertexCount; i++)
        {
            var p = vertices[i];
            if (p.x < min.x) min.x = p.x;
            if (p.y < min.y) min.y = p.y;
            if (p.x > max.x) max.x = p.x;
            if (p.y > max.y) max.y = p.y;
        }
        Vector2 size = max - min;
        if (size.x <= 1e-6f) size.x = 1f;
        if (size.y <= 1e-6f) size.y = 1f;
        Vector2[] uvs = new Vector2[vertexCount];
        for (int i = 0; i < vertexCount; i++)
        {
            var p = vertices[i];
            uvs[i] = new Vector2((p.x - min.x) / size.x, (p.y - min.y) / size.y);
        }

        Mesh mesh = new Mesh { name = "FractalFilledMesh" };
        mesh.SetVertices(vertices);
        mesh.SetUVs(0, uvs);
        mesh.SetTriangles(tris, 0);
        mesh.RecalculateNormals();
        mesh.RecalculateBounds();
        return mesh;
    }

    private bool IsClosed(List<Vector3> pts)
    {
        if (pts.Count < 2) return false;
        var a = pts[0]; var b = pts[pts.Count - 1];
        return (Mathf.Abs(a.x - b.x) < 1e-5f) &&
               (Mathf.Abs(a.y - b.y) < 1e-5f);
    }

    private void ClearFractal()
    {
        for (int i = activeLines.Count - 1; i >= 0; i--)
        {
            var lr = activeLines[i];
            if (lr != null) Destroy(lr.gameObject);
        }
        activeLines.Clear();

        // 清除单元 Mesh
        for (int i = activeCellMeshes.Count - 1; i >= 0; i--)
        {
            var go = activeCellMeshes[i];
            if (go != null)
            {
                var mf = go.GetComponent<MeshFilter>();
                Destroy(go);
                Destroy(mf.sharedMesh); // 关键：销毁运行时 Mesh

            }
        }
        activeCellMeshes.Clear();

        // 清除整体 Mesh
        for (int i = activeMeshes.Count - 1; i >= 0; i--)
        {
            var go = activeMeshes[i];
            if (go != null)
            {
                var mf = go.GetComponent<MeshFilter>();
                Destroy(go);
                Destroy(mf.sharedMesh); // 关键：销毁运行时 Mesh
            }
        }
        activeMeshes.Clear();
    }
    private void DrawCubeCells(List<CubeCell> cells)
    {
        if (cells == null || cells.Count == 0)
        {
            Debug.LogWarning("Vicsek3D cells 为空");
            return;
        }

        foreach (var cell in cells)
        {
            Mesh m = cell.BuildMesh();
            var go = new GameObject("Vicsek3DCell");
            go.transform.SetParent(transform, false);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = m;
            mr.sharedMaterial = _runtimeSharedMat; // 复用材质
            activeCellMeshes.Add(go);
        }
    }

    // 根据枚举解析配置文件路径
    private string ResolveConfigPath(ConfigFilePreset preset)
    {
        // 你可以把这些文件放在 Assets/Configs/ 下
        string baseDir = Path.Combine(Application.dataPath, "Configs");
        switch (preset)
        {
            case ConfigFilePreset.KochTriangle: return Path.Combine(baseDir, "kochSnow1.txt");
            case ConfigFilePreset.KochHexagon: return Path.Combine(baseDir, "kochSnow2.txt");
            case ConfigFilePreset.SierpinskiTriangle: return Path.Combine(baseDir, "Sierpinski.txt");
            case ConfigFilePreset.Vicsek: return Path.Combine(baseDir, "vicsek.txt");
            case ConfigFilePreset.HilbertCurve: return Path.Combine(baseDir, "hilbert.txt");
            case ConfigFilePreset.Circle: return Path.Combine(baseDir, "circle.txt");
            case ConfigFilePreset.H: return Path.Combine(baseDir, "hfractal.txt");
            case ConfigFilePreset.TSquare: return Path.Combine(baseDir, "TSquare.txt");
            default: return Path.Combine(baseDir, "default.txt");
        }
    }


    //---------------------------------------------------UI------------------------------------------------------
    public void SetSize(float newSize)
    {
        //newSize = Mathf.Max(0.001f, newSize);
        //if (Mathf.Abs(newSize - size) < 1e-5f) return;

        size = newSize;
        if (config != null) config.size = size;

        // 重建
        ClearFractal();
        baseShape = ShapeGenerator.GenerateInitialShape(config.initialShape, size, config.offsetAngle);
        if (baseShape == null || baseShape.Count == 0)
        {
            Debug.LogError("SetSize: 重建 baseShape 失败");
            return;
        }
        points = GenerateByType(new List<Vector3>(baseShape), iteration, config.type);
        DrawByMode(points, config.drawMode);
    }

    public void SetRotation(float angleOffset)
    {
        if (config == null) return;
        config.offsetAngle = angleOffset;
        // 重建
        ClearFractal();
        baseShape = ShapeGenerator.GenerateInitialShape(config.initialShape, size, config.offsetAngle);
        if (baseShape == null || baseShape.Count == 0)
        {
            Debug.LogError("SetRotation: 重建 baseShape 失败");
            return;
        }
        points = GenerateByType(new List<Vector3>(baseShape), iteration, config.type);
        DrawByMode(points, config.drawMode);
    }
    public void SetPreset(ConfigFilePreset preset)
    {
        if (configPreset == preset) return;
        configPreset = preset;
        InitializeFractal(force: true);
        // 重置 MiddleMouseOrbit 的旋转角度为默认
        var orbit = GetComponent<MiddleMouseOrbit>();
        if (orbit != null) orbit.ResetRotation();
    }

    public void SetPresetByIndex(int index)
    {
        if (index < 0 || index >= System.Enum.GetNames(typeof(ConfigFilePreset)).Length)
        {
            Debug.LogWarning("SetPresetByIndex: index 超出范围");
            return;
        }
        SetPreset((ConfigFilePreset)index);
    }

    public void SpinY(float duration = 2f)
    {
        if (_spinRoutine != null) StopCoroutine(_spinRoutine);
        _spinRoutine = StartCoroutine(SpinYCoroutine(duration));
    }

    private System.Collections.IEnumerator SpinYCoroutine(float duration)
    {
        var origin = transform.localRotation;
        float t = 0f;
        while (t < duration)
        {
            t += Time.deltaTime;
            float angle = Mathf.Lerp(0f, 360f, Mathf.Clamp01(t / duration));
            transform.localRotation = origin * Quaternion.Euler(0f, angle, 0f);
            yield return null;
        }
        transform.localRotation = origin; // 360°后回到初始姿态
        _spinRoutine = null;
    }


    // 提供给 UI 的方法：切换开关并刷新
    public void SetVicsek3DEnabled(bool enabled)
    {
        vicsek3DEnabled = enabled;
        UpdateFractal();
    }
    private void DrawMeshes(List<Mesh> meshes)
    {
        if (meshes == null || meshes.Count == 0) return;
        foreach (var m in meshes)
        {
            var go = new GameObject("FractalMesh3D");
            go.transform.SetParent(transform, false);
            var mf = go.AddComponent<MeshFilter>();
            var mr = go.AddComponent<MeshRenderer>();
            mf.sharedMesh = m;
            mr.sharedMaterial = _runtimeSharedMat != null
                ? _runtimeSharedMat
                : (meshMaterial != null ? meshMaterial : new Material(Shader.Find("Standard")));
            activeCellMeshes.Add(go);
        }
    }
}