using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// メニューカーソルを上下に動かしたときにSEを鳴らす
/// </summary>
public class MenuCursorSE : MonoBehaviour
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
    [Tooltip("カーソルを移動したときのSE")]
    private AudioClip cursorMoveSE;

    /// <summary>
    /// スティックを倒しているか
    /// </summary>
    private bool stickMoved = false;

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

        // ↑ ↓ または W S を押した瞬間
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
    /// カーソル移動SEを再生
    /// </summary>
    private void PlayMoveSE()
    {
        if (audioSource == null || cursorMoveSE == null)
        {
            return;
        }

        audioSource.PlayOneShot(cursorMoveSE);
    }
}