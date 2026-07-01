using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// プレイヤーの視線中央にあるオブジェクトをインタラクトするクラス
/// </summary>
public class PlayerInteractor : MonoBehaviour
{
    /// <summary>
    /// プレイヤーがオブジェクトに手が届く距離
    /// </summary>
    [Tooltip("インタラクトできる距離")]
    public float InteractRange = 3.0f;
    [Tooltip("解除に必要な時間")]
    public float RequiredHoldTime = 3.0f;

    /// <summary>
    /// プレイヤーのカメラ(ここから視線の光線を飛ばします)
    /// </summary>
    [Tooltip("プレイヤーのカメラ")]
    public Camera PlayerCamera;

    //---内部状態---
    private float _currentHoldTime = 0.0f; ///< 現在の長押し経過時間

    // ハイライト用
    private Renderer _lastRenderer = null;
    private Color _originalColor;
    public UnityEngine.UI.Image ReticleImage;
    public Color NormalReticleColor = Color.white;
    public Color HighlightReticleColor = Color.red;
    private void Update()
    {

        // 1.左クリック長押しによるインタラクト（オルゴール解除など）
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            HandleHoldInteraction();
        }
        else
        {
            ResetHoldInteraction();
        }

        // 2.スペースキーによる即時インタラクト（ドアの開閉のみ）
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            HandleQuickInteraction();
        }

        // 3.見ているオブジェクトのハイライト処理
        HandleHightlight();
    }

    /// <summary>
    /// 長押し（左クリック）の処理。対象がドアの場合は何もしない。
    /// </summary>
    private void HandleHoldInteraction()
    {
        if (PlayerCamera == null) return;
        
        // Rayを視点から少し右下から出す
        Vector3 origin = PlayerCamera.transform.position
                         - PlayerCamera.transform.up * 0.3f
                         + PlayerCamera.transform.right * 0.3f;

        Ray ray = new Ray(origin, PlayerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, InteractRange))
        {
            IInteractable interactableObj = hit.collider.GetComponent<IInteractable>();

            if (interactableObj != null && interactableObj.IsInteractable)
            {
                // ★修正点：対象が『ドア（DoorSystem）』を持っていたら、長押し処理をスルー（中断）する
                if (hit.collider.GetComponent<DoorSystem>() != null)
                {
                    ResetHoldInteraction(); // ゲージをリセットして何もしない
                    return;
                }

                _currentHoldTime += Time.deltaTime;
                Debug.Log($"<color=cyan>【Action】解除中... {_currentHoldTime:F1} / {RequiredHoldTime} 秒</color>");

                if (_currentHoldTime >= RequiredHoldTime)
                {
                    interactableObj.ExecuteInteraction();
                    _currentHoldTime = 0.0f;
                    Debug.Log("<color=green>【Action】解除成功！</color>");
                }
                return;
            }
        }
        ResetHoldInteraction();
    }

    /// <summary>
    /// 即時実行（スペースキー）の処理
    /// </summary>
    private void HandleQuickInteraction()
    {
        if (PlayerCamera == null) return;

        Ray ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);
        if (Physics.Raycast(ray, out RaycastHit hit, InteractRange))
        {
            IInteractable interactableObj = hit.collider.GetComponent<IInteractable>();

            // 看板(IInteractable)を持っていて、有効なら即座に実行（ドアが開く）
            if (interactableObj != null && interactableObj.IsInteractable)
            {
                interactableObj.ExecuteInteraction();
                Debug.Log("<color=yellow>【Action】スペースキー実行</color>");
            }
        }
    }

    private void ResetHoldInteraction()
    {
        if (_currentHoldTime > 0.0f)
        {
            _currentHoldTime = 0.0f;
            Debug.Log("<color=orange>【Action】長押しがリセットされました。</color>");
        }
    }

    /// <summary>
    /// 見ているオブジェクトがドアなら赤くハイライトする
    /// </summary>
    private void HandleHightlight()
    {
        if (PlayerCamera == null) return;

        Ray ray = new Ray(PlayerCamera.transform.position, PlayerCamera.transform.forward);

        if (Physics.Raycast(ray, out RaycastHit hit, InteractRange))
        {
            DoorSystem door = hit.collider.GetComponent<DoorSystem>();

            if (door != null)
            {
                Renderer r = hit.collider.GetComponent<Renderer>();
                if (r != null)
                {
                    // 前のオブジェクトの色を戻す
                    if (_lastRenderer != null && _lastRenderer != r)
                    {
                        _lastRenderer.material.color = _originalColor;
                    }

                    // 新しいオブジェクトをハイライト
                    if (_lastRenderer != r)
                    {
                        _lastRenderer = r;
                        _originalColor = r.material.color;
                        r.material.color = Color.red;
                    }

                    // ★レティクルを赤に変更
                    if (ReticleImage != null)
                        ReticleImage.color = HighlightReticleColor;

                    return;
                }
            }
        }

        // 何も見ていない or ドア以外 → 色を戻す
        if (_lastRenderer != null)
        {
            _lastRenderer.material.color = _originalColor;
            _lastRenderer = null;
        }

        // ★レティクルを通常色に戻す
        if (ReticleImage != null)
            ReticleImage.color = NormalReticleColor;
    }

    /// <summary>
    /// @brief ギズモを描画するUnity標準のメソッド
    /// @details 選択時だけでなく常に表示したい場合はOnDrawGizmos()を使用します
    /// </summary>
    private void OnDrawGizmos()
    {
        // カメラがセットされていなければ処理を中断
        if (PlayerCamera == null) return;

        // ギズモの色を赤に設定
        Gizmos.color = Color.red;

        // カメラの現在位置と、向いている方向を取得
        Vector3 origin = PlayerCamera.transform.position
                         - PlayerCamera.transform.up * 0.3f
                         + PlayerCamera.transform.right * 0.3f;
        Vector3 forward = PlayerCamera.transform.forward;

        // 1本目：メインRay
        Gizmos.DrawRay(origin, forward * InteractRange);

        // 2本目：左に少し傾けたRay
        Vector3 leftRay = Quaternion.Euler(0, -5, 0) * forward;
        Gizmos.DrawRay(origin, leftRay * InteractRange);

        // 3本目：右に少し傾けたRay（Y軸で 5度 回転させる）
        Vector3 rightRay = Quaternion.Euler(0, 5, 0) * forward;
        Gizmos.DrawRay(origin, rightRay * InteractRange);
    }
}
