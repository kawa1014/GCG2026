using UnityEngine;
using UnityEditor;
using UnityEditor.TerrainTools;

/// <summary>
///     グリッドスナップと複数階層に対応した3Dマップ制作補助ツール
///     Sceneビュー上でショートカットキーを用いて高さを切りかえ、配置の基準となる高さを視覚化します
/// </summary>
public class MapEditorWindow : EditorWindow
{
    // ==============================================================================
    // フィールド
    // ==============================================================================

    /// <summary>
    /// 現在編集中の階層
    /// 0を1とし、1が2階、-1が地下というように適用しています
    /// </summary>
    [Header("===現在編集中の階層===")]
    [SerializeField] private int currentLayer = 0;

    /// <summary>
    /// 1階層あたりの高さ
    /// 制作するゲームの建物に合わせて変更します
    /// </summary>
    [Header("===1階層あたりの高さ===")]
    [SerializeField] float layerHeight = 3.0f;

    /// <summary>
    /// 配置時はグリッドの1マスサイズ
    /// 現在はUIのみですが、今後はオブジェクトをスナップさせる際の基準地として使用します
    /// </summary>
    [SerializeField] float gridSize = 1.0f;

    /// <summary>
    /// 配置したいプレハブを登録する
    /// </summary>
    [Header("===配置したいプレハブ===")]
    [SerializeField] private GameObject prefabToPlace;

    // 「予測配置機能」現在Sceneビューに表示されているプレビュー用のオブジェクトのインスタンス
    // HidenInInspectorを付けて、インスペクター上には表示されないようにしています
    [SerializeField, HideInInspector] private GameObject currentPreviewInstance;

    // 「予測配置機能」配置するものが変更されたかを検知するための変数
    private GameObject previouslySelectedPrefab;

    // 現在の回転角度
    [SerializeField, HideInInspector] private float currentRotationY = 0.0f;

    // ==============================================================================
    // 初期化・ライフサイクル
    // ==============================================================================

    /// <summary>
    /// メニューバーからウィンドウを開くためのメソッド
    /// </summary>
    [MenuItem("Tools/Map Editor")]
    public static void ShowWindow()
    {
        // ウィンドウのインスタンスを取得、または作成して表示する
        GetWindow<MapEditorWindow>("MapEditor");
    }

    /// <summary>
    /// ウィンドウを開いたときに呼ばれる処理
    /// </summary>
    private void OnEnable()
    {
        // SceneビューのGUI更新処理をデリケートに登録
        // これにより、Sceneビュー上でのマウスやキーボード操作を園地できるようになります
        SceneView.duringSceneGui += OnSceneGUI;
    }

    private void OnDisable()
    {
        SceneView.duringSceneGui -= OnSceneGUI;
        // ツールが閉じられた際、予測用のオブジェクトを確実に削除する
        DestroyPreviewInstance();
    }

    // ==============================================================================
    // GUI描画処理
    // ==============================================================================

    /// <summary>
    /// 専用ウィンドウ内のUIを描画する処理
    /// </summary>
    private void OnGUI()
    {
        GUILayout.Label("マップエディタ設定", EditorStyles.boldLabel);

        // EditorGUILayoutを用いて、インスペクターのように数値を変更できる入力ランを作成
        currentLayer = EditorGUILayout.IntField("現在の階層", currentLayer);
        layerHeight = EditorGUILayout.FloatField("1階層の高さ", layerHeight);
        gridSize = EditorGUILayout.FloatField("グリッドサイズ", gridSize);

        EditorGUILayout.Space(); // 空間を開ける用

        // プレハブを登録するUI
        prefabToPlace = (GameObject)EditorGUILayout.ObjectField("配置するプレハブ", prefabToPlace, typeof(GameObject), false);

        // 使い方を表示
        EditorGUILayout.HelpBox(
                    "上矢印 / 下矢印 : 高さの切り替え\n" +
                    "Rキー : 配置オブジェクトの回転 (90度)\n" +
                    "Shiftキー押下 : 予測プレビュー表示\n" +
                    "Shift + 左クリック : プレハブを配置", MessageType.Info);
    }

