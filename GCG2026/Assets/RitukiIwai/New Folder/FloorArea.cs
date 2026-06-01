using UnityEngine;

/// <summary>
/// 床が何階かを表すコンポーネント。
/// 1階の床なら Floor Index = 1、2階の床なら Floor Index = 2 にする。
/// </summary>
public class FloorArea : MonoBehaviour
{
    [SerializeField] private int floorIndex = 1;

    /// <summary>
    /// この床の階数。
    /// </summary>
    public int FloorIndex
    {
        get { return floorIndex; }
    }
}