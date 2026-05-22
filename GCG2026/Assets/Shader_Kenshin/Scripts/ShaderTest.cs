using UnityEngine;

[ExecuteInEditMode]
public class ShaderTest : MonoBehaviour
{
    [SerializeField]
    private Material material;

    private void OnRenderImage(RenderTexture source, RenderTexture destination)
    {
        Graphics.Blit(source, destination, material);
    }
}
