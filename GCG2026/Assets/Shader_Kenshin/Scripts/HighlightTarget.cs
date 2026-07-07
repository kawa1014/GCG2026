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
            // ハイライトにする
            Highlight();


            // 向こうに戻す
            isEnableHighlight = false;
        }
    }


    // ハイライトを有効にする処理
    // 外から呼び出す
    public void EnableHighlight()
    {
        isEnableHighlight = true;
    }


    // 実際にハイライトにする
    private void Highlight()
    {

    }
}