    /// <summary>
    /// Sceneビュー上で呼ばれる描画・イベント検知処理
    /// </summary>
    /// <param name="sceneView">現在のSceneViewインスタンス</param>
    private void OnSceneGUI(SceneView sceneView)
    {
        // 現在発生しているイベント(キーボード入力やマウスクリックなど)を取得
        Event e = Event.current;

        int controlID = GUIUtility.GetControlID(FocusType.Passive);

        // ショートカットキーでレイヤーを切り替える
        // キーが押し込まれた瞬間を検知します
        if (e.type == EventType.KeyDown)
        {
            // 上矢印キーで1階層上がる
            if (e.keyCode == KeyCode.UpArrow)
            {
                currentLayer++;
                // イベントを使用済みにして、Unity標準のショートカット処理をブロックする
                e.Use();
                // ウィンドウの表示を更新して、変更された数値をUIに即座に反映
                sceneView.Repaint();
            }
            // 下矢印キーで1階層下がる
            else if (e.keyCode == KeyCode.DownArrow)
            {
                currentLayer--;
                e.Use();
                sceneView.Repaint();
            }
            // 追加：Rキーで90度回転
            else if (e.keyCode == KeyCode.R)
            {
                currentRotationY += 90.0f;
                if (currentRotationY >= 360.0f) currentRotationY = 0.0f;

                // 予測配置表示中なら即座に回転を反映
                if (currentPreviewInstance != null)
                {
                    currentPreviewInstance.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);
                }

                e.Use();

                sceneView.Repaint();
            }
        }

        // Shiftが押されているかどうかの判定
        if (e.shift)
        {
            // Shiftを押している間は、Unity標準のマウス操作をブロックする
            if (e.type == EventType.Layout)
            {
                HandleUtility.AddDefaultControl(controlID);
            }

            // 配置予定のオブジェクトの位置を常に更新する
            UpdatePreviewPosition(e.mousePosition, sceneView.camera);

            // 常に描画を要求して、配置予定のオブジェクトが消えないようにする
            sceneView.Repaint();

            // Shiftを押しながら左クリックしたときの配置処理
            if (e.type == EventType.MouseDown && e.button == 0)
            {
                PlacePrefab(e.mousePosition, sceneView.camera);
                e.Use();
            }
        }
        else
        {
            // Shiftキーが離されたら配置予定のオブジェクトを消す
            if (currentPreviewInstance != null)
            {
                DestroyPreviewInstance();
                sceneView.Repaint();
            }
        }

