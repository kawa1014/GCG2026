using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
public class AudioVolumeUI : MonoBehaviour
{
    [SerializeField] private AudioSource bgmSource;
    [SerializeField] private Slider volumeSlider;
    [SerializeField] private string saveKey = "BGMVolume";

    private const float Step = 0.1f;

    private void OnEnable()
    {
        if (volumeSlider == null) return;

        float savedVolume = PlayerPrefs.GetFloat(saveKey, 0.5f);
        volumeSlider.SetValueWithoutNotify(savedVolume);

        if (bgmSource != null)
            bgmSource.volume = savedVolume;

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

        PlayerPrefs.SetFloat(saveKey, value);
        PlayerPrefs.Save();
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
