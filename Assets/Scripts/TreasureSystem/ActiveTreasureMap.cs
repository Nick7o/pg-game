using UnityEngine;
using UnityEngine.Tilemaps;

[System.Serializable]
public class ActiveTreasureMap
{
    public IslandData island;
    public Vector3Int treasureTilePosition;
    
    [System.NonSerialized] public Tilemap islandTilemap; 

    public ActiveTreasureMap(IslandData newIsland)
    {
        island = newIsland;
        
        GameObject islandGO = GameObject.Find(island.islandGameObjectName);
        Debug.Log("[ActiveTreasureMap] Szukam obiektu wyspy o nazwie: " + island.islandGameObjectName, islandGO);
        if (islandGO != null)
        {
            // KLUCZOWA ZMIANA: Szukamy konkretnego dziecka o nazwie "Layer 2 - Flat Ground"
            Transform flatGroundTransform = islandGO.transform.Find("Layer 2 - Flat Ground");
            
            if (flatGroundTransform != null)
            {
                islandTilemap = flatGroundTransform.GetComponent<Tilemap>();
            }
            else
            {
                // Logika awaryjna: jeśli jakaś wyspa nie ma tej warstwy, weźmie pierwszą lepszą
                islandTilemap = islandGO.GetComponentInChildren<Tilemap>();
                Debug.LogWarning($"[TreasureSystem] Nie znaleziono warstwy 'Layer 2 - Flat Ground' na obiekcie {island.islandGameObjectName}. Łapię domyślną Tilemapę!");
            }
        }

        if (islandTilemap != null)
        {
            GenerateTreasureLocation(islandTilemap);
        }
        else
        {
            Debug.LogError($"[TreasureSystem] Nie znaleziono żadnej Tilemapy dla obiektu: {island.islandGameObjectName}!");
        }
    }

    private void GenerateTreasureLocation(Tilemap targetTilemap)
    {
        bool validSpotFound = false;
        int attempts = 0;

        // Losujemy DOKŁADNIE w przedziale, który wpisałeś w ScriptableObject
        while (!validSpotFound && attempts < 100)
        {
            int randomX = Random.Range(island.minBounds.x, island.maxBounds.x);
            int randomY = Random.Range(island.minBounds.y, island.maxBounds.y);
            Vector3Int potentialPos = new Vector3Int(randomX, randomY, 0);

            if (targetTilemap.HasTile(potentialPos))
            {
                treasureTilePosition = potentialPos;
                validSpotFound = true;
            }
            attempts++;
        }

        if (!validSpotFound)
        {
            Debug.LogError($"[TreasureSystem] Nie znaleziono kafelka na 'Flat Ground' w Twoich własnych granicach Min: {island.minBounds} do Max: {island.maxBounds}. Sprawdź, czy dobrze przepisałeś lokalne koordynaty!");
        }
        else
        {
            Debug.Log($"[TreasureSystem] Sukces! Skarb dla '{island.islandGameObjectName}' schowany na pozycji: {treasureTilePosition}");

            // Spawn kostki debugowej do testu
            Vector3 worldPos = targetTilemap.GetCellCenterWorld(treasureTilePosition);
            GameObject debugCube = GameObject.CreatePrimitive(PrimitiveType.Cube);
            debugCube.transform.position = worldPos;
            debugCube.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
            debugCube.GetComponent<Renderer>().material.color = Color.red;
            GameObject.Destroy(debugCube.GetComponent<BoxCollider>());
            debugCube.name = $"DEBUG_SKARB_{island.islandGameObjectName}";
        }
    }
}