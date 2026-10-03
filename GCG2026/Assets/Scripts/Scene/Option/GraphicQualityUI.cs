using UnityEngine;
using UnityEngine.UI;
public class GraphicQualityUI : MonoBehaviour
{
    [SerializeField] private Slider qualitySlider;

    private const string SaveKey = "GraphicQuality";

    private void OnEnable()
    {
        if (qualitySlider == null) return;

        int maxLevel = Mathf.Max(0, QualitySettings.names.Length - 1);
        qualitySlider.minValue = 0;
        qualitySlider.maxValue = maxLevel;
        qualitySlider.wholeNumbers = true;

        int savedLevel = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, QualitySettings.GetQualityLevel()),
            0, maxLevel);

        qualitySlider.SetValueWithoutNotify(savedLevel);
        QualitySettings.SetQualityLevel(savedLevel);

        qualitySlider.onValueChanged.AddListener(SetQuality);
    }

    private void OnDisable()
    {
        if (qualitySlider != null)
            qualitySlider.onValueChanged.RemoveListener(SetQuality);
    }

    public void SetQuality(float value)
    {
        int level = Mathf.RoundToInt(value);
        QualitySettings.SetQualityLevel(level);
        PlayerPrefs.SetInt(SaveKey, level);
        PlayerPrefs.Save();
    }

    public void DecreaseQuality()
    {
        if (qualitySlider != null)
            qualitySlider.value -= 1;
    }

    public void IncreaseQuality()
    {
        if (qualitySlider != null)
            qualitySlider.value += 1;
    }
}
