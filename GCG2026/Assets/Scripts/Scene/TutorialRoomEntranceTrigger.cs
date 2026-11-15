using UnityEngine;

// プレイヤーが特定のエリア（部屋など）に入ったことを検知するトリガー
public class TutorialRoomEntranceTrigger : MonoBehaviour
{
    [Header("連携するチュートリアル管理スクリプト")]
    [Tooltip("ヒエラルキーのTutorialManagerなどをアタッチしてください")]
    [SerializeField] private TutorialCaller tutorialCaller;

    // トリガーが1回だけ発動するようにするフラグ
    private bool hasTriggered = false;

    private void OnTriggerEnter(Collider other)
    {
        // 既に発動済みなら何もしない
        if (hasTriggered) return;

        // ぶつかった相手が「Player」タグを持っているか確認する
        if (other.CompareTag("Player"))
        {
            hasTriggered = true;

            // TutorialCallerに「フェーズ7を表示して！」と合図を送る
            if (tutorialCaller != null)
            {
                tutorialCaller.ShowPhase7();
            }
            else
            {
                Debug.LogWarning("TutorialRoomEntranceTrigger に TutorialCaller が設定されていません！");
            }
        }
    }
}