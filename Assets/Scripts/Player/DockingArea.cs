using UnityEngine;

public class DockingArea : Interactable
{
    [SerializeField] private Transform _shipDockingPoint;
    [SerializeField] private Transform _playerExitPoint;

    protected override void Interact(InteractionController interactionController)
    {
        if (Player.Instance.CurrentState == PlayerState.Player)
        {
            Player.Instance.SetState(PlayerState.Ship);
        }
        else
        {
            Player.Instance.transform.position = _playerExitPoint.position;
            Player.Instance.SetState(PlayerState.Player);
        }
    }

    public override string GetInteractionPrompt()
    {
        return Player.Instance.CurrentState == PlayerState.Player ? "Wejdź na statek" : "Wejdź na ląd";
    }
}
