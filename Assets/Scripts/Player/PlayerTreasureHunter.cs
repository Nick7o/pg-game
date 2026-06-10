using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.InputSystem;

public class PlayerTreasureHunter : MonoBehaviour
{
    [Header("Baza Danych Map")]
    [Tooltip("Wrzuć tutaj WSZYSTKIE pliki IslandData, jakie masz w grze")]
    public List<IslandData> allAvailableIslands; 
    public int mapsToDrawAtOnce = 3; 

    [HideInInspector]
    public List<ActiveTreasureMap> myMaps = new List<ActiveTreasureMap>();

    [Header("Animacje i Dźwięki")]
    public AudioClip dirtDigSound;      
    public AudioClip treasureHitSound;

    [SerializeField]
    private Animator anim;
    private AudioSource audioSource;    
    private bool Digging = false;

    void Start()
    {
        audioSource = GetComponent<AudioSource>();
        DrawNewMaps();
    }

    void Update()
    {
        if (!Digging && Keyboard.current != null && Keyboard.current.spaceKey.wasPressedThisFrame)
        {
            StartCoroutine(DiggingRoutine());
        }
    }

    public void DrawNewMaps()
    {
        if (allAvailableIslands.Count == 0)
        {
            Debug.LogWarning("Baza wysp jest pusta! Dodaj pliki IslandData w Inspektorze gracza.");
            return;
        }

        myMaps.Clear();
        
        // Tworzymy tymczasową pulę, żeby wylosować unikalne mapy (bez powtórek)
        List<IslandData> availablePool = new List<IslandData>(allAvailableIslands);

        for (int i = 0; i < mapsToDrawAtOnce; i++)
        {
            // Zabezpieczenie: jeśli chcemy wylosować 3, a w bazie są np. tylko 2 wyspy
            if (availablePool.Count == 0) break; 

            int randomIndex = Random.Range(0, availablePool.Count);
            IslandData chosenIsland = availablePool[randomIndex];
            
            myMaps.Add(new ActiveTreasureMap(chosenIsland));
            
            // Usuwamy wylosowaną wyspę z tymczasowej puli, żeby jej nie powtórzyć w tym rzucie
            availablePool.RemoveAt(randomIndex); 
        }

        Debug.Log($"[TreasureSystem] Wylosowano {myMaps.Count} nowych map!");
    }

    private IEnumerator DiggingRoutine()
    {
        Digging = true;
        anim.SetBool("Digging", true);
        
        yield return new WaitForSeconds(0.5f);

        TryDigTreasure();

        yield return new WaitForSeconds(0.5f);

        Digging = false;
        anim.SetBool("Digging", false);
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

                int distanceX = Mathf.Abs(playerCell.x - currentMap.treasureTilePosition.x);
                int distanceY = Mathf.Abs(playerCell.y - currentMap.treasureTilePosition.y);
                
                if (distanceX <= 1 && distanceY <= 1)
                {

                    if (treasureHitSound != null)
                    {
                        audioSource.PlayOneShot(treasureHitSound);
                    }

                    int foundGold = Random.Range(10, 91); 
                    
                    if (GameManager.Instance != null)
                    {
                        GameManager.Instance.AddGold(foundGold);
                    }

                    Debug.Log($"[Skarb] Znalazłeś skarb na wyspie {currentMap.island.islandGameObjectName}!");
                    
                    
                    myMaps.RemoveAt(i);

                    if (myMaps.Count == 0)
                    {
                        Debug.Log("Znalazłeś wszystkie skarby! Dostajesz nowe mapy.");
                        DrawNewMaps();
                    }
                    
                    if (UIManager.Instance != null)
                    {
                        UIManager.Instance.OnTreasureDug();
                    }
                    return;
                }
            }
        }

        if (dirtDigSound != null)
        {
            audioSource.PlayOneShot(dirtDigSound);
        }
        Debug.Log("Kopiesz piach, ale nic tu nie ma...");
    }
}