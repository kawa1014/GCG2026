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
    private Transform playerTransform; // プレイヤーの位置を記憶する変数

    private void Start()
    {
        // 出現した位置を記憶しておき、そこを中心に徘徊するようにします
        startPosition = transform.position;

        // 最初の目的地をランダムに設定
        SetNextTarget();

        // シーン内からプレイヤーを探し出して記憶する
        PlayerController player = FindAnyObjectByType<PlayerController>();
        if(player != null)
        {
            playerTransform = player.transform;
        }
    }

    void Update()
    {
        // もしデータがセットされていなかったらエラーを防ぐために処理を中止
        if (enemyData == null) return;

        // プレイヤーが視界にいるかチェックする
        bool isPlayerInSight = CheckSight();

        if (isPlayerInSight)
        {
            // 視界に入った
            Debug.Log("<color=red>【Enemy】プレイヤーを発見！！</color>");

            // プレイヤーの方を向く
            Vector3 lookTarget = new Vector3(playerTransform.position.x, transform.position.y, playerTransform.position.z);
            transform.LookAt(lookTarget);

            // プレイヤーに向かって移動する
            transform.position = Vector3.MoveTowards(transform.position, lookTarget, enemyData.moveSpeed * Time.deltaTime);
        }
        else
        {
            // 視界にいない場合は、今まで通り徘徊する
            Wander();
        }
    }

    /// <summary>
    /// プレイヤーが扇形の視界に入っているか判定する処理
    /// </summary>
    private bool CheckSight()
    {
        // プレイヤーがセットされていなければ見えていないことにする
        if(playerTransform == null) return false;

        // プレイヤーまでの距離を測る
        float distanceToplayer = Vector3.Distance(transform.position, playerTransform.position);

        // 視界の距離より遠ければ、見えていない
        if (distanceToplayer > enemyData.viewRadius) return false;

        // プレイヤーのいる方向を計算する
        Vector3 dirToPlayer = (playerTransform.position - transform.position).normalized;

        // 自分の正面方向と、プレイヤーの方向との角度を測る
        float angleToPlayer = Vector3.Angle(transform.forward, dirToPlayer);

        // 角度が視界の角度の半分より小さければ、扇形の中に入っていると判定
        if (angleToPlayer < enemyData.viewAngle / 2.0f)
        {
            return true;
        }

        return false;
    }

    /// <summary>
    /// 徘徊処理
    /// </summary>
    private void Wander()
    {
        transform.position = Vector3.MoveTowards(transform.position, targetPosition, enemyData.moveSpeed * Time.deltaTime);
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f)
        {
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
