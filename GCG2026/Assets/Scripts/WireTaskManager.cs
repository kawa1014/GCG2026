using UnityEngine;


/// <summary>
/// @file WireTaskManager.cs
/// @brief 配線タスクのUI表示とプレイヤーの操作モード切り替えを管理するクラス
/// @details UIを開いた際にプレイヤーの操作を停止し、マウスカーソルを解放します。
/// </summary>
public class WireTaskManager : MonoBehaviour
{
    [Header("UI設定")]
    /// <summary>表示/非表示を切り替える対象の配線タスクパネル</summary>
    [Tooltip("配線タスクのUIパネル(WireTaskPanel)をセットします")]
    public GameObject taskPanel;

    /// <summary>シーン内のプレイヤーコントローラー(操作停止用)</summary>
    private PlayerController playerController;

    //---タスク進行管理用の変数---
    private int connectedCount = 0; ///< 現在繋がっている線の数
    private int requiredConnections = 4; ///< クリアに必要な線の数(4色)

    /// <summary>
    /// @brief 初期化処理。ゲーム開始時はUIを隠し、FPS操作モードにします
    /// </summary>
    private void Start()
    {
        playerController = FindAnyObjectByType<PlayerController>();

        // ゲーム開始時は必ずタスクUIを閉じた状態にする
        CloseTaskUI();
    }

    /// <summary>
    /// @brief タスク画面を開き、UI操作モードにきりかえる処理
    /// </summary>
    public void OpenTaskUI()
    {
        if (taskPanel != null) taskPanel.SetActive(true);

        // マウスカーソルを表示し、自由に動かせるようにする
        Cursor.lockState = CursorLockMode.Confined;
        Cursor.visible = true;

        // プレイヤーの移動・視点操作を一時停止して、視点が動かないようにする
        if(playerController != null)
        {
            playerController.enabled = false;
        }

        // UIを開くたびにカウントをリセット
        connectedCount = 0;
    }

    /// <summary>
    /// @brief タスク画面、FPS操作モードに戻る処理
    /// </summary>
    public void CloseTaskUI()
    {
        if (taskPanel != null) taskPanel.SetActive(false);

        // マウスカーソルを画面中央に固定して隠す
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;

        // プレイヤーの移動・視点操作を再開する
        if (playerController != null)
        {
            playerController.enabled = true;
        }
    }

    /// <summary>
    /// @brief 線が正しく繋がった時にWireTaskNodeから呼ばれるメソッド
    /// </summary>
    public void AddConnectedWire()
    {
        connectedCount++;
        Debug.Log($"<color=cyan>【Task】配線完了：{connectedCount} / {requiredConnections}</color>");

        // 必要な数だけ繋がったらクリア
        if(connectedCount >= requiredConnections)
        {
            TaskClear();
        }
    }

    /// <summary>
    /// @brief タスクをすべて完了した際の処理
    /// </summary>
    private void TaskClear()
    {
        Debug.Log("<color=yellow>【Clear】すべての配線が完了しました！タスククリア！</color>");

        // クリアしたらUIを閉じて元のFPS視点に戻る
        CloseTaskUI();
    }
}