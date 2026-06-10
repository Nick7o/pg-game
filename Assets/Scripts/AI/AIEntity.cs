using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum AIEntityType
{
    Enemy,
    Animal,
    NPC
}

public enum AIEntityState
{
    Idle,
    Patrol,
    Chase,
    Attack,
    Dead
}

/// <summary>
/// Base class for AI-controlled entities. It handles health, aggression, patrol,
/// chasing, melee attacks and animation state. Tilemap navigation is delegated to
/// AITilemapPathfinder.
/// </summary>
[RequireComponent(typeof(Rigidbody2D))]
[RequireComponent(typeof(AITilemapPathfinder))]
public class AIEntity : MonoBehaviour
{
    [Header("Identity")]
    [SerializeField] private AIEntityType _entityType = AIEntityType.Enemy;
    [SerializeField] private bool _isHostile = true;
    [SerializeField] private bool _becomesHostileWhenDamaged = true;
    [SerializeField] private bool _canAttack = true;

    [Header("Health")]
    [Min(0.01f)]
    [SerializeField] private float _maxHealth = 100f;
    [Min(0f)]
    [SerializeField] private float _health = 100f;
    [SerializeField] private bool _destroyOnDeath = true;
    [Min(0f)]
    [SerializeField] private float _destroyDelay = 0.25f;

    [Header("Movement")]
    [Min(0f)]
    [SerializeField] private float _moveSpeed = 2f;
    [Min(0f)]
    [SerializeField] private float _stoppingDistance = 0.1f;
    [SerializeField] private bool _freezeRotation = true;

    [Header("Pathfinding")]
    [SerializeField] private AITilemapPathfinder _pathfinder;
    [SerializeField] private bool _usePathfinding = true;
    [Min(0f)]
    [SerializeField] private float _pathRefreshInterval = 0.25f;
    [Min(0f)]
    [SerializeField] private float _pathDestinationMoveThreshold = 0.75f;
    [Min(0f)]
    [SerializeField] private float _waypointReachedDistance = 0.08f;

    [Header("Unstuck")]
    [SerializeField] private bool _recalculateWhenStuck = true;
    [Min(0f)]
    [SerializeField] private float _stuckCheckInterval = 0.2f;
    [Min(0f)]
    [SerializeField] private float _stuckMoveDistanceThreshold = 0.025f;
    [Min(0f)]
    [SerializeField] private float _stuckTimeToRecalculate = 0.55f;
    [Min(0f)]
    [SerializeField] private float _blockedPathRetryDelay = 0.1f;

    [Header("Patrol")]
    [SerializeField] private bool _canPatrol = true;
    [Min(0f)]
    [SerializeField] private float _patrolRadius = 4f;
    [Min(0f)]
    [SerializeField] private float _patrolPointReachedDistance = 0.15f;
    [Min(0f)]
    [SerializeField] private float _minPatrolWaitTime = 1f;
    [Min(0f)]
    [SerializeField] private float _maxPatrolWaitTime = 3f;
    [Min(1)]
    [SerializeField] private int _patrolPointAttempts = 10;

    [Header("Detection")]
    [SerializeField] private Transform _target;
    [Min(0f)]
    [SerializeField] private float _detectionRange = 5f;
    [Min(0f)]
    [SerializeField] private float _loseTargetRange = 7f;
    [SerializeField] private LayerMask _visionBlockerMask;

    [Header("Melee Attack")]
    [Min(0f)]
    [SerializeField] private float _attackRange = 0.9f;
    [Min(0f)]
    [SerializeField] private float _attackDamage = 10f;
    [Min(0f)]
    [SerializeField] private float _attackCooldown = 1f;
    [Min(0f)]
    [SerializeField] private float _attackHitDelay = 0.2f;
    [Min(0f)]
    [SerializeField] private float _attackLockTime = 0.45f;

    [Header("Animation")]
    [SerializeField] private Animator _animator;
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private string _idleTrigger = "Idle";
    [SerializeField] private string _walkTrigger = "Walk";
    [SerializeField] private string _attackTrigger = "Attack";
    [Min(0f)]
    [SerializeField] private float _walkAnimationSpeedThreshold = 0.01f;
    [SerializeField] private bool _reverseSpriteFlipDirection = false;

    [Header("Feedback")]
    [SerializeField] private HitFeedback _hitFeedback;
    [Min(0f)]
    [SerializeField] private float _hitStunTime = 0.12f;

    [Header("Debug")]
    [SerializeField] private bool _drawCurrentPath = true;

    private readonly List<Vector2> _path = new();

