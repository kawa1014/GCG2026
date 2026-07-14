using UnityEngine;
using UnityEngine.UI;

public class ListenUI : MonoBehaviour
{
    [SerializeField] private Image uiImage;
    [SerializeField] private Sprite normalSprite;   // 通常時のテクスチャ
    [SerializeField] private Sprite pressedSprite;  // Eキー押した時のテクスチャ

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            uiImage.sprite = pressedSprite;
        }

        if (Input.GetKeyUp(KeyCode.E))
        {
            uiImage.sprite = normalSprite;
        }
    }
}
