using System.Collections.Generic;
using UnityEngine;

public abstract class Interactable : MonoBehaviour
{
    [SerializeField] private bool _canInteract = true;

    private HashSet<InteractionController> _interactingControllers = new();

    public bool CanInteract => _canInteract && isActiveAndEnabled;

    protected HashSet<InteractionController> InteractingControllers => _interactingControllers;

    public virtual void OnInteractionEnter(InteractionController interactionController)
    {
        _interactingControllers.Add(interactionController);
    }

    public virtual void OnInteractionExit(InteractionController interactionController)
    {
        _interactingControllers.Remove(interactionController);
    }

    public void TryInteract(InteractionController interactionController)
    {
        if (!CanInteract)
            return;

        Interact(interactionController);
    }

    public virtual string GetInteractionPrompt()
    {
        return "Interact";
    }

    protected abstract void Interact(InteractionController interactionController);
}