    private Rigidbody2D _rb;
    private Vector2 _spawnPosition;
    private Vector2 _patrolTargetPosition;
    private Vector2 _pathDestination;
    private Transform _currentTarget;
    private AIEntityState _currentState = AIEntityState.Idle;
    private float _waitUntilTime;
    private float _nextAttackTime;
    private float _nextPathRefreshTime;
    private float _stunnedUntilTime;
    private float _nextStuckCheckTime;
    private float _stuckTime;
    private int _pathIndex;
    private bool _hasPatrolTarget;
    private bool _isAttackLocked;
    private bool _isWalking;
    private bool _triedToMoveSinceLastStuckCheck;
    private Vector2 _lastStuckCheckPosition;

    public AIEntityType EntityType => _entityType;
    public AIEntityState CurrentState => _currentState;
    public bool IsHostile => _isHostile;
    public bool IsAlive => _currentState != AIEntityState.Dead && _health > 0f;
    public bool CanAttack => _canAttack;
    public float MaxHealth => _maxHealth;

    public event System.Action<AIEntity> Died;

    public float Health
    {
        get => _health;
        protected set
        {
            if (_currentState == AIEntityState.Dead)
                return;

            _health = Mathf.Clamp(value, 0f, _maxHealth);

            if (_health <= 0f)
                Die();
        }
    }

    private bool HasActivePath => _pathIndex < _path.Count;

    private void Awake()
    {
        _rb = GetComponent<Rigidbody2D>();

        if (_freezeRotation)
            _rb.freezeRotation = true;

        if (_pathfinder == null)
            _pathfinder = GetComponent<AITilemapPathfinder>();

        if (_animator == null)
            _animator = GetComponentInChildren<Animator>();

        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_hitFeedback == null)
            _hitFeedback = GetComponentInChildren<HitFeedback>();

        if (_hitFeedback == null)
            _hitFeedback = gameObject.AddComponent<HitFeedback>();

