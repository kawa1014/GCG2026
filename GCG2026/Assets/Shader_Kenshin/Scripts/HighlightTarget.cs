using UnityEngine;

public class HighlightTarget : MonoBehaviour
{
    bool isEnableHighlight = false;

    void Start()
    {
        
    }

    void Update()
    {
        // 有効になっていたら
        if (isEnableHighlight)
        {
            // 縁をつける
            SetLayer(gameObject, LayerMask.NameToLayer("Outline"));

            // 元に戻す
            isEnableHighlight = false;
        }
        else 
        {
            // 縁を消す
            SetLayer(gameObject, LayerMask.NameToLayer("Default"));
        }
    }


    // ハイライトを有効にする処理
    // 外から呼び出す
    public void EnableHighlight()
    {
        isEnableHighlight = true;
    }

    private void SetLayer(GameObject obj, int layer)
    {
        if (obj == null)
            return;

        obj.layer = layer;

        foreach(Transform child in obj.transform)
        {
            SetLayer(child.gameObject, layer);
        }
    }
}
