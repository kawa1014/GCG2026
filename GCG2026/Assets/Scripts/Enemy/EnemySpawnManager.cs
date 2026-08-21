using System.Collections;
using UnityEngine;

public class EnemySpawnManager : MonoBehaviour
{
    //どこからでもEnemySpawnManagerにアクセスできるようにするためのシングルトンインスタンス
    public static EnemySpawnManager Instance { get; private set; }

    [Header("スポーンするエネミーのプレハブ")]
    public GameObject enemyPrefab;

    [Header("ゲーム開始からの経過時間（確認用）")]
    [SerializeField] private float gameTimer = 0f;

    [Header("同期出来てるかの確認用いじらないように")]
    [SerializeField] private float totalTime = 180f;

    [Header("制限時間半分～終了までの出現確率カーブ")]
    [Tooltip("横軸：時間の進捗(0.5 = 半分, 1.0 = 終了) / 縦軸：出現確率(0.5 = 50%, 1.0 = 100%)")]
    [SerializeField]
    private AnimationCurve secondHalfCurve = new AnimationCurve(
        new Keyframe(0.5f, 0.5f), //横軸0.5,縦軸0.5 (時間の半分で50%の確率)
        new Keyframe(1.0f, 1.0f)  //横軸1.0,縦軸1.0 (時間の終了で100%の確率)
    );

    //================================
    [Header("デバック用")]
    public bool Spawn100 = false;
    //================================

    private GameObject currentEnemy = null; //現在スポーンしているエネミーの参照
    private Camera playerCamera; //プレイヤーのカメラの参照
    private Transform playerTransform; //プレイヤーの位置の参照

    void Awake()
    {
        //シングルトンの設定
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }
    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        //GameManagerから制限時間の初期値を取得
        if(GameManager.Instance != null)
        {
            totalTime = GameManager.Instance.TimeLimit;
        }

