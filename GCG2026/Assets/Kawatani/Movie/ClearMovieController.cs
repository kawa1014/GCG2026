using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

/// <summary>
/// ゲームクリア時のムービー演出を管理するクラス
/// </summary>
public class ClearMovieController : MonoBehaviour
{
    [Header("フェード用UI設定")]
    [SerializeField, Tooltip("ムービー専用のフェード用Image")]
    private Image FadeImage;

    [Header("プレイヤー・扉の参照")]
    [SerializeField, Tooltip("プレイヤーの操作を止めるため、PlayerControllerを指定します")]
    private PlayerController TargetPlayerController;

    [SerializeField, Tooltip("プレイヤー本体(移動させる対象)")]
    private Transform PlayerTransform;

    [SerializeField, Tooltip("開ける対象の扉")]
    private DoorSystem TargetDoor;

    [Header("ムービーの移動ポイント")]
    [SerializeField, Tooltip("ムービー開始時にワープする初期位置")]
    private Transform MoviewStartPosition;

    [SerializeField, Tooltip("扉の前に立つ目標位置")]
    private Transform DoorStandPosition;

    [SerializeField, Tooltip("扉を開けて外に出る目標位置")]
    private Transform OutsidePosition;

    [Header("演出設定")]
    [SerializeField, Tooltip("自動で歩くスピード")]
    private float WalkSpeed = 2.0f;

    /// <summary>
    /// GameManagerから呼ばれる、クリアムービー
    /// </summary>

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
