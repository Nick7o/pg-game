using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    public static UIManager Instance;

    [Header("Referencje do Prefabu UI")]
    public GameObject mapPanel;       
    public Image mapDisplay;          
    public RectTransform xMark;       

    private PlayerTreasureHunter player;
    private int currentMapIndex = 0;
    private bool isMapOpen = false;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        player = FindObjectOfType<PlayerTreasureHunter>();
        mapPanel.SetActive(false); 
    }

    private void Update()
    {
        if (Keyboard.current == null) return;

        // Odpowiednik starego Input.GetKeyDown(KeyCode.M)
        if (Keyboard.current.mKey.wasPressedThisFrame)
        {
            ToggleMap();
        }

        // Przełączanie map strzałkami (tylko gdy mapa jest otwarta)
        if (isMapOpen && player.myMaps.Count > 0)
        {
            // Strzałka w prawo
            if (Keyboard.current.rightArrowKey.wasPressedThisFrame)
            {
                currentMapIndex = (currentMapIndex + 1) % player.myMaps.Count;
                RefreshMapDisplay();
            }
            // Strzałka w lewo
            else if (Keyboard.current.leftArrowKey.wasPressedThisFrame)
            {
                currentMapIndex--;
                if (currentMapIndex < 0) currentMapIndex = player.myMaps.Count - 1;
                RefreshMapDisplay();
            }
        }
    }

    private void ToggleMap()
    {
        if (player.myMaps.Count == 0) 
        {
            Debug.Log("Nie masz żadnych map!");
            return;
        }

        isMapOpen = !isMapOpen;
        mapPanel.SetActive(isMapOpen);

        if (isMapOpen) RefreshMapDisplay();
    }

    public void RefreshMapDisplay()
    {
        ActiveTreasureMap activeMap = player.myMaps[currentMapIndex];
        mapDisplay.sprite = activeMap.island.mapSprite;
        PositionXMark(xMark, mapDisplay.rectTransform, activeMap);
    }

    public void OnTreasureDug()
    {
        if (player.myMaps.Count == 0)
        {
            isMapOpen = false;
            mapPanel.SetActive(false);
            return;
        }

        if (currentMapIndex >= player.myMaps.Count)
        {
            currentMapIndex = Mathf.Max(0, player.myMaps.Count - 1);
        }

        if (isMapOpen) RefreshMapDisplay();
    }

    private void PositionXMark(RectTransform xMarkTransform, RectTransform mapRect, ActiveTreasureMap mapData)
    {
        // Matematyka opiera się SZTYWNO na Twoich danych z ScriptableObject
        float worldWidth = (float)(mapData.island.maxBounds.x - mapData.island.minBounds.x);
        float worldHeight = (float)(mapData.island.maxBounds.y - mapData.island.minBounds.y);

        if (worldWidth == 0) worldWidth = 1;
        if (worldHeight == 0) worldHeight = 1;

        // Liczymy proporcję na podstawie Twoich sztywnych ram
        float normalizedX = (float)(mapData.treasureTilePosition.x - mapData.island.minBounds.x) / worldWidth;
        float normalizedY = (float)(mapData.treasureTilePosition.y - mapData.island.minBounds.y) / worldHeight;

        float mapUIWidth = mapRect.rect.width;
        float mapUIHeight = mapRect.rect.height;

        float anchoredX = (normalizedX * mapUIWidth) - (mapUIWidth / 2f);
        float anchoredY = (normalizedY * mapUIHeight) - (mapUIHeight / 2f);

        xMarkTransform.anchoredPosition = new Vector2(anchoredX, anchoredY);

        Debug.Log($"[UI] Krzyżyk ustawiony sztywno wg IslandData. Pozycja: {anchoredX} , {anchoredY}");
    }
}