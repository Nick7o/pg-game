using Unity.Cinemachine;
using UnityEngine;

public class Ship : MonoBehaviour
{
    [SerializeField] private CinemachineCamera _camera;
    [SerializeField] private PlayerController2D _controller;
    [SerializeField] private InteractionController _interactionController;

    public CinemachineCamera Camera => _camera;
    public PlayerController2D Controller => _controller;
    public InteractionController InteractionController => _interactionController;
}
