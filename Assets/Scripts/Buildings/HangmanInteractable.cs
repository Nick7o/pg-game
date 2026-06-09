using UnityEngine;

public class HangmanInteractable : Interactable
{
    private HangmanManager hangmanManager;

    private void Start()
    {
        hangmanManager = FindAnyObjectByType<HangmanManager>(FindObjectsInactive.Include);

        if (hangmanManager == null)
        {
            Debug.LogError("UWAGA! Klasztor nie znalaz³ HangmanManagera na scenie! Upewnij siê, ¿e Hangman_Canvas jest na scenie.");
        }
    }

    protected override void Interact(InteractionController interactionController)
    {
        if (hangmanManager != null && !hangmanManager.gameObject.activeSelf)
        {
            hangmanManager.OpenScreen(interactionController.gameObject);
        }
    }

    public override string GetInteractionPrompt()
    {
        return "Play with the Devil";
    }
}