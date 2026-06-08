using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Serialization;

[RequireComponent(typeof(Rigidbody2D))]
public class PlayerController2D : MonoBehaviour
{
    private const string WalkTrigger = "Walk";
    private const string IdleTrigger = "Idle";
    
    [FormerlySerializedAs("moveSpeed")]
    [Header("Movement")]
    [SerializeField] private float _moveSpeed = 4f;
    
    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private float _walkAnimationSpeedThreshold = 0.01f;

    [Header("Other")]
    [SerializeField] private bool _reverseSpriteFlipDirection = false;

    private Rigidbody2D _rb;
    private Vector2 _moveInput;
    private Vector2 _lastMoveDirection = Vector2.down;
    private bool _isWalking;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    public void OnMove(InputValue value)
    {
        _moveInput = value.Get<Vector2>();

        if (_moveInput.sqrMagnitude > 0.01f)
        {
            _lastMoveDirection = _moveInput.normalized;
            UpdateSpriteDirection(_lastMoveDirection);
        }
    }

    private void FixedUpdate()
    {
        Vector2 movement = _moveInput.normalized * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + movement);

        float currentSpeed = movement.magnitude / Time.fixedDeltaTime;
        UpdateAnimationState(currentSpeed);
    }

    private void UpdateAnimationState(float currentSpeed)
    {
        if (_animator == null)
            return;

        bool isWalking = currentSpeed > _walkAnimationSpeedThreshold;
        if (isWalking == _isWalking)
            return;

        _isWalking = isWalking;
        _animator.ResetTrigger(_isWalking ? IdleTrigger : WalkTrigger);
        _animator.SetTrigger(_isWalking ? WalkTrigger : IdleTrigger);
    }

    private void UpdateSpriteDirection(Vector2 moveDirection)
    {
        if (_spriteRenderer == null || Mathf.Approximately(moveDirection.x, 0f))
            return;

        _spriteRenderer.flipX = (moveDirection.x < 0f) != _reverseSpriteFlipDirection;
    }
}
