using UnityEngine;
using UnityEngine.UI;

public class ComptuteShaderTest : MonoBehaviour
{
    public ComputeShader computeShader;
    public RenderTexture renderTexture;
    public RawImage rawImage;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        renderTexture = new RenderTexture(256, 256, 24);
        renderTexture.enableRandomWrite = true;
        renderTexture.Create();

        int kermel = computeShader.FindKernel("CSMain");

        computeShader.SetTexture(kermel, "Result", renderTexture);
        computeShader.Dispatch(kermel, renderTexture.width / 8, renderTexture.height / 8, 1);

        rawImage.texture = renderTexture;
    }

    //private void OnRenderImage(RenderTexture source, RenderTexture destination)
    //{
    //    if (renderTexture == null) 
    //    {
    //        renderTexture = new RenderTexture(256, 256, 24);
    //        renderTexture.enableRandomWrite = true;
    //        renderTexture.Create();
    //    }

    //    int kermel = computeShader.FindKernel("CSMain");

    //    computeShader.SetTexture(kermel, "Result", renderTexture);
    //    computeShader.Dispatch(kermel, renderTexture.width / 8, renderTexture.height / 8, 1);

    //    Graphics.Blit(renderTexture, destination);

    //    Debug.Log("シェーダー適用！");
    //}
}
