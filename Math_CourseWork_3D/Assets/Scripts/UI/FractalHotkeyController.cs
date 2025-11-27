using UnityEngine;
using TMPro;

public class FractalHotkeyController : MonoBehaviour
{
    [Header("引用")]
    public FractalUIController uiController;          // 拖入已有的 UI 控制脚本
    public FractalManager fractalManager;             // 可直接调用 SetPreset / SetPresetByIndex
    public TMP_Dropdown fractalDropdown;              // 若不从 uiController 取，可单独拖入

    [Header("热键步进")]
    [SerializeField] private float sizeStep = 0.1f;   // 左右方向键步进
    [SerializeField] private float sizeSpeed = 1.0f;       // 按住左右键的每秒变化量
    [SerializeField] private float sizeSpeedFast = 3.0f;

    void Update()
    {

        if (fractalManager == null || fractalDropdown == null) return;

        // 数字键 1-8（主键盘）
        if (Input.GetKeyDown(KeyCode.Alpha1)) SwitchPreset(1);
        else if (Input.GetKeyDown(KeyCode.Alpha2)) SwitchPreset(2);
        else if (Input.GetKeyDown(KeyCode.Alpha3)) SwitchPreset(3);
        else if (Input.GetKeyDown(KeyCode.Alpha4)) SwitchPreset(4);
        else if (Input.GetKeyDown(KeyCode.Alpha5)) SwitchPreset(5);
        else if (Input.GetKeyDown(KeyCode.Alpha6)) SwitchPreset(6);
        else if (Input.GetKeyDown(KeyCode.Alpha7)) SwitchPreset(7);
        else if (Input.GetKeyDown(KeyCode.Alpha8)) SwitchPreset(8);

        // 方向键控制迭代上下
        if (Input.GetKeyDown(KeyCode.UpArrow))
        {
            IncrementIteration(+1);
        }
        else if (Input.GetKeyDown(KeyCode.DownArrow))
        {
            IncrementIteration(-1);
        }

        // 方向键 左右 控制 scale（size）
        int dir = (Input.GetKey(KeyCode.RightArrow) ? 1 : 0) - (Input.GetKey(KeyCode.LeftArrow) ? 1 : 0);
        if (dir != 0)
        {
            float speed = (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)) ? sizeSpeedFast : sizeSpeed;
            float delta = dir * speed * Time.unscaledDeltaTime; // 与帧率无关
            AdjustSize(delta);
        }
    }

    private void SwitchPreset(int index)
    {
        // 更新下拉框的选中值，触发其回调（FractalUIController 已监听 onValueChanged）
        fractalDropdown.value = index;
        fractalDropdown.RefreshShownValue();

        // 为防止未触发（某些情况下回调被禁用），直接调用 Manager 的方法双保险
        fractalManager.SetPresetByIndex(index-1);

        if (uiController != null)
            uiController.SetIterationFromHotkey(0);
    }

    private void IncrementIteration(int delta)
    {
        if (fractalManager == null) return;
        int newValue = Mathf.Clamp(fractalManager.iteration + delta, 0, fractalManager.maxIteration);
        if (newValue == fractalManager.iteration) return;

        // 通过 UI 控制器统一同步 Slider + 文本 + FractalManager
        if (uiController != null)
            uiController.SetIterationFromHotkey(newValue);
        else
            fractalManager.iteration = newValue;
    }

    private void AdjustSize(float delta)
    {
        if (fractalManager == null) return;

        // 取 Slider 的范围；若无则退回 FractalManager 的默认 Range(0.5~6)
        float min = 0.5f, max = 6.0f;
        if (uiController != null && uiController.sizeSlider != null)
        {
            min = uiController.sizeSlider.minValue;
            max = uiController.sizeSlider.maxValue;
        }

        float newSize = Mathf.Clamp(fractalManager.size + delta, min, max);
        if (Mathf.Abs(newSize - fractalManager.size) < 1e-6f) return;

        // 应用 size 并重建
        fractalManager.SetSize(newSize);

        // 同步 UI（不触发回调）
        if (uiController != null && uiController.sizeSlider != null)
            uiController.sizeSlider.SetValueWithoutNotify(newSize);
    }
}
