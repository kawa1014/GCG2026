using UnityEngine;

public class GameCloser : MonoBehaviour
{
    public void GameQuit()
    {
#if UNITY_EDITOR
        // Unity Editorで開いている場合はこっち
        UnityEditor.EditorApplication.isPlaying = false;
#else
        // ビルドしたexeファイルを開いている場合はこっち
        Application.Quit();
#endif
    }
}
