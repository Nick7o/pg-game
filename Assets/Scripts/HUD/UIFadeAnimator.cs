using UnityEngine;
using System.Collections;


[RequireComponent(typeof(CanvasGroup))]
public class UIFadeAnimator : MonoBehaviour
{
    [Header("Ustawienia Pojawiania siê")]
    [Tooltip("Ile sekund czekaæ? (Ustaw tyle samo co Duration w UISlideAnimator, np. 0.8)")]
    public float delayBeforeFade = 0.8f;
    [Tooltip("Jak d³ugo ma trwaæ pojawianie siê?")]
    public float fadeDuration = 0.5f;

    private CanvasGroup canvasGroup;

    private void Awake()
    {
        canvasGroup = GetComponent<CanvasGroup>();
    }

    private void OnEnable()
    {
        StartCoroutine(FadeInRoutine());
    }

    private IEnumerator FadeInRoutine()
    {
        canvasGroup.alpha = 0f;
        canvasGroup.interactable = false;
        canvasGroup.blocksRaycasts = false;

        yield return new WaitForSecondsRealtime(delayBeforeFade);

        float timeElapsed = 0f;
        while (timeElapsed < fadeDuration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            canvasGroup.alpha = Mathf.Clamp01(timeElapsed / fadeDuration);
            yield return null;
        }

        canvasGroup.alpha = 1f;
        canvasGroup.interactable = true;
        canvasGroup.blocksRaycasts = true;
    }
}