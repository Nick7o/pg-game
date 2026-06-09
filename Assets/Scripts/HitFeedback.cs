using System.Collections;
using UnityEngine;

public class HitFeedback : MonoBehaviour
{
    [Header("Flash")]
    [SerializeField] private bool _getSpriteRenderersInChildren = true;
    [SerializeField] private SpriteRenderer[] _spriteRenderers;
    [SerializeField] private Color _hitColor = new(1f, 0.35f, 0.35f, 1f);
    [Min(0f)]
    [SerializeField] private float _flashDuration = 0.06f;
    [Min(0f)]
    [SerializeField] private float _recoverDuration = 0.08f;

    [Header("Knockback")]
    [SerializeField] private bool _useKnockback = true;
    [SerializeField] private Rigidbody2D _rb;
    [Min(0f)]
    [SerializeField] private float _knockbackDistance = 0.12f;
    [Min(0f)]
    [SerializeField] private float _knockbackDuration = 0.08f;

    private Color[] _originalColors;
    private Coroutine _feedbackRoutine;

    public bool IsPlaying => _feedbackRoutine != null;

    private void Awake()
    {
        if (_getSpriteRenderersInChildren || _spriteRenderers == null || _spriteRenderers.Length == 0)
            _spriteRenderers = GetComponentsInChildren<SpriteRenderer>();

        if (_rb == null)
            _rb = GetComponentInParent<Rigidbody2D>();

        CacheOriginalColors();
    }

    private void OnDisable()
    {
        if (_feedbackRoutine != null)
        {
            StopCoroutine(_feedbackRoutine);
            _feedbackRoutine = null;
        }

        RestoreColors();
    }

    public void Play(Vector2 sourcePosition)
    {
        Vector2 direction = (Vector2)transform.position - sourcePosition;
        PlayDirection(direction);
    }

    public void PlayDirection(Vector2 direction)
    {
        if (_feedbackRoutine != null)
            StopCoroutine(_feedbackRoutine);

        _feedbackRoutine = StartCoroutine(FeedbackRoutine(GetSafeDirection(direction)));
    }

    private IEnumerator FeedbackRoutine(Vector2 knockbackDirection)
    {
        SetColor(_hitColor);

        if (_useKnockback && _rb != null && _knockbackDistance > 0f && _knockbackDuration > 0f)
        {
            Vector2 startPosition = _rb.position;
            Vector2 targetPosition = startPosition + knockbackDirection * _knockbackDistance;
            float elapsedTime = 0f;

            while (elapsedTime < _knockbackDuration)
            {
                elapsedTime += Time.fixedDeltaTime;
                float t = Mathf.Clamp01(elapsedTime / _knockbackDuration);
                float easedT = 1f - (1f - t) * (1f - t);
                _rb.MovePosition(Vector2.Lerp(startPosition, targetPosition, easedT));
                yield return new WaitForFixedUpdate();
            }
        }
        else if (_flashDuration > 0f)
        {
            yield return new WaitForSeconds(_flashDuration);
        }

        if (_recoverDuration <= 0f)
        {
            RestoreColors();
            _feedbackRoutine = null;
            yield break;
        }

        float recoverElapsedTime = 0f;
        while (recoverElapsedTime < _recoverDuration)
        {
            recoverElapsedTime += Time.deltaTime;
            float t = Mathf.Clamp01(recoverElapsedTime / _recoverDuration);
            LerpToOriginalColors(t);
            yield return null;
        }

        RestoreColors();
        _feedbackRoutine = null;
    }

    private void CacheOriginalColors()
    {
        if (_spriteRenderers == null)
        {
            _originalColors = new Color[0];
            return;
        }

        _originalColors = new Color[_spriteRenderers.Length];
        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            _originalColors[i] = _spriteRenderers[i] != null ? _spriteRenderers[i].color : Color.white;
        }
    }

    private void SetColor(Color color)
    {
        if (_spriteRenderers == null)
            return;

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null)
                _spriteRenderers[i].color = color;
        }
    }

    private void LerpToOriginalColors(float t)
    {
        if (_spriteRenderers == null || _originalColors == null)
            return;

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null && i < _originalColors.Length)
                _spriteRenderers[i].color = Color.Lerp(_hitColor, _originalColors[i], t);
        }
    }

    private void RestoreColors()
    {
        if (_spriteRenderers == null || _originalColors == null)
            return;

        for (int i = 0; i < _spriteRenderers.Length; i++)
        {
            if (_spriteRenderers[i] != null && i < _originalColors.Length)
                _spriteRenderers[i].color = _originalColors[i];
        }
    }

    private Vector2 GetSafeDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.001f)
            return Vector2.up;

        return direction.normalized;
    }
}
