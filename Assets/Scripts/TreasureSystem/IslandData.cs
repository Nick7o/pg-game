using UnityEngine;

[CreateAssetMenu(fileName = "NewIslandData", menuName = "Treasure/Island Data")]
public class IslandData : ScriptableObject
{
    [Header("Wizualia")]
    public Sprite mapSprite;

    [Header("Połączenie ze Sceną")]
    [Tooltip("Wpisz DOKŁADNIE taką nazwę, jaką ma obiekt wyspy w Hierarchy (np. Main Island)")]
    public string islandGameObjectName;

    [Header("Koordynaty na tej wyspie (w kafelkach)")]
    public Vector2Int minBounds; 
    public Vector2Int maxBounds; 
}