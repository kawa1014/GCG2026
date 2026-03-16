using UnityEngine;
using UnityEngine.InputSystem;

/// <summary>
/// 音源オブジェクトのON/OFF状態を管理し、色で可視化するクラス
/// 実際には音を鳴らさず、プロトタイプとして視覚的に表現します
/// </summary>
public class OrgelSystem : MonoBehaviour
{
    /// <summary>
    /// 現在音が鳴っているかどうかの状態
    /// 外部(これから作る敵管理スクリプトなど)から読み取れるようにpublicにしています
    /// </summary>
    [Tooltip("現在音が鳴っているか(ON/OFF)")]
    public bool isPlaying = false;

    /// <summary>
    /// オブジェクトの色を変更するための描画コンポーネントを保持しておく変数
    /// </summary>
    private Renderer objRenderer;

    /// <summary>
    /// ゲーム開始時に1回だけ呼ばれる初期化処理
    /// </summary>
    private void Start()
    {
        // 自分がくっついているオブジェクトのRendererを取得して保存
        objRenderer = GetComponent<Renderer>();

        // 初期の状態に合わせて色を設定する
        UpdateColor();
    }

    /// <summary>
    /// 毎フレーム呼ばれる処理。キー入力を監視します
    /// </summary>
    void Update()
    {
        // スペースが押された瞬間を検知
        if(Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            // 状態を反転させる
            isPlaying = !isPlaying;

            // 色を更新する
            UpdateColor();
        }
    }

    /// <summary>
    /// isPlayingの状態に応じてオブジェクトの色を変更する自作のメソッド
    /// </summary>
    private void UpdateColor()
    {
        // 取得したRendererの中にあるMaterialの色を変更します
        if (isPlaying)
        {
            // 音が鳴っている状態(ON)は赤色
            objRenderer.material.color = Color.red;
        }
        else
        {
            // 音が止まっている状態(OFF)は白色
            objRenderer.material.color = Color.white;
        }
    }
}