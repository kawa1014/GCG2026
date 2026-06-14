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

        //[ルール]全体を5分としたとき、1分半時点で確率Up
        //最初は50%,3分半以降は75%の確率でスポーンさせる
        float spawnProbability = (gameTimer >= 90f) ? 0.75f : 0.5f;

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
    private IEnumerator DespawnCheckRoutine(GameObject enemy)
    {
        //10秒間は確実にいるようにする
        yield return new WaitForSeconds(10f);

        //敵が存在している間、1秒ごとにループチェック
        while (enemy != null)
        {
            yield return new WaitForSeconds(1f);

            //プレイヤーの視界外&敵と同じ部屋(エリア)にいない場合
            if (IsEnemyOutOfPlayerVision(enemy) && IsNotInSameArea(enemy))
            {
                //1秒ごとに50%の確率で消滅の抽選
                if (Random.value <= 0.5f)
                {
                    Debug.Log("[消滅] プレイヤーの視界外&別エリアのため、敵が消滅しました。");
                    Destroy(enemy);
                    currentEnemy = null;//参照をクリアして、次の出現を可能にする
                    yield break; //コルーチン終了
                }
            }
        }
    }

    //プレイヤーのカメラに写っているかを高精度で判定する関数
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
            //画面内であっても「壁」に遮られているかをRaycastで判定
            Vector3 direction = (enemy.transform.position - playerCamera.transform.position).normalized;
            float destance = Vector3.Distance(playerCamera.transform.position, enemy.transform.position);

            RaycastHit hit;
            if (Physics.Raycast(playerCamera.transform.position, direction, out hit, destance))
            {
                //何か壁に当たったら、プレイヤーからは見えていないと判断
                if (hit.collider.gameObject != enemy && !hit.collider.gameObject.transform.IsChildOf(enemy.transform))
                {
                    return true; //敵は視界外（壁に遮られている）
                }
            }
            return false; //敵は視界内（壁に遮られていない）
        }

        return true; //そもそも画面の外を向いている場合は、視界外と判断

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
