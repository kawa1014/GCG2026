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
        // 1. 保存された値を読み込む（値がなければデフォルト値を入れる）
        float savedMasterVol = PlayerPrefs.GetFloat(KeyMasterVolume, 1.0f);
        float savedBgmVol = PlayerPrefs.GetFloat(KeyBgmVolume, 1.0f);
        float savedSeVol = PlayerPrefs.GetFloat(KeySeVolume, 1.0f);
        float savedGamma = PlayerPrefs.GetFloat(KeyGammaValue, 1.0f);
        float savedSensitivity = PlayerPrefs.GetFloat(KeyCameraSensitivity, 200.0f); // 初期値をPlayerSettingsに合わせた200に変更

        // 2. スライダーの見た目に反映させる
        if (_masterVolumeSlider != null) _masterVolumeSlider.value = savedMasterVol;
        if (_bgmVolumeSlider != null) _bgmVolumeSlider.value = savedBgmVol;
        if (_seVolumeSlider != null) _seVolumeSlider.value = savedSeVol;
        if (_gammaSlider != null) _gammaSlider.value = savedGamma;
        if (_cameraSensitivitySlider != null) _cameraSensitivitySlider.value = savedSensitivity;

        // 3. 実際のゲームシステムに値を適用する
        ApplyMasterVolume(savedMasterVol);
        ApplyCameraSensitivity(savedSensitivity);
    }

    private void RegisterEvents()
    {
        // タブ切り替え
        for (int i = 0; i < _tabButtons.Length; i++)
        {
            int index = i;
            if (_tabButtons[i] != null) _tabButtons[i].onClick.AddListener(() => SwitchPanel(index));
        }

        // スライダー操作時に保存 ＋ すぐにゲームに反映（Apply）させる
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

        // チュートリアル
        if (_tutorialButton != null) _tutorialButton.onClick.AddListener(GoToTutorialScene);
    }

    /// <summary>
    /// マスター音量を実際のゲームに反映させる処理
    /// </summary>
    private void ApplyMasterVolume(float volume)
    {
        // Unity全体のすべての音量を根本から変更する
        AudioListener.volume = volume;

        // OrgelManagerが存在する場合は、そちらのMasterVolumeも同期する
        if (OrgelManager.Instance != null)
        {
            OrgelManager.Instance.MasterVolume = volume;
        }
    }

    /// <summary>
    /// カメラ感度を実際のゲーム(PlayerSettings)に反映させる処理
    /// </summary>
    private void ApplyCameraSensitivity(float sensitivity)
    {
        if (_playerSettings != null)
        {
            // ScriptableObjectの値を直接書き換えることで、プレイヤーに即座に反映される
            _playerSettings.MouseSensitivity = sensitivity;
        }
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

    private void OnDestroy()
    {
        PlayerPrefs.Save();
    }
}