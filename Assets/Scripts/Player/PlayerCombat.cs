using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerCombat : MonoBehaviour
{
    [Header("Input")]
    [SerializeField] private InputActionReference _attackAction;

    [Header("Melee Attack")]
    [Min(0f)]
    [SerializeField] private float _attackDamage = 25f;
    [Min(0f)]
    [SerializeField] private float _attackCooldown = 0.45f;
    [Min(0f)]
    [SerializeField] private float _attackHitDelay = 0.1f;
    [Min(0f)]
    [SerializeField] private float _attackRange = 0.9f;
    [Min(0f)]
    [SerializeField] private float _attackRadius = 0.45f;
    [SerializeField] private LayerMask _targetMask;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private string _attackTrigger = "Attack";

    [Header("Audio")]
    [SerializeField] private AudioCue _attackSound;
    [SerializeField] private AudioCue _hitSound;
    [SerializeField] private AudioCue _missSound;

    [Header("Debug")]
    [SerializeField] private bool _drawAttackRange = true;

    private PlayerController2D _controller;
    private readonly HashSet<AIEntity> _damagedEntities = new();
    private InputAction _currentAttackAction;
    private float _nextAttackTime;
    private bool _isAttacking;

    private void Awake()
    {
        _controller = GetComponent<PlayerController2D>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();
    }

    private void OnEnable()
    {
        _currentAttackAction = _attackAction != null ? _attackAction.action : null;
        if (_currentAttackAction == null)
            return;

        _currentAttackAction.actionMap?.Enable();
        _currentAttackAction.started += OnAttack;
        _currentAttackAction.Enable();
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _isAttacking = false;

        if (_currentAttackAction == null)
            return;

        _currentAttackAction.started -= OnAttack;
        _currentAttackAction = null;
    }

    public void Attack()
    {
        if (_isAttacking || Time.time < _nextAttackTime)
            return;

        StartCoroutine(AttackRoutine());
    }

    private void OnAttack(InputAction.CallbackContext context)
    {
        if (!context.ReadValueAsButton())
            return;

        Attack();
    }

    private IEnumerator AttackRoutine()
    {
        _isAttacking = true;
        _nextAttackTime = Time.time + Mathf.Max(0f, _attackCooldown);
        TriggerAttackAnimation();
        PlayAudio(_attackSound, transform.position);

        yield return new WaitForSeconds(Mathf.Max(0f, _attackHitDelay));
        DealDamage();

        _isAttacking = false;
    }

    private void DealDamage()
    {
        _damagedEntities.Clear();

        Vector2 hitPosition = GetAttackCenter();
        Collider2D[] hits = Physics2D.OverlapCircleAll(hitPosition, _attackRadius, GetTargetLayerMask());

        for (int i = 0; i < hits.Length; i++)
        {
            AIEntity aiEntity = hits[i].GetComponentInParent<AIEntity>();
            if (aiEntity == null || !aiEntity.IsAlive || _damagedEntities.Contains(aiEntity))
                continue;

            _damagedEntities.Add(aiEntity);
            aiEntity.TakeDamage(_attackDamage, transform);
        }

        PlayAudio(_damagedEntities.Count > 0 ? _hitSound : _missSound, hitPosition);
    }

    private int GetTargetLayerMask()
    {
        return _targetMask.value != 0 ? _targetMask.value : Physics2D.AllLayers;
    }

    private Vector2 GetAttackCenter()
    {
        Vector2 direction = GetAttackDirection();
        return (Vector2)transform.position + direction * _attackRange;
    }

    private Vector2 GetAttackDirection()
    {
        if (_controller == null || _controller.LastMoveDirection.sqrMagnitude <= 0.01f)
            return Vector2.down;

        return _controller.LastMoveDirection.normalized;
    }

    private void TriggerAttackAnimation()
    {
        if (_animator == null || string.IsNullOrEmpty(_attackTrigger))
            return;

        _animator.ResetTrigger(_attackTrigger);
        _animator.SetTrigger(_attackTrigger);
    }

    private void PlayAudio(AudioCue cue, Vector3 position)
    {
        if (cue != null)
            cue.PlayAt(position);
    }

    private void OnDrawGizmosSelected()
    {
        if (!_drawAttackRange)
            return;

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(GetAttackCenter(), _attackRadius);
    }
}