        _maxHealth = Mathf.Max(0.01f, _maxHealth);
        _health = Mathf.Clamp(_health, 0f, _maxHealth);
        _spawnPosition = _rb.position;
        _patrolTargetPosition = _spawnPosition;
        _lastStuckCheckPosition = _rb.position;
    }

    private void OnValidate()
    {
        _maxHealth = Mathf.Max(0.01f, _maxHealth);
        _health = Mathf.Clamp(_health, 0f, _maxHealth);
        _maxPatrolWaitTime = Mathf.Max(_minPatrolWaitTime, _maxPatrolWaitTime);
        _loseTargetRange = Mathf.Max(_detectionRange, _loseTargetRange);
    }

    private void OnEnable()
    {
        _nextAttackTime = Time.time + Random.Range(0f, Mathf.Max(0f, _attackCooldown));
        _nextPathRefreshTime = 0f;
        ResetStuckTracking();
        SetState(AIEntityState.Idle);
    }

    private void OnDisable()
    {
        StopAllCoroutines();
        _isAttackLocked = false;
        ClearPath();
    }

    private void FixedUpdate()
    {
        if (!IsAlive)
            return;

        if (_isAttackLocked)
        {
            UpdateMovementAnimation(0f);
            ResetStuckTracking();
            return;
        }

        if (Time.time < _stunnedUntilTime)
        {
            UpdateMovementAnimation(0f);
            ResetStuckTracking();
            return;
        }

        UpdateTarget();

        if (_currentTarget != null)
        {
            HandleCombatMovement();
        }
        else
        {
            HandlePatrolMovement();
        }

        UpdateStuckDetection();
    }

    public virtual void TakeDamage(float damage)
    {
        TakeDamage(damage, null);
    }

    public virtual void TakeDamage(float damage, Transform attacker)
    {
        if (damage <= 0f)
            return;

        PlayHitFeedback(attacker);
        Health -= damage;

        if (_becomesHostileWhenDamaged && IsAlive)
            Provoke(attacker);
    }

    public virtual void Heal(float amount)
    {
        if (amount <= 0f || !IsAlive)
            return;

        Health += amount;
    }

    public void SetHostile(bool isHostile)
    {
        _isHostile = isHostile;

        if (!_isHostile)
        {
            _currentTarget = null;
            ClearPath();
        }
    }

    public void Provoke(Transform target = null)
    {
        if (!IsAlive)
            return;

        _isHostile = true;

        if (target != null)
            _target = target;
        else if (_target == null && Player.Instance != null)
            _target = Player.Instance.transform;

        _currentTarget = _target;
        _nextPathRefreshTime = 0f;
        ClearPath();
    }

    protected virtual void Die()
    {
        if (_currentState == AIEntityState.Dead)
            return;

        StopAllCoroutines();
        _isAttackLocked = false;
        _health = 0f;
        ClearPath();
        SetState(AIEntityState.Dead);
        UpdateMovementAnimation(0f);
        Died?.Invoke(this);

        if (_destroyOnDeath)
            Destroy(gameObject, _destroyDelay);
    }

    private void UpdateTarget()
    {
        if (!_isHostile || !_canAttack)
        {
            _currentTarget = null;
            return;
        }

        Transform target = GetConfiguredTarget();
        if (target == null || !target.gameObject.activeInHierarchy)
        {
            _currentTarget = null;
            return;
        }

        float sqrDistance = ((Vector2)target.position - _rb.position).sqrMagnitude;
        float detectionRangeSqr = _detectionRange * _detectionRange;
        float loseTargetRangeSqr = _loseTargetRange * _loseTargetRange;

        if (_currentTarget == null)
        {
            if (sqrDistance <= detectionRangeSqr && HasLineOfSight(target))
                _currentTarget = target;

            return;
        }

        if (sqrDistance > loseTargetRangeSqr || !HasLineOfSight(target))
        {
            _currentTarget = null;
            ClearPath();
        }
    }

    private Transform GetConfiguredTarget()
    {
        if (_target != null)
            return _target;

        return Player.Instance != null ? Player.Instance.transform : null;
    }

    private bool HasLineOfSight(Transform target)
    {
        if (_visionBlockerMask.value == 0)
            return true;

        Vector2 origin = _rb.position;
        Vector2 targetPosition = target.position;
        Vector2 direction = targetPosition - origin;
        float distance = direction.magnitude;

        if (distance <= Mathf.Epsilon)
            return true;

        RaycastHit2D hit = Physics2D.Raycast(origin, direction.normalized, distance, _visionBlockerMask);
        return hit.collider == null;
    }

    private void HandleCombatMovement()
    {
        Vector2 targetPosition = _currentTarget.position;
        float distance = Vector2.Distance(_rb.position, targetPosition);

        if (distance <= _attackRange)
        {
            ClearPath();
            UpdateSpriteDirection(targetPosition - _rb.position);
            UpdateMovementAnimation(0f);
            TryAttack();
            return;
        }

        SetState(AIEntityState.Chase);

        float speed = MoveToDestination(targetPosition, _stoppingDistance, true);
        UpdateMovementAnimation(speed);
    }

    private void HandlePatrolMovement()
    {
        if (!_canPatrol || _patrolRadius <= 0f)
        {
            UpdateMovementAnimation(0f);
            SetState(AIEntityState.Idle);
            return;
        }

        if (Time.time < _waitUntilTime)
        {
            UpdateMovementAnimation(0f);
            SetState(AIEntityState.Idle);
            return;
        }

        if (!_hasPatrolTarget)
            SelectPatrolTarget();

        float speed = MoveToDestination(_patrolTargetPosition, _patrolPointReachedDistance, false);
        SetState(speed > _walkAnimationSpeedThreshold ? AIEntityState.Patrol : AIEntityState.Idle);
        UpdateMovementAnimation(speed);

        bool reachedPatrolPoint = Vector2.Distance(_rb.position, _patrolTargetPosition) <= _patrolPointReachedDistance;
        bool lostPathToPatrolPoint = speed <= _walkAnimationSpeedThreshold && !HasActivePath && !reachedPatrolPoint;

        if (reachedPatrolPoint || lostPathToPatrolPoint)
        {
            _hasPatrolTarget = false;
            _waitUntilTime = Time.time + GetRandomPatrolWaitTime();
            ClearPath();
        }
    }

    private void SelectPatrolTarget()
    {
        _hasPatrolTarget = true;
        ClearPath();

        if (CanUsePathfinding() && _pathfinder.TryGetRandomReachablePoint(
            _rb.position,
            _spawnPosition,
            _patrolRadius,
            _patrolPointAttempts,
            out _patrolTargetPosition,
            _path))
        {
            _pathDestination = _patrolTargetPosition;
            _pathIndex = 0;
            return;
        }

        for (int i = 0; i < _patrolPointAttempts; i++)
        {
            Vector2 candidate = _spawnPosition + Random.insideUnitCircle * _patrolRadius;
            if (IsWorldPositionWalkable(candidate))
            {
                _patrolTargetPosition = candidate;
                return;
            }
        }

        _patrolTargetPosition = _spawnPosition;
    }

    private float MoveToDestination(Vector2 destination, float stopDistance, bool refreshPath)
    {
        if (Vector2.Distance(_rb.position, destination) <= stopDistance)
            return 0f;

        if (CanUsePathfinding())
        {
            if (ShouldRefreshPath(destination, refreshPath))
                RequestPath(destination);

            if (HasActivePath)
                return MoveAlongPath(destination, stopDistance);
        }

        return MoveDirectly(destination, stopDistance);
    }

    private bool ShouldRefreshPath(Vector2 destination, bool refreshPath)
    {
        if (!refreshPath)
            return false;

        if (Time.time < _nextPathRefreshTime)
            return false;

        if (!HasActivePath)
            return true;

        float refreshDistance = _pathDestinationMoveThreshold * _pathDestinationMoveThreshold;
        return Vector2.SqrMagnitude(_pathDestination - destination) >= refreshDistance;
    }

    private bool RequestPath(Vector2 destination)
    {
        _nextPathRefreshTime = Time.time + Mathf.Max(0.01f, _pathRefreshInterval);
        _pathDestination = destination;

        if (!_pathfinder.TryFindPath(_rb.position, destination, _path))
        {
            ClearPath();
            return false;
        }

        _pathIndex = 0;
        SkipReachedWaypoints();
        return HasActivePath;
    }

    private float MoveAlongPath(Vector2 finalDestination, float stopDistance)
    {
        SkipReachedWaypoints();

        if (!HasActivePath || Vector2.Distance(_rb.position, finalDestination) <= stopDistance)
            return 0f;

        return MoveDirectly(_path[_pathIndex], _waypointReachedDistance);
    }

    private void SkipReachedWaypoints()
    {
        while (HasActivePath && Vector2.Distance(_rb.position, _path[_pathIndex]) <= _waypointReachedDistance)
            _pathIndex++;
    }

    private float MoveDirectly(Vector2 destination, float stopDistance)
    {
        Vector2 currentPosition = _rb.position;
        Vector2 toDestination = destination - currentPosition;

        if (toDestination.sqrMagnitude <= stopDistance * stopDistance)
            return 0f;

        Vector2 direction = toDestination.normalized;
        Vector2 nextPosition = Vector2.MoveTowards(currentPosition, destination, _moveSpeed * Time.fixedDeltaTime);

        if (!IsWorldPositionWalkable(nextPosition) || IsMovementBlocked(currentPosition, nextPosition))
            return HandleBlockedMovement();

        _rb.MovePosition(nextPosition);
        UpdateSpriteDirection(direction);
        _triedToMoveSinceLastStuckCheck = true;

        return (nextPosition - currentPosition).magnitude / Time.fixedDeltaTime;
    }

    private float HandleBlockedMovement()
    {
        _nextPathRefreshTime = 0f;
        ClearPath();

        if (_currentTarget == null)
        {
            _hasPatrolTarget = false;
            _waitUntilTime = Time.time + _blockedPathRetryDelay;
        }

        return 0f;
    }

    private bool CanUsePathfinding()
    {
        return _usePathfinding && _pathfinder != null && _pathfinder.HasWalkableTilemap;
    }

    private bool IsWorldPositionWalkable(Vector2 position)
    {
        return _pathfinder == null || _pathfinder.IsWorldPositionWalkable(position);
    }

    private bool IsMovementBlocked(Vector2 currentPosition, Vector2 nextPosition)
    {
        return _pathfinder != null && _pathfinder.IsMovementBlocked(currentPosition, nextPosition);
    }

    private void UpdateStuckDetection()
    {
        if (!_recalculateWhenStuck)
            return;

        if (Time.time < _nextStuckCheckTime)
            return;

        _nextStuckCheckTime = Time.time + Mathf.Max(0.02f, _stuckCheckInterval);

        if (!_triedToMoveSinceLastStuckCheck)
        {
            ResetStuckTracking();
            return;
        }

        float movedDistance = Vector2.Distance(_rb.position, _lastStuckCheckPosition);
        if (movedDistance <= _stuckMoveDistanceThreshold)
            _stuckTime += _stuckCheckInterval;
        else
            _stuckTime = 0f;

        _lastStuckCheckPosition = _rb.position;
        _triedToMoveSinceLastStuckCheck = false;

        if (_stuckTime < _stuckTimeToRecalculate)
            return;

        ForcePathRecalculation();
    }

    private void ForcePathRecalculation()
    {
        _stuckTime = 0f;
        _nextPathRefreshTime = 0f;
        ClearPath();

        if (_currentTarget != null)
            return;

        _hasPatrolTarget = false;
        _waitUntilTime = Time.time + _blockedPathRetryDelay;
    }

    private void ResetStuckTracking()
    {
        _stuckTime = 0f;
        _triedToMoveSinceLastStuckCheck = false;
        _lastStuckCheckPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        _nextStuckCheckTime = Time.time + Mathf.Max(0.02f, _stuckCheckInterval);
    }

    private void ClearPath()
    {
        _path.Clear();
        _pathIndex = 0;
    }

    private void TryAttack()
    {
        if (Time.time < _nextAttackTime)
            return;

        StartCoroutine(AttackRoutine());
    }

    private float GetRandomPatrolWaitTime()
    {
        float minWaitTime = Mathf.Max(0f, _minPatrolWaitTime);
        float maxWaitTime = Mathf.Max(minWaitTime, _maxPatrolWaitTime);
        return Random.Range(minWaitTime, maxWaitTime);
    }

    private IEnumerator AttackRoutine()
    {
        _isAttackLocked = true;
        _nextAttackTime = Time.time + Mathf.Max(0f, _attackCooldown);
        SetState(AIEntityState.Attack);
        TriggerAnimation(_attackTrigger);

        yield return new WaitForSeconds(Mathf.Max(0f, _attackHitDelay));
        DealDamageToTarget();

        float remainingLockTime = Mathf.Max(0f, _attackLockTime - _attackHitDelay);
        if (remainingLockTime > 0f)
            yield return new WaitForSeconds(remainingLockTime);

        _isAttackLocked = false;
        _isWalking = false;
        SetState(AIEntityState.Idle);
        TriggerAnimation(_idleTrigger);
    }

    private void DealDamageToTarget()
    {
        if (_currentTarget == null || !IsAlive)
            return;

        float sqrAttackRange = _attackRange * _attackRange;
        if (((Vector2)_currentTarget.position - _rb.position).sqrMagnitude > sqrAttackRange)
            return;

        Player player = _currentTarget.GetComponentInParent<Player>();
        if (player != null)
            player.TakeDamage(_attackDamage, transform);
    }

    private void PlayHitFeedback(Transform attacker)
    {
        if (_hitFeedback == null)
            return;

        Vector2 sourcePosition = attacker != null ? attacker.position : transform.position;
        _hitFeedback.Play(sourcePosition);
        _stunnedUntilTime = Time.time + _hitStunTime;
    }

    private void UpdateMovementAnimation(float currentSpeed)
    {
        if (_isAttackLocked)
            return;

        bool isWalking = currentSpeed > _walkAnimationSpeedThreshold;
        if (isWalking == _isWalking)
            return;

        _isWalking = isWalking;
        TriggerAnimation(_isWalking ? _walkTrigger : _idleTrigger);
    }

    private void TriggerAnimation(string trigger)
    {
        if (_animator == null || string.IsNullOrEmpty(trigger))
            return;

        ResetTrigger(_idleTrigger);
        ResetTrigger(_walkTrigger);
        ResetTrigger(_attackTrigger);
        _animator.SetTrigger(trigger);
    }

    private void ResetTrigger(string trigger)
    {
        if (!string.IsNullOrEmpty(trigger))
            _animator.ResetTrigger(trigger);
    }

    private void UpdateSpriteDirection(Vector2 direction)
    {
        if (_spriteRenderer == null || Mathf.Approximately(direction.x, 0f))
            return;

        _spriteRenderer.flipX = (direction.x < 0f) != _reverseSpriteFlipDirection;
    }

    private void SetState(AIEntityState state)
    {
        if (_currentState == state)
            return;

        _currentState = state;
    }

    private void OnDrawGizmosSelected()
    {
        Vector2 origin = Application.isPlaying ? _spawnPosition : (Vector2)transform.position;

        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(origin, _patrolRadius);

        Gizmos.color = Color.yellow;
        Gizmos.DrawWireSphere(transform.position, _detectionRange);

        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, _attackRange);

        if (!_drawCurrentPath || !Application.isPlaying || _path.Count <= 0)
            return;

        Gizmos.color = Color.magenta;
        Vector2 previousPoint = transform.position;
        for (int i = Mathf.Max(0, _pathIndex); i < _path.Count; i++)
        {
            Gizmos.DrawLine(previousPoint, _path[i]);
            Gizmos.DrawSphere(_path[i], 0.06f);
            previousPoint = _path[i];
        }
    }
}
