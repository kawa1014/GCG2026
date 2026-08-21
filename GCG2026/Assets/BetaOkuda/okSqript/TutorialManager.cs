using UnityEngine;
using UnityEngine.InputSystem;
using System;

public enum TutorialState
{
    WaitCameraMove,   // 視点移動の入力待ち
    WaitPlayerMove,   // WASD移動の入力待ち
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
    [SerializeField] private float keepLookDuration = 1.0f;
    [Tooltip("振り向いてから元の視点に戻るまでの時間（秒）")]
    [SerializeField] private float returnDuration = 0.8f;

    private TutorialState currentState = TutorialState.WaitCameraMove;

    private float accumulatedCameraMove = 0f;
    private float accumulatedMoveTime = 0f;

    private float lookAtTimer = 0f;

    private Quaternion originalCameraRot;
    private Quaternion originalPlayerRot;
    private Quaternion targetCameraRot;
    private Quaternion targetPlayerRot;

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
        switch (currentState)
        {
            case TutorialState.WaitCameraMove:
                CheckCameraMovement();
                break;

            case TutorialState.WaitPlayerMove:
                CheckPlayerMovement();
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
            StartForcedLook();
            OnPlayerMoveClear?.Invoke();
        }
    }

    private void StartForcedLook()
    {
        OrgelSystem targetOrgel = null;
        if (OrgelManager.Instance != null)
        {
            targetOrgel = OrgelManager.Instance.CurrentTargetOrgel;
        }

        if (targetOrgel == null || playerCamera == null || playerController == null)
        {
            currentState = TutorialState.WaitListening;
            return;
        }

        currentState = TutorialState.ForcedLook;
        playerController._isStop = true;
        lookAtTimer = 0f;

        originalPlayerRot = playerController.transform.rotation;
        originalCameraRot = playerCamera.transform.localRotation;

        Vector3 directionToOrgel = targetOrgel.transform.position - playerCamera.transform.position;

        Vector3 flatDirection = new Vector3(directionToOrgel.x, 0, directionToOrgel.z).normalized;
        if (flatDirection != Vector3.zero)
            targetPlayerRot = Quaternion.LookRotation(flatDirection);
        else
            targetPlayerRot = originalPlayerRot;

        Vector3 localDir = playerController.transform.InverseTransformDirection(directionToOrgel);
        float pitchAngle = Mathf.Atan2(-localDir.y, new Vector2(localDir.x, localDir.z).magnitude) * Mathf.Rad2Deg;
        pitchAngle = Mathf.Clamp(pitchAngle, -80f, 80f);
        targetCameraRot = Quaternion.Euler(pitchAngle, 0, 0);
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
        lookAtTimer += Time.deltaTime;

        // 指定した時間（keepLookDuration）が経過したら
        if (lookAtTimer >= keepLookDuration)
        {
            currentState = TutorialState.ReturnLook; // 帰りステートへ
            lookAtTimer = 0f; // 帰り時間の計測用にタイマーをリセット
        }
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
}