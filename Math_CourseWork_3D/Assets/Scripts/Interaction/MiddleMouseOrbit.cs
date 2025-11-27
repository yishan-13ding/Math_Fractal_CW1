using UnityEngine;

// 中键拖拽旋转目标，但不改变其位置；让模型始终围绕自身中心旋转。
public class MiddleMouseOrbit : MonoBehaviour
{
    [Header("目标（不指定则为自身）")]
    public Transform target;

    [Header("旋转速度（度/像素）")]
    public float yawSpeed = 0.25f;   // 鼠标X → 绕Y轴
    public float pitchSpeed = 0.25f; // 鼠标Y → 绕X轴（取反以符合常见手感）

    [Header("俯仰角限制")]
    public float pitchMin = -80f;
    public float pitchMax = 80f;

    private bool _dragging;
    private Vector3 _lastMouse;
    private float _yaw, _pitch;
    private Quaternion _initialRot;

    void Awake()
    {
        if (target == null) target = transform;
        // 记录初始旋转与欧拉角（避免万向锁，用累积欧拉控制）
        _initialRot = target.localRotation;
        Vector3 e = target.localEulerAngles;
        _yaw = e.y;
        _pitch = NormalizeAngle(e.x);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(2)) // 中键按下
        {
            _dragging = true;
            _lastMouse = Input.mousePosition;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            _dragging = false;
        }

        if (!_dragging) return;

        Vector3 m = Input.mousePosition;
        Vector3 delta = m - _lastMouse;
        _lastMouse = m;

        // 累积欧拉角（不改变位置）
        _yaw   -= delta.x * yawSpeed;
        _pitch -= delta.y * pitchSpeed;
        _pitch = Mathf.Clamp(_pitch, pitchMin, pitchMax);

        target.localRotation = Quaternion.Euler(_pitch, _yaw, 0f);
    }

    private static float NormalizeAngle(float a)
    {
        while (a > 180f) a -= 360f;
        while (a < -180f) a += 360f;
        return a;
    }

    public void ResetRotation()
    {
        if (target == null) target = transform;
        target.localRotation = _initialRot;
        Vector3 e = target.localEulerAngles;
        _yaw = e.y;
        _pitch = NormalizeAngle(e.x);
    }
}