using UnityEngine;

public class MapRerollInteractable : Interactable
{
    [Header("Ustawienia Re-rolla")]
    public int rerollCost = 40;

    protected override void Interact(InteractionController interactionController)
    {
        // Pobieramy skrypt łowcy skarbów z gracza
        PlayerTreasureHunter treasureHunter = Player.Instance.GetComponent<PlayerTreasureHunter>();

        if (treasureHunter == null)
        {
            Debug.LogError("Brak komponentu PlayerTreasureHunter na graczu!");
            return;
        }

        // Sprawdzamy tylko, czy gracza stać na re-roll
        if (GameManager.Instance.SpendGold(rerollCost))
        {
            // Losujemy nowe mapy (stare zostaną automatycznie usunięte wewnątrz tej funkcji)
            treasureHunter.DrawNewMaps();
            
            // Odświeżamy UI
            if (UIManager.Instance != null)
            {
                UIManager.Instance.OnTreasureDug(); 
            }

            Debug.Log($"Wylosowano nowe mapy za {rerollCost} złota!");
        }
        else
        {
            Debug.Log("Za mało złota na nowe mapy!");
        }
    }

    public override string GetInteractionPrompt()
    {
        return $"Reroll Maps (-{rerollCost} Gold)";
    }
}