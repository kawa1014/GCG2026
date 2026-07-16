using JetBrains.Annotations;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;
using UnityEngine.UIElements;

public class GenericScreenAnimator : MonoBehaviour
{
    [SerializeField]
    private bool _autoLoad = true;         // 自動でファイルを読み込むかどうか

    [SerializeField]
    private string _filePath;       // ファイルパス
    [SerializeField]
    private int _finalFileNumber;   // 最後のファイル番号
    [SerializeField]
    // 読み込んだテクスチャ達
    private Sprite[] _sprites;

    [SerializeField]
    // アニメーションに使用するイメージ
    private UnityEngine.UI.Image _animationImage;
    [SerializeField]
    // 更新頻度
    private float _updateInterval = 1.0f;
    // 現在の時間
    [SerializeField]
    private float _time = 0.0f;
    [SerializeField]
    // 現在のテクスチャ番号
    private int _textureNumber = 0;

    private bool _finishedAnimation = false;


    void Start()
    {
        if (_autoLoad)
        {
            _sprites = Resources.LoadAll<Sprite>(_filePath);

            //// テクスチャファイル読み込み
            //for (int i = 0; i <= _finalFileNumber; ++i)
            //{
            //    //// ファイルのパスを決定
            //    //string trueFolderPath = Application.streamingAssetsPath + _filePath + i + ".png";

            //    //// ファイルを読み込む
            //    //byte[] bytes = File.ReadAllBytes(trueFolderPath);

            //    //// テクスチャを用意する
            //    //Texture2D texture = new Texture2D(2, 2);
            //    //texture.LoadImage(bytes);

            //    //// 追加する
            //    //_textures.Add(texture);

            //    _sprites = Resources.LoadAll<Sprite>(_filePath);
            //}
        }

        _animationImage.color = new Color(1.0f, 1.0f, 1.0f, 1.0f);

        // テクスチャ更新
        UpdateTexture();
    }

    void Update()
    {
        // 時間更新
        _time += Time.deltaTime;

        // 更新時間になったら
        if (_time >= _updateInterval)
        {
            // テクスチャ更新
            UpdateTexture();

            // 時間リセット
            _time = 0;
        }
    }

    // テクスチャ更新処理
    private void UpdateTexture()
    {
        if (_finishedAnimation) return;

        if (_textureNumber > _finalFileNumber - 1)
        {
            _finishedAnimation = true;
            return;
        }

        //// テクスチャ
        //Texture2D texture = _textures[_textureNumber];

        //Rect rect = new Rect(0, 0, texture.width, texture.height);
        //Vector2 pivot = new Vector2(0.5f, 0.5f);

        // スプライト作成
        //Sprite sprite = Sprite.Create(_textures[_textureNumber], rect, pivot);

        // スプライト更新
        _animationImage.sprite = _sprites[_textureNumber];

        // テクスチャ番号更新
        _textureNumber++;
    }
}
