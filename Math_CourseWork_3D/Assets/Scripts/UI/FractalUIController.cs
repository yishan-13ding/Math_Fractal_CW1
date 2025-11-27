using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;
//using UnityEngine.UIElements;

public class FractalUIController : MonoBehaviour
{
    public TMP_Dropdown fractalDropdown;
    public Slider rotationSlider;              
    public Slider interactionSlider;              
    public Slider sizeSlider;              
    public FractalManager fractal;
    public Material targetMaterial;
    public Toggle vicsek3DToggle;   // 在 Inspector 绑定 Toggle

    public TMP_Text iterationText;
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        interactionSlider.value = interactionSlider.minValue;
        sizeSlider.value = sizeSlider.minValue;
        rotationSlider.value = rotationSlider.minValue;


        rotationSlider.onValueChanged.AddListener(OnAngleChanged);
        interactionSlider.onValueChanged.AddListener(OnIterationChanged);
        sizeSlider.onValueChanged.AddListener(OnSizeChanged);
        fractalDropdown.onValueChanged.AddListener(OnDropdownChanged);
        RefreshIterationText(Mathf.RoundToInt(interactionSlider.value));
        vicsek3DToggle.onValueChanged.AddListener(OnVicsek3DToggleChanged);
        Update3DToggleVisibility(fractal != null ? fractal.CurrentType : GetSelectedTypeName());
    }

    // 在 Button 的 OnClick 里绑定这个方法即可
    public void OnSpinButtonClicked()
    {
        if (fractal != null)
            fractal.SpinY(2f); // 2 秒旋转 360°，可在 Inspector 中改时长
    }

    private void OnDropdownChanged(int index)
    {
        ResetSlidersToMin();
        fractal.SetPresetByIndex(index);
        RefreshIterationText(Mathf.RoundToInt(interactionSlider.value));
        Update3DToggleVisibility(fractal != null ? fractal.CurrentType : GetSelectedTypeName());
    }
    void OnAngleChanged(float value)
    {
        fractal.SetRotation(value);
    }
    void OnIterationChanged(float value)
    {
        fractal.iteration = Mathf.RoundToInt(value); 
        RefreshIterationText(fractal.iteration);
    }
    void OnSizeChanged(float value)
    {
        fractal.SetSize(value+3);
    }

    public void ChangeColor()
    {
        Color newColor = new Color(
            Random.value,
            Random.value,
            Random.value
        );
        targetMaterial.color = newColor;
    }

    private void ResetSlidersToMin()
    {
        if (interactionSlider != null) interactionSlider.value = interactionSlider.minValue;
        if (sizeSlider != null) sizeSlider.value = sizeSlider.minValue;
        if (rotationSlider != null) rotationSlider.value = rotationSlider.minValue;
    }

    public void SetIterationFromHotkey(int value)
    {
        if (interactionSlider != null)
            interactionSlider.SetValueWithoutNotify(value); // 不触发回调，避免重复
        fractal.iteration = value; // Update() 会检测并重建
        RefreshIterationText(value);
    }

    private void RefreshIterationText(int value)
    {
        if (iterationText != null)
            iterationText.text = $"{value}";
    }

    private void OnVicsek3DToggleChanged(bool value)
    {
        fractal.SetVicsek3DEnabled(value);
    }
    // 从 Dropdown 取当前选项文本（与配置里的 type 对应）
    private string GetSelectedTypeName()
    {
        if (fractalDropdown == null || fractalDropdown.options.Count == 0) return "";
        int i = Mathf.Clamp(fractalDropdown.value, 0, fractalDropdown.options.Count - 1);
        return fractalDropdown.options[i].text;
    }
    // 仅当 type 为 vicsek/tsquare/sierpinski 时显示 3D toggle
    private void Update3DToggleVisibility(string typeName)
    {
        if (vicsek3DToggle == null) return;
        string t = string.IsNullOrEmpty(typeName) ? "" : typeName.ToLowerInvariant().Trim();
        bool show = t == "vicsek" || t == "tsquare" || t == "t" || t == "sierpinski";
        vicsek3DToggle.gameObject.SetActive(show);

        if (!show)
        {
            vicsek3DToggle.isOn = false;
            fractal.SetVicsek3DEnabled(false);
        }
    }
}
