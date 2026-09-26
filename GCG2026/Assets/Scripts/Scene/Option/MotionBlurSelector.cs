using UnityEngine;
using UnityEngine.UI;

public class MotionBlurSelector : MonoBehaviour
{
    [SerializeField] private Image valueImage;
    [SerializeField] private Sprite offImage;
    [SerializeField] private Sprite onImage;

    private const string SaveKey = "MotionBlur";
    private bool isOn;

    private void OnEnable()
    {
        isOn = PlayerPrefs.GetInt(SaveKey, 0) == 1;
        Apply();
    }

    public void Previous()
    {
        isOn = false;
        Apply();
    }

    public void Next()
    {
        isOn = true;
        Apply();
    }

    private void Apply()
    {
        if (valueImage != null)
            valueImage.sprite = isOn ? onImage : offImage;

        PlayerPrefs.SetInt(SaveKey, isOn ? 1 : 0);
        PlayerPrefs.Save();
    }
}
