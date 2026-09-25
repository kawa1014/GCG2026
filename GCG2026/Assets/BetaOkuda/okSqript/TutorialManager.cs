using UnityEngine;
using UnityEngine.InputSystem;
using System;

public enum TutorialState
{
    WaitCameraMove,   // 視点移動の入力待ち
    WaitPlayerMove,   // WASD移動の入力待ち
    WaitOrgelSound,   // オルゴールが鳴るまで待機
    ForcedLook,       // オルゴールへ強制的に視点を向ける状態（行き）
    KeepLooking,      // 【追加】オルゴールを向いたまま待機する状態（停止）
    ReturnLook,       // 元の視点に戻る状態（帰り）
    WaitListening,    // 聞き耳(Eキー)の入力待ち
    Completed         // チュートリアル完了
}

public class TutorialManager : MonoBehaviour
{
    public event Action OnTutorialStart;
    public event Action OnCameraMoveClear;
    public event Action OnPlayerMoveClear;
    public event Action OnTutorialComplete;

    [Header("プレイヤー参照")]
    [SerializeField] private PlayerController playerController;
    [SerializeField] private Camera playerCamera;

    [Header("クリア条件の設定")]
    [SerializeField] private float requiredCameraMoveAmount = 300f;
    [SerializeField] private float requiredPlayerMoveTime = 2.0f;

    [Header("視線誘導 (LookAt) 設定")]
    [Tooltip("オルゴールへ振り向くまでの時間（秒）")]
    [SerializeField] private float lookAtDuration = 0.5f;
    [Tooltip("オルゴールを向いたまま止まる時間（秒）")] // 【追加】
    [SerializeField] private float returnDuration = 0.8f;

    [Header("チュートリアルUI")]
    [SerializeField]
    private TutorialCaller tutorialCaller;
    private TutorialState currentState = TutorialState.WaitCameraMove;

    private float accumulatedCameraMove = 0f;
    private float accumulatedMoveTime = 0f;
    private float lookAtTimer = 0f;
    private bool firstOrgelLookStarted = false;
    [Header("フェーズ7：オルゴール接近判定")]
    [SerializeField]
    private float phase7Distance = 8f;
    private bool phase7DistanceTriggered = false;

    private Quaternion originalCameraRot;
    private Quaternion originalPlayerRot;
    private Quaternion targetCameraRot;
    private Quaternion targetPlayerRot;
    private Quaternion returnStartPlayerRot;
    private Quaternion returnStartCameraRot;

    private void Start()
    {
        if (playerController != null)
        {
            playerController._isStop = false;
            playerController.SetCanMove(false);
        }
        OnTutorialStart?.Invoke();
    }

    private void Update()
    {
        CheckPhase7Distance();
        switch (currentState)
        {
            case TutorialState.WaitCameraMove:
                CheckCameraMovement();
                break;

            case TutorialState.WaitPlayerMove:
                CheckPlayerMovement();
                break;

            case TutorialState.WaitOrgelSound:
                CheckOrgelSound();
                break;
            case TutorialState.ForcedLook:
                HandleForcedLook(); // 行き
                break;

            case TutorialState.KeepLooking:
                HandleKeepLooking(); // 【追加】停止
                break;

            case TutorialState.ReturnLook:
                HandleReturnLook(); // 帰り
                break;

            case TutorialState.WaitListening:
                CheckListening();
                break;
        }
    }

    private void CheckCameraMovement()
    {
        if (Mouse.current == null) return;
        Vector2 delta = Mouse.current.delta.ReadValue();
        accumulatedCameraMove += Mathf.Abs(delta.x) + Mathf.Abs(delta.y);

        if (accumulatedCameraMove >= requiredCameraMoveAmount)
        {
            currentState = TutorialState.WaitPlayerMove;
            if (playerController != null) playerController.SetCanMove(true);
            OnCameraMoveClear?.Invoke();
        }
    }

    private void CheckPlayerMovement()
    {
        if (Keyboard.current == null) return;
        if (Keyboard.current.wKey.isPressed || Keyboard.current.aKey.isPressed ||
            Keyboard.current.sKey.isPressed || Keyboard.current.dKey.isPressed)
        {
            accumulatedMoveTime += Time.deltaTime;
        }

        if (accumulatedMoveTime >= requiredPlayerMoveTime)
        {
            currentState = TutorialState.WaitOrgelSound;
            OnPlayerMoveClear?.Invoke();
        }
    }

