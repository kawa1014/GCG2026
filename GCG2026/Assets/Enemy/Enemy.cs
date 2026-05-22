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

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();

        //最初の目的地を設定
        if ((waypoints.Length > 0))
        {
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    void Update()
    {
        //状態に応じた行動を実行
        switch (currentState)
        {
            case State.walk:
                PatrolRoutine();
                CheckVision();
                CheckDoorOnRight();
                break;
            case State.chase:
                ChaseRoutine();
                CheckVision();
                break;
            case State.Action:
                //コルーチンで処理中なので、何もしない
                break;
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
        //プレイヤーとの距離を測定
        float distanceToPlayer = Vector3.Distance(transform.position, player.position);

        //距離が視界の半径内か
        if (distanceToPlayer <= visionRadius)
        {
            //プレイヤーへの方向ベクトルを計算
            Vector3 dirToPlayer = (player.position - transform.position).normalized;
            //自分の正面とプレイヤーへの方向の角度を計算
            float angle = Vector3.Angle(transform.forward, dirToPlayer);
            //角度が視界の半分以内か
            if (angle <= visionAngle / 2f)
            {
                //壁越しに見えないようにレイキャストで確認
                RaycastHit hit;
                if (Physics.Raycast(transform.position, dirToPlayer, out hit, visionRadius))
                {
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
            Debug.Log("プレイヤーを見失った。徘徊に戻る。");
            currentState = State.walk;
            //目指していた徘徊ポイントへ戻る
            agent.SetDestination(waypoints[currentWaypointIndex].position);
        }
    }

    //4.右側に扉があるかのチェック
    private void CheckDoorOnRight()
    {
        //右側にレイを飛ばして扉の確認
        RaycastHit hit;
        Vector3 rightDir = transform.right; //右方向

        if (Physics.Raycast(transform.position, rightDir, out hit, 2f))
        {
            //当たった物がDoorタグのオブジェクトか
            if (hit.collider.CompareTag("Door"))
            {
                //扉を開けるアクションを開始
                StartCoroutine(OpenDoorAction(hit.collider.gameObject));
            }
        }
    }

    //5.扉を開けるアクションのコルーチン
    private IEnumerator OpenDoorAction(GameObject door)
    {
        //状態をActionに変更して、他の行動を停止
        currentState = State.Action;

        //NavMeshAgent移動を停止
        agent.isStopped = true;

        Debug.Log("扉を開けるアクション開始");
        //ここで扉を開けるアニメーションや処理を実装
        //例: door.GetComponent<Door>().Open();

        //仮に2秒間のアクションとする
        yield return new WaitForSeconds(2f);

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
}
