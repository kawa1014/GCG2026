using UnityEngine;

/// <summary>
/// オルゴールの状態を監視し、敵の出現と消滅を管理するクラス
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    /// <summary>
    /// 監視対象となるオルゴールのスクリプト
    /// </summary>
    [Tooltip("音源の状態を管理するスクリプト")]
    public OrgelSystem orgelSystem;

    /// <summary>
    /// 出現させる敵のプレハブ
    /// </summary>
    [Tooltip("出現させる敵のプレハブ")]
    public GameObject enemyPrefab;

    // 現在画面に出現している敵オブジェクトを記憶しておく変数
    private GameObject currentEnemy;

    // 全開チェックした時のオルゴールの状態を記憶しておく変数
    private bool wasPlaying = false;

    void Update()
    {
        // オルゴールシステム参照がセットされていない場合は何もしない
        if (orgelSystem == null) return;

        // オルゴールの状態が「OFFからON」に切り替わった瞬間を検知
        if(orgelSystem.isPlaying && !wasPlaying)
        {
            Debug.Log("<color=green>【Spawner】オルゴールのONを検知！敵を出撃させます。</color>");
            SpawnEnemy(); // 敵を出現させる処理を呼ぶ
        }
        else if(!orgelSystem.isPlaying && wasPlaying)
        {
            Debug.Log("<color=blue>【Spawner】オルゴールのOFFを検知！敵を撤退させます。</color>");
            DestroyEnemy(); // 敵を消去する処理を呼ぶ
        }

        // 次のフレームのために、現在の状態を記憶しておく
        wasPlaying = orgelSystem.isPlaying;
    }

    /// <summary>
    /// 敵を出現させる処理
    /// </summary>
    private void SpawnEnemy()
    {
        // すでに敵がいる場合や、プレハブがセットされていない場合は何もしない
        if (currentEnemy != null || enemyPrefab == null) return;

        // プレハブから新しい敵オブジェクトを生成し、currentEnemyに記憶する
        currentEnemy = Instantiate(enemyPrefab, transform.position, Quaternion.identity);
    }

    /// <summary>
    /// 敵を消滅させる処理
    /// </summary>
    private void DestroyEnemy()
    {
        // 敵が存在している場合のみ処理を行う
        if(currentEnemy != null)
        {
            // 敵のオブジェクトを完全に削除する
            Destroy(currentEnemy);

            // 空にして、また新しい敵を生成できるようにする
            currentEnemy = null;
        }
    }
}