    private void StartForcedLook(OrgelSystem targetOrgel)
    {
        if (targetOrgel == null || playerCamera == null || playerController == null)
        {
            // 参照がない場合は視線誘導せず待ち続ける
            currentState = TutorialState.WaitOrgelSound;
            return;
        }

        currentState = TutorialState.ForcedLook;
        // 視線誘導中だけプレイヤー操作を止める
        playerController._isStop = true;
        lookAtTimer = 0f;

        if (tutorialCaller != null)
        {
            tutorialCaller.ShowPhase2();
        }

        // 元の向きを保存
        originalPlayerRot = playerController.transform.rotation;
        originalCameraRot = playerCamera.transform.localRotation;

        Vector3 directionToOrgel = targetOrgel.transform.position - playerCamera.transform.position;

        // プレイヤー本体は横方向だけ回転
        Vector3 flatDirection = new Vector3(directionToOrgel.x, 0f, directionToOrgel.z).normalized;

        if (flatDirection.sqrMagnitude > 0.001f)
        {
            targetPlayerRot = Quaternion.LookRotation(flatDirection);
        }
        else
        {
            targetPlayerRot = originalPlayerRot;
        }

        // カメラは上下方向だけ回転
        Vector3 localDirection = playerController.transform.InverseTransformDirection(directionToOrgel);

        float horizontalDistance = new Vector2(localDirection.x, localDirection.z).magnitude;

        float pitchAngle = Mathf.Atan2(-localDirection.y, horizontalDistance) * Mathf.Rad2Deg;

        pitchAngle = Mathf.Clamp(pitchAngle, -80f, 80f);

        targetCameraRot = Quaternion.Euler(pitchAngle, 0f, 0f);
    }

    private void HandleForcedLook()
    {
        lookAtTimer += Time.deltaTime;
        float t = Mathf.Clamp01(lookAtTimer / lookAtDuration);
        float easeT = t * t * (3f - 2f * t);

        playerController.transform.rotation = Quaternion.Slerp(originalPlayerRot, targetPlayerRot, easeT);
        playerCamera.transform.localRotation = Quaternion.Slerp(originalCameraRot, targetCameraRot, easeT);

        // 振り向き終わったら、次は「待機状態」へ
        if (t >= 1.0f)
        {
            currentState = TutorialState.KeepLooking; // 【変更】直接戻らずに待機ステートへ
            lookAtTimer = 0f; // 待機時間の計測用にタイマーをリセット
        }
    }

    // --- 【追加】向いたまま停止する処理 ---
    private void HandleKeepLooking()
    {
       
    }

    private void HandleReturnLook()
    {
        lookAtTimer += Time.deltaTime;
        float t = Mathf.Clamp01(lookAtTimer / returnDuration);
        float easeT = t * t * (3f - 2f * t);

        playerController.transform.rotation = Quaternion.Slerp(targetPlayerRot, originalPlayerRot, easeT);
        playerCamera.transform.localRotation = Quaternion.Slerp(targetCameraRot, originalCameraRot, easeT);

        if (t >= 1.0f)
        {
            playerController.transform.rotation = originalPlayerRot;
            playerCamera.transform.localRotation = originalCameraRot;
            currentState = TutorialState.WaitListening;
            playerController._isStop = false;
        }
    }

    private void CheckListening()
    {
        if (ListenSkill.IsListening)
        {
            currentState = TutorialState.Completed;
            OnTutorialComplete?.Invoke();
        }
    }
    private void CheckOrgelSound()
    {
        if (OrgelManager.Instance == null)
        {
            return;
        }

        // 鳴っているオルゴールがない場合は待ち続ける
        if (OrgelManager.Instance.CurrentOrgelPlayingCount <= 0)
        {
            return;
        }

        // 現在鳴っている対象オルゴールを取得
        OrgelSystem targetOrgel = OrgelManager.Instance.CurrentTargetOrgel;

        if (targetOrgel == null)
        {
            return;
        }

        // オルゴールが鳴ったので視線誘導開始
        StartForcedLook(targetOrgel);
    }

    private void OnEnable()
    {
        OrgelSystem.OnOrgelStarted += HandleTutorialOrgelStarted;
    }

    private void OnDisable()
    {
        OrgelSystem.OnOrgelStarted -= HandleTutorialOrgelStarted;
    }

    private void HandleTutorialOrgelStarted(OrgelSystem startedOrgel)
    {
        if (startedOrgel == null)
        {
            return;
        }

        if (firstOrgelLookStarted)
        {
            return;
        }

        firstOrgelLookStarted = true;


        Debug.Log($"【Tutorial】{startedOrgel.name}が鳴ったので視線誘導します");

        StartForcedLook(startedOrgel);
    }

    public void ReturnLookFromTutorialClick()
    {
        // 視線誘導中またはオルゴールを向いている時だけ実行
        if (currentState != TutorialState.ForcedLook && currentState != TutorialState.KeepLooking)
        {
            return;
        }

        // クリックされた瞬間の向きを保存
        returnStartPlayerRot = playerController.transform.rotation;
        returnStartCameraRot = playerCamera.transform.localRotation;

        currentState = TutorialState.ReturnLook;
        lookAtTimer = 0f;
    }
    private void CheckPhase7Distance()
    {
        if (phase7DistanceTriggered)
        {
            return;
        }

        if (playerController == null || tutorialCaller == null || OrgelManager.Instance == null)
        {
            return;
        }

        // ランダムで選ばれた現在のオルゴール
        OrgelSystem playingOrgel = OrgelManager.Instance.CurrentTargetOrgel;

        if (playingOrgel == null)
        {
            return;
        }

        // 実際に鳴っている間だけ判定
        if (!playingOrgel.IsPlaying)
        {
            return;
        }

        float distance = Vector3.Distance(playerController.transform.position, playingOrgel.transform.position);

        if (distance <= phase7Distance)
        {
            tutorialCaller.ShowPhase7();
            phase7DistanceTriggered = true;
        }
    }
}