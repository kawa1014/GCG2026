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
    public float interactRange = 3.0f;
    [Tooltip("解除に必要な時間")]
    public float requiredHoldTime = 3.0f;

    /// <summary>
    /// プレイヤーのカメラ(ここから視線の光線を飛ばします)
    /// </summary>
    [Tooltip("プレイヤーのカメラ")]
    public Camera playerCamera;

    //---内部状態---
    private float currentHoldTime = 0.0f; ///< 現在の長押し経過時間

    private void Update()
    {

        // 左クリックが「今」押されているかチェック（長押し判定）
        if (Mouse.current != null && Mouse.current.leftButton.isPressed)
        {
            CheckAndInteract();
        }
        else
        {
            // 指を離したらカウントをリセット
            ResetInteraction();
        }
    }

    /// <summary>
    /// @brief 視線の先をチェックし、有効対象なら進捗を進める
    /// </summary>
    private void CheckAndInteract()
    {
        if (playerCamera == null) return;

        Vector3 origin = playerCamera.transform.position
                         - playerCamera.transform.up * 0.3f
                         + playerCamera.transform.right * 0.3f;

        Ray ray = new Ray(origin, playerCamera.transform.forward);
        RaycastHit hitInfo;

        // レイを飛ばして何かに当たったか
        if(Physics.Raycast(ray, out hitInfo, interactRange))
        {
            OrgelSystem orgel = hitInfo.collider.GetComponent<OrgelSystem>();
            
            // 鳴っているオルゴールに当たっている場合
            if(orgel != null && orgel.isPlaying)
            {
                currentHoldTime += Time.deltaTime; // 内部で時間を加算

                // デバッグ用にコンソールに進捗を表示
                Debug.Log($"<color=cyan>【Action】解除中... {currentHoldTime:F1} / {requiredHoldTime} 秒</color>");

                // 規定時間に達したら解除
                if(currentHoldTime >= requiredHoldTime)
                {
                    orgel.TurnOff();
                    ResetInteraction(); // 解除完了したのでリセット
                    Debug.Log("<color=green>【Action】長押しによる解除に成功！</color>");
                }
                return;
            }
        }

        // 何にも当たっていない、またはオルゴールから視線が外れた場合はリセット
        ResetInteraction();
    }

    /// <summary>
    /// @brief 長押し状態をリセットする
    /// </summary>
    private void ResetInteraction()
    {
        // カウントダウンが0より大きい場合のみリセットログを出す
        if(currentHoldTime > 0.0f)
        {
            currentHoldTime = 0.0f;
            Debug.Log("<color=orange>【Action】解除が中断されました。</color>");
        }
    }

    /// <summary>
    /// 視線の先にインタラクト可能なオブジェクトがあるか判定して実行する処理
    /// </summary>
    private void TryInteract()
    {
        // カメラがセットされていなければ処理を中断
        if (playerCamera == null) return;

        // カメラの位置から、カメラが向いている前方向へRayを作る
        Ray ray = new Ray(playerCamera.transform.position, playerCamera.transform.forward);
        RaycastHit hitInfo; // 当たった物の情報を入れる箱を用意

        // 光線を飛ばして、何かに当たったか判定(out hitInfoに情報が入ります)
        if (Physics.Raycast(ray, out hitInfo, interactRange))
        {
            // 当たったオブジェクトがOrgelSystemコンポーネントを持っているか確認
            OrgelSystem orgel = hitInfo.collider.GetComponent<OrgelSystem>();

            // もし持っていたら
            if (orgel != null)
            {
                // 鳴っている時だけTurnOff()を呼ぶようにする
                if(orgel.isPlaying)
                {
                    orgel.TurnOff();
                }
                else
                {
                    Debug.Log("【Interact】今は鳴っていません。");
                }
            }
        }
    }

    /// <summary>
    /// @brief ギズモを描画するUnity標準のメソッド
    /// @details 選択時だけでなく常に表示したい場合はOnDrawGizmos()を使用します
    /// </summary>
    private void OnDrawGizmos()
    {
        // カメラがセットされていなければ処理を中断
        if (playerCamera == null) return;

        // ギズモの色を赤に設定
        Gizmos.color = Color.red;

        // カメラの現在位置と、向いている方向を取得
        Vector3 origin = playerCamera.transform.position
                         - playerCamera.transform.up * 0.3f
                         + playerCamera.transform.right * 0.3f;
        Vector3 forward = playerCamera.transform.forward;

        // 1本目：メインRay
        Gizmos.DrawRay(origin, forward * interactRange);

        // 2本目：左に少し傾けたRay
        Vector3 leftRay = Quaternion.Euler(0, -5, 0) * forward;
        Gizmos.DrawRay(origin, leftRay * interactRange);

        // 3本目：右に少し傾けたRay（Y軸で 5度 回転させる）
        Vector3 rightRay = Quaternion.Euler(0, 5, 0) * forward;
        Gizmos.DrawRay(origin, rightRay * interactRange);
    }
}
