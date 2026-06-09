using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PlayerTreasureHunter : MonoBehaviour
{
    [Header("Mapy na start (do testów)")]
    public List<IslandData> startingIslands; 

    [HideInInspector]
    public List<ActiveTreasureMap> myMaps = new List<ActiveTreasureMap>();

    void Start()
    {
        // Generujemy mapy - skrypt sam znajdzie odpowiednie kafelki wysp
        foreach (var island in startingIslands)
        {
            myMaps.Add(new ActiveTreasureMap(island));
        }
    }

    void Update()
    {
        if (Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            TryDigTreasure();
        }
    }

    private void TryDigTreasure()
    {
        for (int i = 0; i < myMaps.Count; i++)
        {
            ActiveTreasureMap currentMap = myMaps[i];

            // Jeśli zgubiliśmy referencję do tilemapy wyspy (np. po przeładowaniu sceny)
            if (currentMap.islandTilemap == null)
            {
                GameObject islandGO = GameObject.Find(currentMap.island.islandGameObjectName);
                if (islandGO != null) currentMap.islandTilemap = islandGO.GetComponentInChildren<Tilemap>();
            }

            if (currentMap.islandTilemap != null)
            {
                // KLUCZOWE: Sprawdzamy pozycję gracza na siatce TEJ KONKRETNEJ wyspy
                Vector3Int playerCell = currentMap.islandTilemap.WorldToCell(transform.position);

                if (playerCell == currentMap.treasureTilePosition)
                {
                    Debug.Log($"[Skarb] Znalazłeś skarb na wyspie {currentMap.island.islandGameObjectName}!");
                    
                    myMaps.RemoveAt(i);
                    
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.OnTreasureDug();
                    }
                    return;
                }
            }
        }
        Debug.Log("Kopiesz piach, ale nic tu nie ma...");
    }
}