using System.Collections;
using UnityEngine;
using UnityEngine.AI;




public class Enemy : MonoBehaviour
{
    //エネミーの状態を定義
    public enum State
    {
        walk,   //徘徊
        chase,  //追跡
        Action  //扉を開ける等の特殊行動中
    }

    [Header("現在の状態")]
    public State currentState = State.walk;

    [Header("各種設定")]
    public Transform player; //プレイヤーの位置
    public Transform[] waypoints; //徘徊ポイント
    public float visionRadius = 10f; //視界の半径
    [Range(0, 360)]
    public float visionAngle = 90f; //視界の角度
    
    private NavMeshAgent agent;
    private int currentWaypointIndex = 0;
    private float doorCheckCooldown = 0f; //ドアの判定をとるクールダウンタイマー

    //色を変える為のRenderer
    private Renderer enemyRenderer;

    [Header("捕獲設定")]
    [Tooltip("プレイヤーとの距離がこの値以下になったらゲームオーバー(捕獲)")]
    public float catchDistance = 1.5f;

    [Header("エフェクト・サウンド")]
    [Tooltip("常に足元に出る煙のエフェクト")]
    public ParticleSystem smokeEffect;
    [Tooltip("追跡時に火の粉が出るエフェクト")]
    public ParticleSystem fireEffect;
    [Tooltip("燃えるエフェクト")]
    public GameObject dissolveEffect;
    [Tooltip("追跡時に鳴らすSE")]
    public AudioSource chaseAudioSource;

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        //エネミーの見た目
        enemyRenderer = GetComponentInChildren<Renderer>();

