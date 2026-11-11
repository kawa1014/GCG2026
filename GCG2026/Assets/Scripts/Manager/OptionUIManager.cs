using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

/// <summary>
/// オプション画面のUIと設定値を一括管理するクラス
/// </summary>
public class OptionUIManager : MonoBehaviour
{
    [Header("連携するデータ")]
    [Tooltip("プレイヤーの設定データ（PlayerSettings）をアタッチしてください")]
    [SerializeField] private PlayerSettings _playerSettings;

    [Header("タブ切り替え設定")]
    [SerializeField] private Button[] _tabButtons;
    [SerializeField] private GameObject[] _settingPanels;

    [Header("オーディオ設定UI")]
    [SerializeField] private Slider _masterVolumeSlider;
    [SerializeField] private Slider _bgmVolumeSlider;
    [SerializeField] private Slider _seVolumeSlider;

    [Header("グラフィック設定UI")]
    [SerializeField] private Slider _gammaSlider;

    [Header("カメラ設定UI")]
    [SerializeField] private Slider _cameraSensitivitySlider;

    [Header("チュートリアル移行設定")]
    [SerializeField] private Button _tutorialButton;
    [SerializeField] private string _tutorialSceneName = "TutorialScene";

    [Header("戻る設定")]
    [Tooltip("オプション画面を閉じるボタン（exitボタン）")]
    [SerializeField] private Button _closeButton;

    // ★追加：フェード処理を行うスクリプトへの参照（クラス名はそのままNewMonoBehaviourScriptを使用）
    [Tooltip("タイトルへ戻るフェード演出用。ヒエラルキーのFadeControllerをアタッチしてください")]
    [SerializeField] private NewMonoBehaviourScript _fadeScript;

    // 保存キー
    private const string KeyMasterVolume = "MasterVolume";
    private const string KeyBgmVolume = "BGMVolume";
    private const string KeySeVolume = "SEVolume";
    private const string KeyGammaValue = "GammaValue";
    private const string KeyCameraSensitivity = "CameraSensitivity";

    private void Start()
    {
        LoadSettings();
        RegisterEvents();
        SwitchPanel(0);
    }

    private void LoadSettings()
    {
        float savedMasterVol = PlayerPrefs.GetFloat(KeyMasterVolume, 1.0f);
        float savedBgmVol = PlayerPrefs.GetFloat(KeyBgmVolume, 1.0f);
        float savedSeVol = PlayerPrefs.GetFloat(KeySeVolume, 1.0f);
        float savedGamma = PlayerPrefs.GetFloat(KeyGammaValue, 1.0f);
        float savedSensitivity = PlayerPrefs.GetFloat(KeyCameraSensitivity, 200.0f);

        if (_masterVolumeSlider != null) _masterVolumeSlider.value = savedMasterVol;
        if (_bgmVolumeSlider != null) _bgmVolumeSlider.value = savedBgmVol;
        if (_seVolumeSlider != null) _seVolumeSlider.value = savedSeVol;
        if (_gammaSlider != null) _gammaSlider.value = savedGamma;
        if (_cameraSensitivitySlider != null) _cameraSensitivitySlider.value = savedSensitivity;

        ApplyMasterVolume(savedMasterVol);
        ApplyCameraSensitivity(savedSensitivity);
    }

    private void RegisterEvents()
    {
        for (int i = 0; i < _tabButtons.Length; i++)
        {
            int index = i;
            if (_tabButtons[i] != null) _tabButtons[i].onClick.AddListener(() => SwitchPanel(index));
        }

        if (_masterVolumeSlider != null)
            _masterVolumeSlider.onValueChanged.AddListener(val => {
                PlayerPrefs.SetFloat(KeyMasterVolume, val);
                ApplyMasterVolume(val);
            });

        if (_bgmVolumeSlider != null)
            _bgmVolumeSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat(KeyBgmVolume, val));

        if (_seVolumeSlider != null)
            _seVolumeSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat(KeySeVolume, val));

        if (_gammaSlider != null)
            _gammaSlider.onValueChanged.AddListener(val => PlayerPrefs.SetFloat(KeyGammaValue, val));

        if (_cameraSensitivitySlider != null)
            _cameraSensitivitySlider.onValueChanged.AddListener(val => {
                PlayerPrefs.SetFloat(KeyCameraSensitivity, val);
                ApplyCameraSensitivity(val);
            });

        if (_tutorialButton != null) _tutorialButton.onClick.AddListener(GoToTutorialScene);
        if (_closeButton != null) _closeButton.onClick.AddListener(CloseOption);
    }

    private void ApplyMasterVolume(float volume)
    {
        AudioListener.volume = volume;
        if (OrgelManager.Instance != null) OrgelManager.Instance.MasterVolume = volume;
    }

    private void ApplyCameraSensitivity(float sensitivity)
    {
        if (_playerSettings != null) _playerSettings.MouseSensitivity = sensitivity;
    }

    private void SwitchPanel(int activeIndex)
    {
        for (int i = 0; i < _settingPanels.Length; i++)
        {
            if (_settingPanels[i] != null) _settingPanels[i].SetActive(i == activeIndex);
        }
    }

    private void GoToTutorialScene()
    {
        PlayerPrefs.Save();
    }

    private void CloseOption()
    {
        PlayerPrefs.Save();

        if (SceneManager.sceneCount > 1)
        {
            // プレイ中（ポーズ中）に開かれた場合は、ポーズを再開して自身を破棄
            Pause pauseScript = Object.FindFirstObjectByType<Pause>();
            if (pauseScript != null)
            {
                pauseScript.ReturnFromOption();
            }
            SceneManager.UnloadSceneAsync("OptionScene");
        }
        else
        {
            // ★変更：タイトル画面から開かれた場合は、Fadeスクリプトの「オプション→タイトル遷移」を呼び出す
            if (_fadeScript != null)
            {
                _fadeScript.OnClickStartButton7(); // Fade.csに書かれているタイトルへ戻る処理
            }
            else
            {
                // ※万が一Fadeスクリプトが設定されていなかった場合の保険
                SceneManager.LoadScene("TitleScene");
            }
        }
    }

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}