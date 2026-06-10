using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class EnemySpawner : MonoBehaviour
{
    [Header("Ustawienia Spawnera")]
    public GameObject enemyPrefab;

    [Tooltip("Co ile sekund spawner próbuje stworzyæ przeciwnika?")]
    public float spawnInterval = 60f;

    [Tooltip("Szansa na spawn przy ka¿dej próbie (w procentach)")]
    [Range(0f, 100f)] public float spawnChance = 60f;

    [Tooltip("Maksymalna iloœæ ¿ywych przeciwników z tego spawnera na raz")]
    public int maxEnemies = 10;

    [Tooltip("Promieñ, w jakim losowo pojawiaj¹ siê przeciwnicy")]
    public float spawnRadius = 4f;

    [Header("Spawn na starcie")]
    [Tooltip("Minimalna iloœæ szkieletów na starcie")]
    public int minInitialSpawns = 0;

    [Tooltip("Maksymalna iloœæ szkieletów na starcie")]
    public int maxInitialSpawns = 5;

    private List<GameObject> spawnedEnemies = new List<GameObject>();

    // Zmienne do przechowywania referencji do ziemi
    private Tilemap mainGround;
    private Tilemap[] additionalGrounds;
    private List<Tilemap> allGroundTilemaps = new List<Tilemap>();

    private void Awake()
    {
        // Spawner szuka wszystkich obiektów z naklejk¹ GroundTilemap
        GroundTilemap[] groundMarkers = Object.FindObjectsByType<GroundTilemap>(FindObjectsSortMode.None);

        foreach (GroundTilemap marker in groundMarkers)
        {
            Tilemap tm = marker.GetComponent<Tilemap>();
            if (tm != null)
            {
                allGroundTilemaps.Add(tm);
            }
        }

        if (allGroundTilemaps.Count == 0)
        {
            Debug.LogError("Spawner nie znalaz³ ¿adnego obiektu ze skryptem GroundTilemap na scenie!");
        }
    }

    private void Start()
    {
        int initialCount = Random.Range(minInitialSpawns, maxInitialSpawns + 1);
        for (int i = 0; i < initialCount; i++)
        {
            if (spawnedEnemies.Count < maxEnemies) SpawnEnemy();
        }
        StartCoroutine(SpawnRoutine());
    }

    private IEnumerator SpawnRoutine()
    {
        while (true)
        {
            yield return new WaitForSeconds(spawnInterval);
            spawnedEnemies.RemoveAll(enemy => enemy == null);

            if (spawnedEnemies.Count >= maxEnemies) continue;

            if (Random.Range(0f, 100f) <= spawnChance)
            {
                SpawnEnemy();
            }
        }
    }

    // --- NOWA FUNKCJA: WERYFIKACJA BEZPIECZNEGO GRUNTU ---
    private bool IsValidSpawnPoint(Vector2 point)
    {
        if (allGroundTilemaps.Count == 0) return false;

        // Przeszukujemy po kolei ka¿dy zarejestrowany tilemap
        foreach (Tilemap tm in allGroundTilemaps)
        {
            if (tm == null) continue;

            Vector3Int cellPosition = tm.WorldToCell(point);

            // Jeœli ten konkretny tilemap ma kafelek w tym miejscu, to mamy sukces!
            if (tm.HasTile(cellPosition))
            {
                return true;
            }
        }

        return false; // Punkt nie wpad³ na ¿adn¹ z wysp
    }

    private void SpawnEnemy()
    {
        Vector2 validPoint = (Vector2)transform.position;
        bool foundSafeSpot = false;

        // Próbujemy maksymalnie 30 razy wylosowaæ punkt, w którym JEST ziemia
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;
            if (IsValidSpawnPoint(randomPoint))
            {
                validPoint = randomPoint;
                foundSafeSpot = true;
                break;
            }
        }

        // Jeœli po 30 próbach nadal losujemy wodê (np. spawner le¿y w oceanie), przerywamy!
        if (!foundSafeSpot)
        {
            Debug.LogWarning("Spawner nie zrespi³ szkieleta, bo nie znalaz³ wokó³ siebie l¹du! SprawdŸ po³o¿enie spawnera.");
            return;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, validPoint, Quaternion.identity);

        AITilemapPathfinder pathfinder = newEnemy.GetComponent<AITilemapPathfinder>();
        if (pathfinder != null && mainGround != null)
        {
            pathfinder.SetWalkableTilemaps(mainGround, additionalGrounds);
        }

        spawnedEnemies.Add(newEnemy);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}