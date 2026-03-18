using UnityEngine;

/// <summary>
/// 敵の移動を制御するクラス
/// EnemyDataのパラメーターを参照して動きます
/// </summary>
public class EnemyController : MonoBehaviour
{
    /// <summary>
    /// ここにEnemyDataファイルをセットします
    /// </summary>
    [Tooltip("敵のパラメータデータ")]
    public EnemyData enemyData;

    private Vector3 startPosition; // 出現した最初の位置
    private Vector3 targetPosition; // 次に向かう目的地

    private void Start()
    {
        // 出現した位置を記憶しておき、そこを中心に徘徊するようにします
        startPosition = transform.position;

        // 最初の目的地をランダムに設定
        SetNextTarget();
    }

    void Update()
    {
        // もしデータがセットされていなかったらエラーを防ぐために処理を中止
        if (enemyData == null) return;

        // 目的地に向かって移動する
        // Vector3.MoveTowardsは、「現在地」から「目的地」へ「指定した速度」で党則移動指せる便利な関数です
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, enemyData.moveSpeed * Time.deltaTime);

        // 目的地に到達したかを判定
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
            // 次のランダムな目的地を設定
            SetNextTarget();
        }
    }

    /// <summary>
    /// wanderRadius(徘徊半径)の範囲内で、次のランダムな目的地を決定する処理
    /// </summary>
    private void SetNextTarget()
    {
        if (enemyData == null) return;

        // X軸とZ軸で、マイナス半径～プラス半径の間でランダムな数値を出す
        float randomX = Random.Range(-enemyData.wanderRadius, enemyData.wanderRadius);
        float randomZ = Random.Range(-enemyData.wanderRadius, enemyData.wanderRadius);

        // 出現した位置を基準に、ランダムにずらした場所を次の目的地にする
        targetPosition = startPosition + new Vector3(randomX, 0.0f, randomZ);
    }
}
