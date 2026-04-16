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
    private CharacterController controller;

    private Vector3 startPosition; // 出現した最初の位置
    private Vector3 targetPosition; // 次に向かう目的地
    private Vector3 verticalVelocity;
    private Transform playerTransform; // プレイヤーの位置を記憶する変数

    // 敵の現在の「状態」を管理する列挙型(モード切替用)
    private enum State {Wander, Chase }
    private State currentState = State.Wander;

    private void Start()
    {
        controller = GetComponent<CharacterController>();
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

        // 重力の計算
        if (controller.isGrounded && verticalVelocity.y <0)
        {
            verticalVelocity.y = -2f;
        }
        verticalVelocity.y += -9.81f * Time.deltaTime;

        // 状態判定
        if(CheckSight())
        {
            // 見つけたら追跡モードに切り替え
            if(currentState != State.Chase)
            {
                currentState = State.Chase;
                Debug.Log("<color=red>【Enemy】プレイヤー発見！追跡開始！</color>");
            }
        }
        else if(currentState != State.Chase)
        {
            // 追跡中だが視界から外れた場合、距離が離れすぎたら諦める
            if(playerTransform != null)
            {
                float distance = Vector3.Distance(transform.position, playerTransform.position);
                // 視界の1.5倍の距離まで逃げ切られたら徘徊に戻る
                if (distance > enemyData.viewRadius * 1.5f)
                {
                    currentState = State.Wander;
                    SetNextTarget(); // 新しい徘徊ルートを設定
                    Debug.Log("<color=blue>【Enemy】プレイヤーを見失った。徘徊に戻る。</color>");
                }
            }
        }
        
        // 状況に応じた移動の実行
        if(currentState == State.Chase)
        {
            Chase();
        }
        else
        {
            Wander();
        }

            // 最終的な垂直移動の適用
            controller.Move(verticalVelocity * Time.deltaTime);
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
        // 目的地への方向を計算
        Vector3 direction = (targetPosition - transform.position);
        direction.y = 0; // 高さは無視

        if(direction.magnitude > 0.1f)
        {
            // 目的地を向く
            transform.forward = Vector3.Slerp(transform.forward, direction.normalized, Time.deltaTime * 5.0f);

            // 移動の実行
            Vector3 move = transform.forward * enemyData.moveSpeed * Time.deltaTime;
            controller.Move(move);
        }
        else
        {
            SetNextTarget();
        }
    }

    /// <summary>
    /// @brief プレイヤーを一直線に追いかける処理
    /// </summary>
    private void Chase()
    {
        if(playerTransform == null) return;

        // プレイヤーの方向を計算
        Vector3 direction = (playerTransform.position - transform.position);
        direction.y = 0;

        if( direction.magnitude > 0.1f)
        {
            // プレイヤーの方を素早く向く
            transform.forward = Vector3.Slerp(transform.forward, direction.normalized, Time.deltaTime * 10.0f);

            Vector3 move = transform.forward * (enemyData.moveSpeed * 1.5f) * Time.deltaTime;
            controller.Move(move);
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

    /// <summary>
    /// @brief キャラクターコントローラーによる衝突判定
    /// </summary>
    private void OnControllerColliderHit(ControllerColliderHit hit)
    {
        if (hit.gameObject.CompareTag("Player"))
        {
            Debug.Log("<color=red>【Enemy】捕まえたぞ！</color>");
            if (GameManager.instance != null)
            {
                GameManager.instance.GameOver("敵に捕まってしまった...");
            }
        }
    }
}
