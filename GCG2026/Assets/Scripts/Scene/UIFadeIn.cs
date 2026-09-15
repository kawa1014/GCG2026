using UnityEngine;
using System.Collections;

public class UIFadeIn : MonoBehaviour
{
    [Header("ï\é¶éûä‘")]
    [SerializeField]
    private float fadeDuration = 0.6f;

    [Header("Ç”ÇÌÇ¡Ç∆ägëÂÇ∑ÇÈê›íË")]
    [SerializeField]
    private bool useScaleAnimation = true;

    [SerializeField]
    [Range(0.5f, 1f)]
    private float startScale = 0.9f;

    private CanvasGroup canvasGroup;
    private Coroutine fadeCoroutine;
    private Vector3 normalScale;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
        }

        normalScale = transform.localScale;
    }

    private void OnEnable()
    {
        if (canvasGroup == null)
        {
            canvasGroup = GetComponent<CanvasGroup>();
        }

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeInRoutine());
    }

    private void OnDisable()
    {
        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
            fadeCoroutine = null;
        }

        if (canvasGroup != null)
        {
            canvasGroup.alpha = 0f;
        }

        transform.localScale = normalScale;
    }

    private IEnumerator FadeInRoutine()
    {
        float timer = 0f;

        canvasGroup.alpha = 0f;

        if (useScaleAnimation)
        {
            transform.localScale = normalScale * startScale;
        }

        while (timer < fadeDuration)
        {
            timer += Time.unscaledDeltaTime;

            float progress = Mathf.Clamp01(timer / fadeDuration);

            // ç≈èâÇÕÇ‰Ç¡Ç≠ÇËÅAìríÜÇ©ÇÁääÇÁÇ©Ç…ï\é¶
            float smoothProgress = Mathf.SmoothStep(0f, 1f, progress);

            canvasGroup.alpha = smoothProgress;

            if (useScaleAnimation)
            {
                transform.localScale = Vector3.Lerp(normalScale * startScale,
                    normalScale, smoothProgress);
            }

            yield return null;
        }

        canvasGroup.alpha = 1f;
        transform.localScale = normalScale;

        fadeCoroutine = null;
    }
}
