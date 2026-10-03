using UnityEngine;
using UnityEngine.UI;
public class GraphicQualitySelector : MonoBehaviour
{
    [SerializeField] private Image valueImage;
    [SerializeField] private Sprite lowImage;
    [SerializeField] private Sprite mediumImage;
    [SerializeField] private Sprite highImage;

    private const string SaveKey = "GraphicQuality";
    private int value;

    private void OnEnable()
    {
        value = Mathf.Clamp(PlayerPrefs.GetInt(SaveKey, 1), 0, 2);
        Apply();
    }

    public void Previous()
    {
        value = Mathf.Max(0, value - 1);
        Apply();
    }

    public void Next()
    {
        value = Mathf.Min(2, value + 1);
        Apply();
    }

    private void Apply()
    {
        if (valueImage != null)
        {
            valueImage.sprite = value switch
            {
                0 => lowImage,
                1 => mediumImage,
                _ => highImage
            };
        }

        // Quality Settings の先頭3段階を「低・中・高」として使用
        if (QualitySettings.names.Length >= 3)
            QualitySettings.SetQualityLevel(value);

        PlayerPrefs.SetInt(SaveKey, value);
        PlayerPrefs.Save();
    }
}
