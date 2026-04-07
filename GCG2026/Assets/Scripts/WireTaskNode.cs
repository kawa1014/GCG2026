using UnityEngine;
using UnityEngine.EventSystems; // UIのクリックやドラッグを検知するために使います
using UnityEngine.UI;
using System.Collections.Generic; // RaycastResultのリストを使うために必要

/// <summary>
/// @file WireTaskNode.cs
/// @brief 配線タスクにおけるノードのデータと状態を管理するクラス
/// @details 始点と終点のノードにアタッチし、色情報や接続状態を保存します
/// </summary>
public class WireTaskNode : MonoBehaviour, IPointerDownHandler, IDragHandler, IPointerUpHandler
{
    /// <summary>
    /// 配線の色を定義する列挙型
    /// </summary>
    public enum WireColor
    {
        Red,    ///< 赤色の配線
        Blue,   ///< 青色の配線
        Yellow, ///< 黄色の配線
        Green   ///< 緑色の配線
    }

    [Header("ノード設定")]

    /// <summary>
    /// このノードが担当する色
    /// インスペクターから指定します
    /// </summary>
    [Tooltip("このノードの色を指定します")]
    public WireColor nodeColor;

    /// <summary>
    /// 左側のノード(線を引っ張り出す側)かどうかを判定するフラグ
    /// </summary>
    [Tooltip("チェックを入れると左側(始点)、外すと右側(終点)として扱います")]
    public bool isLeftNode;

    /// <summary>
    /// 正しい色の線が繋がったかどうかを記憶するフラグ
    /// 外部から読み書きするためpublicにしていますが、インスペクターには出しません
    /// </summary>
    [HideInInspector]
    public bool isConnected = false;

    [Header("配線描画設定")]
    /// <summary>
    /// @brief 描画する線の太さ
    /// </summary>
    [Tooltip("生成される線の太さ")]
    public float wireThickness = 15.0f;

    //---内部処理の変数---
    private GameObject currentWire; ///< ドラッグ中に生成される線のオブジェクト
    private RectTransform wireRect; ///< 線のRectTransform(長さや角度の調整用)
    private Canvas parentCanvas; ///< 画面座標の計算に使用する親Canvas
    private Vector2 startPointLocal; ///< ドラッグ開始位置(ノードの中心)

    /// <summary>
    /// @brief 初期化処理
    /// @brief 親のCanvasを取得して座標計算の準備をします
    /// </summary>
    private void Start()
    {
        parentCanvas = GetComponentInParent<Canvas>();
    }

    /// <summary>
    /// @brief ノードがクリックされた瞬間の処理
    /// @brief eventData マウスやタッチの入力上
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"<color=cyan>【Test】{gameObject.name} がクリックされました！</color>");

        // 既に繋がっている、または右側のノードの場合は線を引けないようにする
        if (isConnected || !isLeftNode) return;

        // 線を描画するための新しいUIオブジェクトを動的に作成
        currentWire = new GameObject("Wire_Dynamic");
        currentWire.transform.SetParent(transform.parent); // パネルの子オブジェクトにする
        currentWire.transform.SetAsLastSibling();          // 一番手前に表示する
        
        // Imageコンポーネントを追加して色を設定
        Image wireImage = currentWire.AddComponent<Image>();
        wireImage.color = GetColorValue(nodeColor);
        wireImage.raycastTarget = false; // 線自体がマウスクリックの邪魔にならないようにする

        // RectTransformを設定(左端を基準にして伸びるようにPivotを調整)
        wireRect = currentWire.GetComponent<RectTransform>();
        wireRect.pivot = new Vector2(0.0f, 0.5f);
        wireRect.sizeDelta = new Vector2(0.0f, wireThickness); // 初期サイズ(長さ0, 太さ指定)

        // ノードの中心位置を線の始点とする
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out startPointLocal
        );
        wireRect.localPosition = startPointLocal;
    }

    /// <summary>
    /// @brief マウスをドラッグしている間、毎フレーム呼ばれる処理
    /// @param eventData マウスやタッチの入力情報
    /// </summary>
    public void OnDrag(PointerEventData eventData)
    {
        if (currentWire == null) return;

        // マウスの現在位置をUIパネル内の座標に変換
        Vector2 currentMousePosLocal;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            (RectTransform)transform.parent,
            eventData.position,
            eventData.pressEventCamera,
            out currentMousePosLocal
        );

        // 始点からマウス位置までの「方向」と「距離」を計算
        Vector2 direction = currentMousePosLocal - startPointLocal;
        float distance = direction.magnitude;
        float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

        // 線の長さと角度を更新(マウスに追従させる)
        wireRect.sizeDelta = new Vector2(distance, wireThickness);
        wireRect.localEulerAngles = new Vector3(0.0f, 0.0f, angle);
    }

    /// <summary>
    /// @brief ドラッグを終了した瞬間の処理
    /// @param eventData マウスやタッチ入力情報
    /// </summary>
    public void OnPointerUp(PointerEventData eventData)
    {
        if (currentWire == null) return;

        // マウスの現在位置にあるUI要素をすべて取得する(重なっているものをリスト化)
        List<RaycastResult> results = new List<RaycastResult>();
        EventSystem.current.RaycastAll(eventData, results);

        bool isSuccess = false;

        foreach (RaycastResult result in results)
        {
            // 当たったUIの中に「WireTaskNode」を持っているものがあるかチェック
            WireTaskNode targetNode = result.gameObject.GetComponent<WireTaskNode>();

            // 自分自身ではなく、右側のノードで、色が同じで、まだ繋がっていない場合
            if(targetNode != null && targetNode != this && !targetNode.isLeftNode && targetNode.nodeColor == this.nodeColor && !targetNode.isConnected)
            {
                // 接続成功
                isSuccess = true;
                this.isConnected = true;
                targetNode.isConnected = true;

                // 綺麗に見せるため、線の始点と終点を両方のノードの真ん中二＠言ったり合わせる
                Vector2 startPosLocal = GetComponent<RectTransform>().localPosition;
                Vector2 targetPosLocal = targetNode.GetComponent<RectTransform>().localPosition;

                Vector2 direction = targetPosLocal - startPosLocal;
                float distance = direction.magnitude;
                float angle = Mathf.Atan2(direction.y, direction.x) * Mathf.Rad2Deg;

                // 位置、長さ、角度を最終設定して固定
                wireRect.localPosition = startPosLocal;
                wireRect.sizeDelta = new Vector2(distance, wireThickness);
                wireRect.localEulerAngles = new Vector3(0.0f, 0.0f, angle);

                Debug.Log($"<color=green>【WireTask】{nodeColor}の配線が繋がりました！</color>");

                // マネージャーに「1本繋がったよ！」と報告する
                FindAnyObjectByType<WireTaskManager>().AddConnectedWire();

                break; // 繋がったらループ終了
            }
        }

        if(!isSuccess)
        {
            // 失敗した場合は線を消す
            Debug.Log($"<color=orange>【WireTask】{nodeColor}の配線に失敗しました。線を消去します。</color>");
            Destroy(currentWire);
            currentWire = null;
        }
    }

    /// <summary>
    /// @brief EnumのWireColorから実際のColorオブジェクトに変換する補助メソッド
    /// @param color 変換したいWireColor
    /// @return 実際の表示色(Color)
    /// </summary>
    private Color GetColorValue(WireColor color)
    {
        switch (color)
        {
            case WireColor.Red: return Color.red;
            case WireColor.Blue: return Color.blue;
            case WireColor.Yellow: return Color.yellow;
            case WireColor.Green: return Color.green;
            default: return Color.white;
        }
    }
}