        if (player == null)
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
            else
            {
                Debug.LogError("'Player'タグのオブジェクトがいません！");
            }
        }

        if (waypoints == null || waypoints.Length == 0)
        {
            // "Waypoint"タグを持つ全オブジェクトを取得
            GameObject[] wpObjs = GameObject.FindGameObjectsWithTag("Waypoint");

            if (wpObjs.Length > 0)
            {
                waypoints = new Transform[wpObjs.Length];
                for (int i = 0; i < wpObjs.Length; i++)
                {
                    waypoints[i] = wpObjs[i].transform;
                }
                Debug.Log($"[システム] Waypointを自動で {wpObjs.Length} 個取得しました。");
            }
            else
            {
                Debug.LogWarning("[システム] Waypointが一つも設定されておらず、タグからも見つかりませんでした。");
            }
        }

        //最初の目的地を設定
        if (waypoints != null && waypoints.Length > 0)
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }

        //ゲーム開始時煙だしっぱ
        if(smokeEffect != null) smokeEffect.Play();
        //火の粉は止める
        if(fireEffect != null) fireEffect.Stop();
    }

    void Update()
    {
        //ゲームオーバー状態なら最優先でエネミーの動きを止める処理
        if (GameManager.Instance != null && GameManager.Instance.IsGameOver)
        {
            if (agent != null && !agent.isStopped)
            {
                agent.isStopped = true;        //navigationの停止
                agent.velocity = Vector3.zero; //慣性の滑りをゼロにする
            }

            //扉を開けるコルーチン(OpendoorAction)が途中で動き続けないように強制終了
            StopAllCoroutines();

            return; //これ以降の追跡や捕獲判定を、タイマー処理をスキップ
        }
        
        CheckCatchPlayer();

        //ドアの判定をとるタイマーを減らす
        if (doorCheckCooldown > 0)
        {
            doorCheckCooldown -= Time.deltaTime;
        }

        if (Input.GetKeyDown(KeyCode.Space))
        {
            if (player != null)
            {
                float dist = Vector3.Distance(transform.position, player.position);
                Debug.Log($"【強制調査】現在ターゲットにしている物: {player.name} / 距離: {dist:F1}m / 現在の状態: {currentState}");
            }
            else
            {
                Debug.LogError("【強制調査】ターゲット(player)が空っぽ（null）です！");
            }
        }

        //状態に応じた行動を実行
        switch (currentState)
        {
            case State.walk:
                //エフェクト火の粉を止める
                if (fireEffect != null && fireEffect.isPlaying) fireEffect.Stop();
                if (chaseAudioSource != null && chaseAudioSource.isPlaying) chaseAudioSource.Stop();

                //agent.speed = 2.0f; //徘徊時の速度を設定
                SetColor(Color.green);//徘徊時は緑色
                PatrolRoutine();
                CheckVision();
                CheckDoorOnRight();
                break;
            case State.chase:
                //エフェクト火の粉と音を出す
                if (fireEffect != null && !fireEffect.isPlaying) fireEffect.Play();
                if (chaseAudioSource != null && !chaseAudioSource.isPlaying) chaseAudioSource.Play();
                if (dissolveEffect != null && !dissolveEffect.activeSelf) dissolveEffect.SetActive(true);

                //agent.speed = 4.0f; //追跡時の速度を設定
                SetColor(Color.red);//追跡時は赤色
                ChaseRoutine();
                CheckVision();
                break;
            case State.Action:
                //エフェクト火の粉と音を止める
                if (fireEffect != null && fireEffect.isPlaying) fireEffect.Stop();
                if (chaseAudioSource != null && chaseAudioSource.isPlaying) chaseAudioSource.Stop();
                if (dissolveEffect != null && dissolveEffect.activeSelf) dissolveEffect.SetActive(false);

                SetColor(Color.yellow);//扉を開ける等の特殊行動中は黄色
                //コルーチンで処理中なので、何もしない
                break;
        }
    }

    //色変更
    private void SetColor(Color color)
    {
        if(enemyRenderer != null)
        {
            //マテリアルの色を直接書き換える
            enemyRenderer.material.color = color;
        }
    }
    //1.徘徊(walk)の処理
    private void PatrolRoutine()
    {
        //目的地に到達したら次のポイントへ
        if (!agent.pathPending && agent.remainingDistance < 0.5f)
        {
            currentWaypointIndex = (currentWaypointIndex + 1) % waypoints.Length;
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    //2.追跡(chase)の処理
    private void ChaseRoutine()
    {
        //プレイヤーの位置を目的地に設定
        agent.SetDestination(player.position);
    }

    //3.視界のチェック判定
    private void CheckVision()
    {
        if (player == null)
        {
            Debug.LogWarning("[索敵エラー] player変数が設定されていません！");
            return;
        }

        //プレイヤーとの距離を測定
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //距離が視界の半径内か
        if (distanceToPlayer <= visionRadius)
        {
            Vector3 rayOrigin = transform.position + Vector3.up * 1.0f + transform.forward * 0.5f;
            Vector3 targetPos = player.position + Vector3.up * 1.0f; // プレイヤーの胸の高さ

            //プレイヤーへの方向ベクトルを計算
            Vector3 dirToPlayer = (targetPos - rayOrigin).normalized;
            //自分の正面とプレイヤーへの方向の角度を計算
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            //角度が視界の半分以内か
            if (angle <= visionAngle / 2f)
            {
                //壁越しに見えないようにレイキャストで確認
                RaycastHit hit;
                if (Physics.Raycast(rayOrigin, dirToPlayer, out hit, visionRadius))
                {
                    Debug.Log($"[索敵デバッグ] Rayが当たった物: {hit.collider.gameObject.name} (Tag: {hit.collider.tag})");

                    if (hit.collider.CompareTag("Player"))
                    {
                        //プレイヤーを見つけたら追跡状態に遷移
                        if (currentState != State.chase)
                        {
                            Debug.Log("プレイヤーを発見！追跡開始！");
                            currentState = State.chase;
                        }
                        return; //プレイヤーを見つけたら終了

                    }
                }
            }
        }
        //プレイヤーが見えない場合は徘徊状態に戻る
        if (currentState == State.chase && distanceToPlayer > visionRadius)
        {
            Debug.Log($"[距離デバッグ] 距離が{distanceToPlayer:F1}mのため、プレイヤーを見失った。徘徊に戻る。");
            Debug.Log("プレイヤーを見失った。徘徊に戻る。");
            currentState = State.walk;
            //目指していた徘徊ポイントへ戻る
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    //4.右側に扉があるかのチェック
    private void CheckDoorOnRight()
    {
        //☆ドアの判定をとるクールダウン中はチェックしない
        if (doorCheckCooldown > 0)
        {
            return;
        }
        //右側にレイを飛ばして扉の確認
        RaycastHit hit;
        Vector3 rightDir = transform.right; //右方向

        if (Physics.Raycast(transform.position, rightDir, out hit, 2f))
        {
            //当たった物がDoorタグのオブジェクトか
            if (hit.collider.CompareTag("Door"))
            {
                //☆次にドアの判定をとるまでのクールダウンを設定（例: 5秒）
                doorCheckCooldown = 5.0f;
                //扉を開けるアクションを開始
                StartCoroutine(OpenDoorAction(hit.collider.gameObject));
            }
        }
    }

    //5.扉を開けるアクションのコルーチン
    private IEnumerator OpenDoorAction(GameObject door)
    {
        //DoorSystemコンポーネントの取得
        DoorSystem doorSystem = door.GetComponent<DoorSystem>();
        if (doorSystem == null)
        {
            //親オブジェクトにDoorSystemがある場合の考慮
            doorSystem = door.GetComponentInParent<DoorSystem>();
        }
        //DoorSystemが見つかったか
        if (doorSystem == null)
        {
            Debug.LogError($" [エラー] {door.name}からDoorSystemコンポーネントが見つかりませんでした。");
        }
        else
        {
            Debug.Log($"[成功] {door.name}のDoorSystemを取得しました。");
        }
        //DoorSystemが既に開いている状態ならスルーして進む
        if (doorSystem != null && doorSystem.IsOpen)
        {
            yield break; //コルーチンを終了して、徘徊や追跡に戻る
        }

        //状態をActionに変更して、他の行動を停止
        currentState = State.Action;

        //NavMeshAgent移動を停止
        agent.isStopped = true;

        ////扉として認識させないためタグを消す
        //door.tag = "Untagged";

        Debug.Log("扉を開けるアクション開始");
        //ここで扉を開けるアニメーションや処理を実装
        //例: door.GetComponent<Door>().Open();

        //ドアが閉まっている場合は、DoorSystemのExecuteInteractionを呼び出して開ける
        if (doorSystem != null)
        {
            doorSystem.ExecuteInteraction();
        }

        //仮に2秒間のアクションとする
        yield return new WaitForSeconds(2f);

        ////エネミーが物理的に引っかからないよう、当たり判定も消す
        //Collider doorCollider = door.GetComponent<Collider>();
        //if (doorCollider != null)
        //{
        //    doorCollider.enabled = false;
        //}

        Debug.Log("中に入り見回す");
        //ここで指定位置に移動したり、首を振るアニメーションや処理を実装
        //例: agent.SetDestination(door.transform.position + door.transform.forward * 2f);

        //仮にさらに2秒間のアクションとする
        yield return new WaitForSeconds(2f);

        Debug.Log("元のルートに戻る");
        //移動を再開して、元のウェイポイントに目的地を再設定
        agent.isStopped = false;
        agent.SetDestination(waypoints[currentWaypointIndex].position);

        //状態をwalkに戻す
        currentState = State.walk;
    }

    private void OnDrawGizmos()
    {
        //敵が存在しないときが描画しない
        if (transform == null) return;

        //視界の半径を赤いワイヤーで表示
        Gizmos.color = new Color(1, 0, 0, 0.5f); //半透明の赤

        //正面の線
        Gizmos.DrawLine(transform.position, transform.position + transform.forward * visionRadius);
        //左視界端の方向を計算して線を引く
        Vector3 leftDir = Quaternion.Euler(0, -visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + leftDir * visionRadius);
        //右視界端の方向を計算して線を引く
        Vector3 rightDir = Quaternion.Euler(0, visionAngle / 2f, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + rightDir * visionRadius);

        //弧を描画
        int segments = 20; //弧の分割数
        for (int i = 0; i <= segments; i++)
        {
            float angle_a = -visionAngle / 2f + (visionAngle / segments) * i;
            float angle_b = -visionAngle / 2f + (visionAngle / segments) * (i + 1);
            Vector3 dir_a = Quaternion.Euler(0, angle_a, 0) * transform.forward;
            Vector3 dir_b = Quaternion.Euler(0, angle_b, 0) * transform.forward;
            Gizmos.DrawLine(transform.position + dir_a * visionRadius, transform.position + dir_b * visionRadius);
        }
    }

    /// <summary>
    /// @brief 他の物理コライダーと接触した際に呼ばれる処理
    /// @param collision 衝突した相手のオブジェクト情報
    /// @details プレイヤーに触れた瞬間、GameManagerにゲームオーバーを指示します。
    /// </summary>
    private void OnCollisionEnter(Collision collision)
    {
        // ぶつかった相手のタグが "Player" かどうかを確認
        if (collision.gameObject.CompareTag("Player"))
        {
            Debug.Log("【Enemy】プレイヤーを捕獲しました！(Collision)");

            // GameManagerが存在するか安全確認してから、ゲームオーバー処理を呼び出す
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MaxOutFearAndGameOver();
            }
        }
    }

    /// <summary>
    /// @brief 他のトリガーコライダー（すり抜ける判定）と接触した際に呼ばれる処理
    /// @param other 侵入してきた相手のコライダー情報
    /// @details エネミーの判定が「Is Trigger」の場合や、プレイヤーがトリガーの場合に備えた予備処理です。
    /// </summary>
    private void OnTriggerEnter(Collider other)
    {
        // 侵入した相手のタグが "Player" かどうかを確認
        if (other.CompareTag("Player"))
        {
            Debug.Log("【Enemy】プレイヤーを捕獲しました！(Trigger)");

            // GameManagerが存在するか安全確認してから、ゲームオーバー処理を呼び出す
            if (GameManager.Instance != null)
            {
                GameManager.Instance.MaxOutFearAndGameOver();
            }
        }
    }

    /// <summary>
    /// @brief プレイヤーとの距離を計算し、捕獲距離以内に入ったらゲームオーバーにする処理
    /// @details Updateメソッド内で毎フレーム呼ばれます。物理演算に依存しない確実な判定です。
    /// </summary>
    private void CheckCatchPlayer()
    {
        // プレイヤーとGameManagerが両方存在している場合のみ実行
        if (player != null && GameManager.Instance != null)
        {
            // 敵とプレイヤーの間の直線距離を計算
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // 距離が catchDistance (例: 1.5m) 以下に近づいた場合
            if (distanceToPlayer <= catchDistance)
            {
                Debug.Log($"【Enemy】プレイヤーとの距離が {distanceToPlayer:F1}m になったため、捕獲しました！");

                // GameManagerのゲームオーバー処理を呼び出す
                GameManager.Instance.MaxOutFearAndGameOver();
            }
        }
    }
}
