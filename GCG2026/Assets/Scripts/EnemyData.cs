using UnityEngine;

/// <summery>
/// 敵のパラーメータを管理するScriptableObject
/// プランナーやデザイナーがシーンファイルと競合せずに
/// 敵の速度などのパラメータを簡単に調整できるようにするためのデータファイルです
/// </summery>
[CreateAssetMenu(fileName = "NewEnemyData", menuName = "ScriptableObjects/EnemyData", order = 1)]
public class EnemyData : ScriptableObject
{
    /// <summary>
    /// 敵の移動速度
    /// 徘徊時の移動スピードを決定する変数です
    /// </summary>
    [Header("移動設定")]
    [Tooltip("敵の移動速度")]
    public float moveSpeed = 3.0f;


    /// <summary>
    /// 敵の徘徊半径
    /// 出現した位置からどれくらいの範囲をランダムに歩き回るかを指定する変数
    /// </summary>
    [Tooltip("出現位置からの徘徊範囲の半径")]
    public float wanderRadius = 5.0f;
}
