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

    private PlayerController2D _controller;
    private InteractionController _interactionController;
    private Ship _ship;
    private CinemachineCamera _camera;
    private PlayerState _currentState;

    public static Player Instance => _instance;

    public PlayerController2D Controller => _controller;
    public InteractionController InteractionController => _interactionController;
    public Ship Ship => _ship;
    public CinemachineCamera Camera => _camera;
    public PlayerState CurrentState => _currentState;

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
        _ship = GameObject.FindAnyObjectByType<Ship>();

        SetState(PlayerState.Player);
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
}
