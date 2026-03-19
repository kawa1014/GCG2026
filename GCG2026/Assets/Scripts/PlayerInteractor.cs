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

    /// <summary>
    /// プレイヤーのカメラ(ここから視線の光線を飛ばします)
    /// </summary>
    [Tooltip("プレイヤーのカメラ")]
    public Camera playerCamera;

    private void Update()
    {
        // シーンビューに赤い光線を可視化するプロのデバッグ技
        if(playerCamera != null)
        {
            Debug.DrawRay(playerCamera.transform.position, playerCamera.transform.forward * interactRange, Color.red);
        }

        // マウスの右クリックが押された瞬間を検知
        if(Mouse.current != null && Mouse.current.leftButton.wasPressedThisFrame)
        {
            TryInteract();
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
                // オルゴール側の切り替え処理を呼び出す
                orgel.TogglePlayer();
                Debug.Log("<color=green> [Interact]オルゴールを操作しました!</color>");
            }
        }
    }
}
