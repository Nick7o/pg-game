using UnityEngine;

public class HealerInteractable : Interactable
{
    [Header("Ustawienia Leczenia")]
    public float healAmount = 25f;
    public int healCost = 25;

    protected override void Interact(InteractionController interactionController)
    {
        if (Player.Instance.Health >= 100f)
        {
            Debug.Log("Jesteœ ju¿ w pe³ni zdrów!");
            return;
        }

        if (GameManager.Instance.SpendGold(healCost))
        {
            Player.Instance.Heal(healAmount);
            Debug.Log($"Uleczono o {healAmount} HP!");
        }
        else
        {
            Debug.Log("Za ma³o z³ota na leczenie!");
        }
    }

    public override string GetInteractionPrompt()
    {
        if (Player.Instance != null && Player.Instance.Health >= 100f)
        {
            return "Health Full";
        }

        return $"Heal {healAmount} HP (-{healCost} Gold)";
    }
}