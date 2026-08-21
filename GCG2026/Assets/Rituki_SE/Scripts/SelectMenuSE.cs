using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// セレクト画面のSEを管理するクラス
/// </summary>
public class SelectMenuSE : MonoBehaviour
{
    /// <summary>
    /// SE再生用AudioSource
    /// </summary>
    [SerializeField]
    [Tooltip("SEを再生するAudioSource")]
    private AudioSource audioSource;

    /// <summary>
    /// カーソル移動SE
    /// </summary>
    [SerializeField]
    [Tooltip("カーソル移動時に鳴らすSE")]
    private AudioClip cursorMoveSE;

    /// <summary>
    /// 決定SE
    /// </summary>
    [SerializeField]
    [Tooltip("ステージを決定したときに鳴らすSE")]
    private AudioClip decisionSE;

    /// <summary>
    /// 戻るSE
    /// </summary>
    [SerializeField]
    [Tooltip("タイトルへ戻るときに鳴らすSE")]
    private AudioClip backSE;

    /// <summary>
    /// スティック入力中か
    /// </summary>
    private bool stickMoved;

    /// <summary>
    /// 毎フレーム入力を確認
    /// </summary>
    private void Update()
    {
        CheckKeyboard();
        CheckGamepad();
    }

    /// <summary>
    /// キーボード入力を確認
    /// </summary>
    private void CheckKeyboard()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.upArrowKey.wasPressedThisFrame ||
            Keyboard.current.downArrowKey.wasPressedThisFrame ||
            Keyboard.current.wKey.wasPressedThisFrame ||
            Keyboard.current.sKey.wasPressedThisFrame)
        {
            PlayMoveSE();
        }
    }

    /// <summary>
    /// ゲームパッド入力を確認
    /// </summary>
    private void CheckGamepad()
    {
        if (Gamepad.current == null)
        {
            return;
        }

        // 十字キー
        if (Gamepad.current.dpad.up.wasPressedThisFrame ||
            Gamepad.current.dpad.down.wasPressedThisFrame)
        {
            PlayMoveSE();
        }

        // 左スティック
        float vertical = Gamepad.current.leftStick.y.ReadValue();

        if (Mathf.Abs(vertical) < 0.3f)
        {
            stickMoved = false;
        }
        else if (Mathf.Abs(vertical) >= 0.7f && !stickMoved)
        {
            PlayMoveSE();
            stickMoved = true;
        }
    }

    /// <summary>
    /// カーソル移動SEを鳴らす
    /// </summary>
    private void PlayMoveSE()
    {
        if (audioSource == null || cursorMoveSE == null)
        {
            return;
        }

        audioSource.PlayOneShot(cursorMoveSE);
    }

    /// <summary>
    /// 決定SEを鳴らす
    /// </summary>
    public void PlayDecisionSE()
    {
        if (audioSource == null || decisionSE == null)
        {
            return;
        }

        audioSource.PlayOneShot(decisionSE);
    }

    /// <summary>
    /// 戻るSEを鳴らす
    /// </summary>
    public void PlayBackSE()
    {
        if (audioSource == null || backSE == null)
        {
            return;
        }

        audioSource.PlayOneShot(backSE);
    }
}