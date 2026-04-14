using UnityEngine;

/// <summary>
/// @file EnemySpawner.cs
/// @brief 配置された場所に敵を出現させるクラス
/// @details ゲーム開始時に、このオブジェクトが置かれている位置に敵を生成します
/// </summary>
public class EnemySpawner : MonoBehaviour
{
    [Header("設定")]
    [Tooltip("出現させる敵のプレハブ")]
    public GameObject enemyPrefab;

    /// <summary>
    /// ゲーム開始時に1回だけ呼ばれる
    /// </summary>
    private void Start()
    {
        SpawnEnemy();
    }

    /// <summary>
    /// 敵を出現させる処理
    /// </summary>
    private void SpawnEnemy()
    {
        // プレハブがセットされているか確認
        if(enemyPrefab != null)
        {
            // このSpawner自信の位置と回転で敵を作成
            Instantiate(enemyPrefab, transform.position, transform.rotation);
            Debug.Log($"<color=green>【Spawner】{gameObject.name} の位置に敵を出現させました。</color>");
        }
        else
        {
            Debug.LogWarning("【Spawner】敵のプレハブがインスペクターにセットされていません！");
        }
    }
}