using UnityEngine;
using UnityEngine.UI;

public class SanGaugeUI : MonoBehaviour
{
    [Header("SAN値画像")]
    [SerializeField]
    private Image sanImage;

    [SerializeField]
    private Sprite[] sanSprites;

    private int currentSpriteIndex = -1;

    /// <summary>
    /// 0～1の割合でSAN値画像を変更する
    /// </summary>
    public void SetNormalizedValue(float normalizedValue)
    {
        if (sanImage == null || sanSprites == null || sanSprites.Length == 0)
        {
            return;
        }

        normalizedValue = Mathf.Clamp01(normalizedValue);

        int index = Mathf.RoundToInt(normalizedValue * (sanSprites.Length - 1));

        // 同じ画像なら再設定しない
        if (index == currentSpriteIndex)
        {
            return;
        }

        currentSpriteIndex = index;
        sanImage.sprite = sanSprites[index];
    }

    /// <summary>
    /// 現在値と最大値を使って画像を変更する
    /// </summary>
    public void SetValue(float currentValue, float maxValue)
    {
        if (maxValue <= 0f)
        {
            SetNormalizedValue(0f);
            return;
        }

        SetNormalizedValue(currentValue / maxValue);
    }

    public void ResetGauge()
    {
        currentSpriteIndex = -1;
        SetNormalizedValue(0f);
    }
}