using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// リザルト画面のSEを管理するクラス
/// </summary>
public class ResultMenuSE : MonoBehaviour
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
    [Tooltip("左右にカーソルを移動したときのSE")]
    private AudioClip cursorMoveSE;

    /// <summary>
    /// 決定SE
    /// </summary>
    [SerializeField]
    [Tooltip("Selectを決定したときのSE")]
    private AudioClip decisionSE;

    /// <summary>
    /// 戻るSE
    /// </summary>
    [SerializeField]
    [Tooltip("Titleを決定したときのSE")]
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
    /// キーボードの左右入力を確認
    /// </summary>
    private void CheckKeyboard()
    {
        if (Keyboard.current == null)
        {
            return;
        }

        if (Keyboard.current.leftArrowKey.wasPressedThisFrame ||
            Keyboard.current.rightArrowKey.wasPressedThisFrame ||
            Keyboard.current.aKey.wasPressedThisFrame ||
            Keyboard.current.dKey.wasPressedThisFrame)
        {
            PlayMoveSE();
        }
    }

    /// <summary>
    /// ゲームパッドの左右入力を確認
    /// </summary>
    private void CheckGamepad()
    {
        if (Gamepad.current == null)
        {
            return;
        }

        // 十字キー左右
        if (Gamepad.current.dpad.left.wasPressedThisFrame ||
            Gamepad.current.dpad.right.wasPressedThisFrame)
        {
            PlayMoveSE();
        }

        // 左スティック左右
        float horizontal = Gamepad.current.leftStick.x.ReadValue();

        if (Mathf.Abs(horizontal) < 0.3f)
        {
            stickMoved = false;
        }
        else if (Mathf.Abs(horizontal) >= 0.7f && !stickMoved)
        {
            PlayMoveSE();
            stickMoved = true;
        }
    }

    /// <summary>
    /// カーソル移動SE
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
    /// Select決定SE
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
    /// Titleへ戻るSE
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