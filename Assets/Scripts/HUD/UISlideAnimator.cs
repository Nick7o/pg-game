using System.Collections;
using UnityEngine;

[RequireComponent(typeof(RectTransform))]
public class UISlideAnimator : MonoBehaviour
{
    [Header("Ustawienia Animacji")]
    [Tooltip("Sk¹d obiekt ma przyjechaæ? (np. X=-1500 to z lewej, Y=1000 to z góry)")]
    public Vector2 startOffset;
    public float duration = 0.8f;

    public AnimationCurve slideCurve = AnimationCurve.EaseInOut(0, 0, 1, 1);

    private RectTransform rectTransform;
    private Vector2 targetPosition; 
    private bool isInitialized = false;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();
        targetPosition = rectTransform.anchoredPosition;
        isInitialized = true;
    }

    private void OnEnable()
    {
        if (!isInitialized) return;
        StartCoroutine(SlideIn());
    }

    private IEnumerator SlideIn()
    {
        rectTransform.anchoredPosition = targetPosition + startOffset;

        float timeElapsed = 0f;

        while (timeElapsed < duration)
        {
            timeElapsed += Time.unscaledDeltaTime;
            float t = timeElapsed / duration;

            float curveValue = slideCurve.Evaluate(t);

            rectTransform.anchoredPosition = Vector2.Lerp(targetPosition + startOffset, targetPosition, curveValue);
            yield return null;
        }

        rectTransform.anchoredPosition = targetPosition;
    }
}