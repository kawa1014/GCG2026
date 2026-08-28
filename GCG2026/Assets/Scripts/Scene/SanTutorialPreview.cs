using System.Collections;
using UnityEngine;

public class SanTutorialPreview : MonoBehaviour
{
    [SerializeField]
    private SanGaugeUI sanGaugeUI;

    [SerializeField]
    [Range(0f, 1f)]
    private float previewFearRatio = 0.4f;

    [SerializeField]
    private float previewDuration = 5f;

    private Coroutine previewCoroutine;

    private void Awake()
    {
        // Inspector‚Ö‚Ì“o˜^–Y‚ê‚É‘Î‰
        if (sanGaugeUI == null)
        {
            sanGaugeUI = GetComponent<SanGaugeUI>();
        }
    }

    /// <summary>
    /// SAN’l‚Ìà–¾ƒAƒjƒ[ƒVƒ‡ƒ“‚ğÅ‰‚©‚çÄ¶‚·‚é
    /// </summary>
    public void PlayPreview()
    {
        if (sanGaugeUI == null)
        {
            Debug.LogError("SanTutorialPreview‚ÉSanGaugeUI‚ª“o˜^‚³‚ê‚Ä‚¢‚Ü‚¹‚ñ", this);

            return;
        }

        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
        }

        gameObject.SetActive(true);
        previewCoroutine = StartCoroutine(PreviewRoutine());
    }

    public void StopPreview()
    {
        if (previewCoroutine != null)
        {
            StopCoroutine(previewCoroutine);
            previewCoroutine = null;
        }
    }

    private void OnDisable()
    {
        StopPreview();
    }

    private IEnumerator PreviewRoutine()
    {
        float timer = 0f;

        sanGaugeUI.SetNormalizedValue(0f);

        while (timer < previewDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / previewDuration);

            float displayedRatio = Mathf.Lerp(0f, previewFearRatio, progress);

            sanGaugeUI.SetNormalizedValue(displayedRatio);

            yield return null;
        }

        sanGaugeUI.SetNormalizedValue(previewFearRatio);
        previewCoroutine = null;
    }
}