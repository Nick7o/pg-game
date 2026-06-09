using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionController : MonoBehaviour
{
    [SerializeField] private bool _debugLogs;
    [SerializeField] private InputActionReference _interactAction;

    private readonly List<Interactable> _interactablesInRange = new();
    private InputAction _currentInteractAction;

    public Interactable CurrentInteractable => GetClosestInteractable();

    private void OnEnable()
    {
        _currentInteractAction = _interactAction != null ? _interactAction.action : null;
        if (_currentInteractAction == null)
        {
            if (_debugLogs)
                Debug.LogWarning("InteractionController has no interact action assigned.", this);

            return;
        }

        _currentInteractAction.actionMap?.Enable();
        _currentInteractAction.started += OnInteract;
        _currentInteractAction.Enable();

        if (_debugLogs)
            Debug.Log($"InteractionController listening for input action: {_currentInteractAction.name}.", this);
    }

    private void OnDisable()
    {
        if (_currentInteractAction == null)
            return;

        _currentInteractAction.started -= OnInteract;
        _currentInteractAction = null;
    }

    private void Update()
    {
        Interactable interactable = CurrentInteractable;
        HUD.Instance.ShowInteractionPrompt(interactable != null, interactable != null ? interactable.GetInteractionPrompt() : string.Empty);
    }

    private void OnInteract(InputAction.CallbackContext context)
    {
        if (_debugLogs)
            Debug.Log($"Interact input received from {context.action.name}.", this);

        if (!context.ReadValueAsButton())
            return;

        Interact();
    }

    public void Interact()
    {
        if (_debugLogs)
            Debug.Log($"Attempting to interact. Interactables in range: {_interactablesInRange.Count}.", this);

        Interactable interactable = CurrentInteractable;
        if (interactable == null)
            return;

        if (_debugLogs)
            Debug.Log($"Interacting with {interactable.name}.", interactable);

        interactable.TryInteract(this);
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (interactable == null || _interactablesInRange.Contains(interactable))
            return;

        _interactablesInRange.Add(interactable);
        interactable.OnInteractionEnter(this);
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        Interactable interactable = other.GetComponentInParent<Interactable>();
        if (interactable == null || !_interactablesInRange.Remove(interactable))
            return;

        interactable.OnInteractionExit(this);
    }

    private Interactable GetClosestInteractable()
    {
        Interactable closest = null;
        float closestDistance = float.MaxValue;

        for (int i = _interactablesInRange.Count - 1; i >= 0; i--)
        {
            Interactable interactable = _interactablesInRange[i];
            if (interactable == null)
            {
                _interactablesInRange.RemoveAt(i);
                continue;
            }

            if (!interactable.CanInteract)
                continue;

            float distance = Vector2.SqrMagnitude(interactable.transform.position - transform.position);
            if (distance >= closestDistance)
                continue;

            closest = interactable;
            closestDistance = distance;
        }

        return closest;
    }
}
