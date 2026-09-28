using UnityEngine;
using UnityEngine.UI;
public class AudioVolumeUI : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private Slider volumeSlider;

    private const float Step = 0.1f;

    private void OnEnable()
    {
        if (bgmSource == null || volumeSlider == null) return;

        volumeSlider.SetValueWithoutNotify(bgmSource.volume);
        volumeSlider.onValueChanged.AddListener(SetVolume);
    }

    private void OnDisable()
    {
        if (volumeSlider != null)
            volumeSlider.onValueChanged.RemoveListener(SetVolume);
    }

    public void SetVolume(float value)
    {
        if (bgmSource != null)
            bgmSource.volume = value;
    }

    public void DecreaseVolume()
    {
        if (volumeSlider != null)
            volumeSlider.value = Mathf.Clamp01(volumeSlider.value - Step);
    }

    public void IncreaseVolume()
    {
        if (volumeSlider != null)
            volumeSlider.value = Mathf.Clamp01(volumeSlider.value + Step);
    }
}
