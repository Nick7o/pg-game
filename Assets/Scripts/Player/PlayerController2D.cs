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
    [SerializeField] private InputActionReference _moveAction;
    
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

    public Vector2 LastMoveDirection => _lastMoveDirection;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();
    }

    private void OnEnable()
    {
        InputAction moveAction = _moveAction != null ? _moveAction.action : null;
        if (moveAction == null)
            return;

        moveAction.Enable();
    }

    private void OnDisable()
    {
        _moveInput = Vector2.zero;
        UpdateAnimationState(0f);
    }

    private void FixedUpdate()
    {
        ReadMoveInput();

        Vector2 movement = _moveInput.normalized * _moveSpeed * Time.fixedDeltaTime;
        _rb.MovePosition(_rb.position + movement);

        float currentSpeed = movement.magnitude / Time.fixedDeltaTime;
        UpdateAnimationState(currentSpeed);
    }

    private void ReadMoveInput()
    {
        InputAction moveAction = _moveAction != null ? _moveAction.action : null;
        _moveInput = moveAction != null ? moveAction.ReadValue<Vector2>() : Vector2.zero;

        if (_moveInput.sqrMagnitude <= 0.01f)
            return;

        _lastMoveDirection = _moveInput.normalized;
        UpdateSpriteDirection(_lastMoveDirection);
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
