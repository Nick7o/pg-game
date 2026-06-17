using UnityEngine;
using TMPro;
using System.Collections;

[RequireComponent(typeof(TMP_Text))]
public class GoldDisplay : MonoBehaviour
{
    private TMP_Text goldText;
    [SerializeField] private Color _gainColor = new(1f, 0.84f, 0.18f, 1f);
    [SerializeField] private float _gainScale = 1.18f;
    [SerializeField] private float _feedbackDuration = 0.45f;
    [SerializeField] private bool _showGainSuffix = true;

    private int _lastGold;
    private bool _initialized;
    private bool _showingFeedback;
    private Color _baseColor;
    private Vector3 _baseScale;
    private Coroutine _feedbackRoutine;

    void Awake()
    {
        goldText = GetComponent<TMP_Text>();
        _baseColor = goldText.color;
        _baseScale = transform.localScale;
    }

    void Update()
    {
        if (GameManager.Instance == null)
            return;

        int currentGold = GameManager.Instance.playerGold;
        if (!_initialized)
        {
            _initialized = true;
            _lastGold = currentGold;
            SetGoldText(currentGold);
            return;
        }

        int delta = currentGold - _lastGold;
        if (delta > 0)
            PlayGainFeedback(currentGold, delta);
        else if (!_showingFeedback)
            SetGoldText(currentGold);

        _lastGold = currentGold;
    }

    private void PlayGainFeedback(int currentGold, int gainedGold)
    {
        if (_feedbackRoutine != null)
            StopCoroutine(_feedbackRoutine);

        _feedbackRoutine = StartCoroutine(GainFeedbackRoutine(currentGold, gainedGold));
    }

    private IEnumerator GainFeedbackRoutine(int currentGold, int gainedGold)
    {
        _showingFeedback = true;
        goldText.color = _gainColor;
        transform.localScale = _baseScale * _gainScale;
        goldText.text = _showGainSuffix ? $"{currentGold} (+{gainedGold})" : currentGold.ToString();

        float elapsedTime = 0f;
        while (elapsedTime < _feedbackDuration)
        {
            elapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(elapsedTime / _feedbackDuration);
            transform.localScale = Vector3.Lerp(_baseScale * _gainScale, _baseScale, t);
            goldText.color = Color.Lerp(_gainColor, _baseColor, t);
            yield return null;
        }

        _showingFeedback = false;
        _feedbackRoutine = null;
        transform.localScale = _baseScale;
        goldText.color = _baseColor;
        SetGoldText(GameManager.Instance != null ? GameManager.Instance.playerGold : currentGold);
    }

    private void SetGoldText(int gold)
    {
        goldText.text = gold.ToString();
    }
}