        // ガイドの表示
        if (e.type == EventType.Repaint)
        {
            DrawCustomGrid();
        }
    }

    // ==============================================================================
    // 内部メソッド
    // ==============================================================================

    /// <summary>
    /// 「予測配置機能」マウスの位置に合わせて配置予定のオブジェクトの位置を更新するメソッド
    /// </summary>
    private void UpdatePreviewPosition(Vector2 mousePosition, Camera sceneCamera)
    {
        // プレハブが未設定、またはShiftキーを離した瞬間の場合は終了
        if (prefabToPlace == null)
        {
            DestroyPreviewInstance();
            return;
        }

        // マウスの位置に対応する仮想平面状の座標を取得
        Ray ray = sceneCamera.ScreenPointToRay(new Vector2(mousePosition.x, sceneCamera.pixelHeight - mousePosition.y));
        float currentY = currentLayer * layerHeight;
        Plane plane = new Plane(Vector3.up, new Vector3(0, currentY, 0));

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 hitPoint = ray.GetPoint(distance);

            // グリッドスナップされた座標
            float snappedX = Mathf.Round(hitPoint.x / gridSize) * gridSize;
            float snappedZ = Mathf.Round(hitPoint.z / gridSize) * gridSize;
            Vector3 previewPosition = new Vector3(snappedX, currentY, snappedZ);

            // 配置予定のオブジェクトのインスタンス化または更新
            // まだ配置予定のオブジェクトが存在しない、または別のプレハブが選択された場合
            if (currentPreviewInstance == null || prefabToPlace != previouslySelectedPrefab)
            {
                DestroyPreviewInstance();

                currentPreviewInstance = Instantiate(prefabToPlace);

                // [重要]配置予定のオブジェクトがSceneに保存されないように設定する
                // DontSaveInEditor: Sceneファイル保存時に含まれない
                // DontSaveInBuild: ビルドしたゲームに含まれない
                // HideInHierarchy: Hierarchyウィンドウに表示しない（スッキリさせるため）
                currentPreviewInstance.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild | HideFlags.HideInHierarchy;

                // コライダーがあれば、マウスのRaycastを邪魔しないように無効化する
                var colliders = currentPreviewInstance.GetComponentsInChildren<Collider>();
                foreach (var col in colliders)
                {
                    col.enabled = false;
                }

                // 選択されたプレハブを記憶
                previouslySelectedPrefab = prefabToPlace;

            }

            // 配置予定のオブジェクトの位置を設定した座標に移動
            currentPreviewInstance.transform.position = previewPosition;
            currentPreviewInstance.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);

        }
    }

    /// <summary>
    /// 【予測プレビュー機能】プレビューオブジェクトを安全に削除するメソッド。
    /// </summary>
    private void DestroyPreviewInstance()
    {
        if (currentPreviewInstance != null)
        {
            // エディタ拡張内でのオブジェクト削除は、DestroyImmediateを使用する
            DestroyImmediate(currentPreviewInstance);
            currentPreviewInstance = null;
            previouslySelectedPrefab = null;
        }
    }

    /// <summary>
    /// マウス位置にプレハブをスナップする配置メソッド
    /// </summary>
    private void PlacePrefab(Vector2 mousePosition, Camera sceneCamera)
    {
        if (prefabToPlace == null)
        {
            Debug.LogWarning("配置するプレハブが設定されていません。Map Editorウィンドウで設定してください。");
            return;
        }

        // マウスのスクリーン座標をSceneビュー上のRayに変換
        Ray ray = sceneCamera.ScreenPointToRay(new Vector2(mousePosition.x, sceneCamera.pixelHeight - mousePosition.y));

        // 現在のレイヤーの高さに仮想的な平面を作成
        float currentY = currentLayer * layerHeight;
        Plane plane = new Plane(Vector3.up, new Vector3(0, currentY, 0));

        // Rayと平面が交差する距離を計算
        if (plane.Raycast(ray, out float distance))
        {
            // 交差点の座標を取得
            Vector3 hitPoint = ray.GetPoint(distance);

            // グリッドサイズに合わせてX, Z座標をスナップ
            // Mathf.Roundで四捨五入することで、最も近いグリッドに吸着させます
            float snappedX = Mathf.Round(hitPoint.x / gridSize) * gridSize;
            float snappedZ = Mathf.Round(hitPoint.z / gridSize) * gridSize;

            // スナップ後の最終的な座標
            Vector3 spawnPosition = new Vector3(snappedX, currentY, snappedZ);

            // プレハブをインスタンス化して配置
            // 追記：予測は一機能の注意点
            // 予測は位置のオブジェクトが存在していても、それはDontSaveなので、
            // 実際に配置する際はPrefabUtility.InstantiatePrefabを使って新しいリンクを生成します
            GameObject newObj = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPlace);
            newObj.transform.position = spawnPosition;
            newObj.transform.rotation = Quaternion.Euler(0, currentRotationY, 0);

            // 生成したオブジェクトを元に戻す履歴に登録
            // この処理を書くことで、間違えておいたとしてもctrl + zで消せるようになります
            Undo.RegisterCreatedObjectUndo(newObj, "Place Prefab");
        }
    }

    /// <summary>
    /// Sceneビューにカスタムグリッドやガイドを描画する処理。
    /// </summary>
    private void DrawCustomGrid()
    {
        // 現在のY座標を計算（階層 × 1フロアの高さ）
        float currentY = currentLayer * layerHeight;

        // Handlesを使用して描画する図形の色を設定（ここでは半透明の青色）
        Handles.color = new Color(0f, 0.5f, 1f, 0.3f);

        // ガイドの中心座標を設定
        Vector3 center = new Vector3(0, currentY, 0);

        // 指定した高さに円盤（平面）を描画し、視覚的なガイドとする
        // Vector3.upは円盤の法線（上向き）を指定、10fは円の半径
        Handles.DrawSolidDisc(center, Vector3.up, 10f);
    }
}