        //プレイヤーとカメラを自動取得
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
        if (playerObj != null)
        {
            playerTransform = playerObj.transform;
            playerCamera = playerObj.GetComponentInChildren<Camera>();

            if (playerCamera == null) playerCamera = Camera.main; //プレイヤーの子オブジェクトにカメラがない場合は、シーンのメインカメラを使用
        }
    }

    // Update is called once per frame
    void Update()
    {
        //ゲーム開始からの経過時間を更新
        gameTimer += Time.deltaTime;
    }

    //センサーから呼び出される、エネミーをスポーンさせるための関数
    public void TrySpawnEnemy(Transform[] spawnPoints)
    {
        //すでにエネミーがスポーンしている場合は、何もしない
        if (currentEnemy != null) return;

        float spawnProbability = 0f;

        //GameManagerから取得したtotalTimeを使って現在の進行度(0.0~1.0)を計算
        float progress = Mathf.Clamp01(gameTimer / totalTime);

        if (progress < 0.5f)
        {
            //前半（制限時間の半分まで）は出現確率0%
            spawnProbability = 0f;
        }
        else
        {
            //後半（制限時間の半分以降）は、インスペクターのAnimationCurveを使って出現確率を取得
            spawnProbability = secondHalfCurve.Evaluate(progress);
        }

        if (Spawn100)
        {
            spawnProbability = 1.0f;
        }

        //確率の抽選(0.0~0.1のランダムな値)
        if (Random.value <= spawnProbability)
        {
            //渡された候補地の中からランダムに1つ選ぶ
            Transform selectedPoint = spawnPoints[Random.Range(0, spawnPoints.Length)];

            //敵を出現させ、currentEnemyに登録
            currentEnemy = Instantiate(enemyPrefab, selectedPoint.position, selectedPoint.rotation);
            Debug.Log($"[出現成功] 現在の確率: {spawnProbability * 100}% / 経過時間: {gameTimer:F1}秒");

            //消滅チェックを行うコルーチン（裏処理）をスタート
            StartCoroutine(DespawnCheckRoutine(currentEnemy));
        }
        else
        {
            Debug.Log($"[出現失敗] (現在の確率: {spawnProbability * 100}%)");
        }
    }

    //[消滅ロジック] 時間経過とプレイヤーの視界を監視するコルーチン
    //private IEnumerator DespawnCheckRoutine(GameObject enemy)
    //{
    //    //10秒間は確実にいるようにする
    //    yield return new WaitForSeconds(10f);

    //    //敵が存在している間、1秒ごとにループチェック
    //    while (enemy != null)
    //    {
    //        yield return new WaitForSeconds(1f);

    //        //プレイヤーの視界外&敵と同じ部屋(エリア)にいない場合
    //        if (IsEnemyOutOfPlayerVision(enemy) && IsNotInSameArea(enemy))
    //        {
    //            //1秒ごとに50%の確率で消滅の抽選
    //            if (Random.value <= 0.5f)
    //            {
    //                Debug.Log("[消滅] プレイヤーの視界外&別エリアのため、敵が消滅しました。");
    //                Destroy(enemy);
    //                currentEnemy = null;//参照をクリアして、次の出現を可能にする
    //                yield break; //コルーチン終了
    //            }
    //        }
    //    }
    //}
    private IEnumerator DespawnCheckRoutine(GameObject enemy)
    {
        // Manager側からEnemyスクリプトの状態を読み取るために取得しておく
        Enemy enemyScript = enemy.GetComponent<Enemy>();

        float hiddenTimer = 0f;       // 見失っている＆見られていない時間を測るタイマー
        float timeToDespawn = 10f;    // 消滅するまでの時間（10秒）

        // 敵が存在している間はずっと監視を続ける
        while (enemy != null)
        {
            // ① プレイヤーのカメラに映っていないか？（先ほど高精度化した関数）
            bool isOutOfVision = IsEnemyOutOfPlayerVision(enemy);

            // ② エネミーがプレイヤーを追跡していないか？（見失って徘徊に戻っているか）
            bool isNotChasing = (enemyScript.currentState != Enemy.State.chase);

            // 【判定】「カメラに映っていない」かつ「追跡していない」時だけタイマーを進める
            if (isOutOfVision && isNotChasing)
            {
                hiddenTimer += 1.0f; // 1秒進める

                // 10秒経過したら消滅！
                if (hiddenTimer >= timeToDespawn)
                {
                    Debug.Log("【システム】プレイヤーを見失って10秒経過し、視界外のためエネミーを消滅させます。");
                    Destroy(enemy);
                    yield break; // コルーチンを終了
                }
            }
            else
            {
                // もし「カメラで見られている」か「絶賛追跡中」なら、タイマーをリセットする
                hiddenTimer = 0f;
            }

            // 毎フレーム判定すると重いので、1秒ごとにチェックを繰り返す
            yield return new WaitForSeconds(1.0f);
        }
    }
    //プレイヤーのカメラに写っているかを高精度で判定する関数
    //private bool IsEnemyOutOfPlayerVision(GameObject enemy)
    //{
    //    if (playerCamera == null || enemy == null) return true;

    //    //敵の座標を、画面上の比率座標(0~1の範囲)に変換
    //    Vector3 screenPoint = playerCamera.WorldToViewportPoint(enemy.transform.position);

    //    //カメラの画角(画面内)に入っているかを判定
    //    bool inScreen = screenPoint.x >= 0 && screenPoint.x <= 1 &&
    //                    screenPoint.y >= 0 && screenPoint.y <= 1 &&
    //                    screenPoint.z > 0;

    //    if (inScreen)
    //    {
    //        //画面内であっても「壁」に遮られているかをRaycastで判定
    //        Vector3 direction = (enemy.transform.position - playerCamera.transform.position).normalized;
    //        float destance = Vector3.Distance(playerCamera.transform.position, enemy.transform.position);

    //        RaycastHit hit;
    //        if (Physics.Raycast(playerCamera.transform.position, direction, out hit, destance))
    //        {
    //            //何か壁に当たったら、プレイヤーからは見えていないと判断
    //            if (hit.collider.gameObject != enemy && !hit.collider.gameObject.transform.IsChildOf(enemy.transform))
    //            {
    //                return true; //敵は視界外（壁に遮られている）
    //            }
    //        }
    //        return false; //敵は視界内（壁に遮られていない）
    //    }

    //    return true; //そもそも画面の外を向いている場合は、視界外と判断

    //}

    private bool IsEnemyOutOfPlayerVision(GameObject enemy)
    {
        if (playerCamera == null || enemy == null) return true;

        //敵の座標を、画面上の比率座標(0~1の範囲)に変換
        Vector3 screenPoint = playerCamera.WorldToViewportPoint(enemy.transform.position);

        //カメラの画角(画面内)に入っているかを判定
        bool inScreen = screenPoint.x >= 0 && screenPoint.x <= 1 &&
                        screenPoint.y >= 0 && screenPoint.y <= 1 &&
                        screenPoint.z > 0;

        if (inScreen)
        {
            //敵の足元ではなく胸の高さ（+1.0m）に向かってRayを飛ばす
            Vector3 targetPos = enemy.transform.position + Vector3.up * 1.0f;
            Vector3 direction = (targetPos - playerCamera.transform.position).normalized;
            float distance = Vector3.Distance(playerCamera.transform.position, targetPos);

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, direction, out hit, distance))
            {
                //敵ではなくプレイヤー自身でもない物に当たったら「壁」と判定
                if (hit.collider.gameObject != enemy &&
                    !hit.collider.gameObject.transform.IsChildOf(enemy.transform) &&
                    !hit.collider.CompareTag("Player")) //プレイヤー自身の体は無視する
                {
                    return true; //壁などの障害に隠れている
                }
            }
            return false; //画面内でかつ障害物もない、見えている
        }
        return true; //画面外
    }

    //「同じ部屋にいない」を簡易的に直線距離(例:15m以上離れているか)で判定する関数
    //※もし「部屋ごとの管理スクリプト」を作る場合は、ここをその判定に置き換える
    private bool IsNotInSameArea(GameObject enemy)
    {
        if (playerTransform == null || enemy == null) return true;

        //15m以上離れている場合は、同じ部屋にいないと判断
        float distance = Vector3.Distance(playerTransform.position, enemy.transform.position);
        return distance >= 15f;
    }
}
