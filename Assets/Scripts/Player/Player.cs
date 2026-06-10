using Unity.Cinemachine;
using UnityEngine;

public enum PlayerState
{
    Player,
    Ship
}

public class Player : MonoBehaviour
{

    private static Player _instance;

    [Range(0f, 100f)]
    [SerializeField] private float _startHealth = 100f;

    private PlayerController2D _controller;
    private InteractionController _interactionController;
    private Ship _ship;
    private CinemachineCamera _camera;
    private HitFeedback _hitFeedback;
    private PlayerState _currentState;

    [Header("Audio")]
    [SerializeField] private AudioCue _hurtSound;
    [SerializeField] private AudioCue _healSound;

    public static Player Instance => _instance;

    public PlayerController2D Controller => _controller;
    public InteractionController InteractionController => _interactionController;
    public Ship Ship => _ship;
    public CinemachineCamera Camera => _camera;
    public PlayerState CurrentState => _currentState;

    private bool _isDead = false;

    public float Health
    {
        get => _startHealth;
        set
        {
            _startHealth = Mathf.Clamp(value, 0f, 100f);
            HUD.Instance.UpdateHealthBar(_startHealth, 100f);
        }
    }

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;

        _controller = GetComponent<PlayerController2D>();
        _interactionController = GetComponentInChildren<InteractionController>();
        _camera = GetComponentInChildren<CinemachineCamera>();
        _hitFeedback = GetComponentInChildren<HitFeedback>();

        if (_hitFeedback == null)
            _hitFeedback = gameObject.AddComponent<HitFeedback>();

        _ship = GameObject.FindAnyObjectByType<Ship>();

        SetState(PlayerState.Player);
    }

    public void TakeDamage(float damage, Transform attacker = null)
    {
        if (damage <= 0f || _isDead)
            return;

        if (_hitFeedback != null)
        {
            Vector2 sourcePosition = attacker != null ? attacker.position : transform.position;
            _hitFeedback.Play(sourcePosition);
        }

        if (_hurtSound != null)
            _hurtSound.PlayAt(transform.position);

        Health -= damage;

        if (Health <= 0f)
        {
            _isDead = true;
            GameFlowController.Instance.TriggerPlayerDeath();
        }
    }

    public void Heal(float amount)
    {
        if (amount <= 0f)
            return;

        if (_healSound != null)
            _healSound.PlayAt(transform.position);

        Health += amount;
    }

    public void SetState(PlayerState state)
    {
        if (_ship == null)
            return;

        Debug.Log($"Switching player state to {state}");

        _camera.enabled = state == PlayerState.Player;
        _ship.Camera.enabled = state == PlayerState.Ship;

        _controller.enabled = state == PlayerState.Player;
        _ship.Controller.enabled = state == PlayerState.Ship;

        _interactionController.enabled = state == PlayerState.Player;
        _ship.InteractionController.enabled = state == PlayerState.Ship;

        gameObject.SetActive(state == PlayerState.Player);

        _currentState = state;
    }

    public void ResetDeathState()
    {
        _isDead = false;
    }
}
