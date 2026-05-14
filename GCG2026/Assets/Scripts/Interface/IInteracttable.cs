using UnityEngine;

/// <summary>
/// プレイヤーがインタラクト(干渉)できるオブジェクトが必ず持つべき機能(看板)
/// </summary>
public interface IInteractable
{
    // インタラクトの対象として有効かどうか
    bool IsInteractable {  get; }

    // インタラクトが完了した(ゲージが溜まり切った)時に呼ばれる処理
    void ExecuteInteraction();
}
