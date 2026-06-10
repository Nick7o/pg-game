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

    
    private Tilemap GetTilemapAtPosition(Vector2 point)
    {
        if (allGroundTilemaps.Count == 0) return null;

        // Przeszukujemy po kolei ka¿d¹ wyspê
        foreach (Tilemap tm in allGroundTilemaps)
        {
            if (tm == null) continue;

            // U¿ywamy siatki TEJ KONKRETNEJ mapy, by sprawdziæ kordynaty
            Vector3Int cellPosition = tm.WorldToCell(point);

            // Jeœli ta mapa ma tu kafelek - znaleŸliœmy wyspê, na której stoimy!
            if (tm.HasTile(cellPosition))
            {
                return tm;
            }
        }

        return null; // Punkt le¿y w wodzie
    }

    private void SpawnEnemy()
    {
        Vector2 validPoint = (Vector2)transform.position;
        Tilemap correctIslandTilemap = null;

        // Próbujemy maksymalnie 30 razy wylosowaæ punkt z ziemi¹
        for (int i = 0; i < 30; i++)
        {
            Vector2 randomPoint = (Vector2)transform.position + Random.insideUnitCircle * spawnRadius;

            correctIslandTilemap = GetTilemapAtPosition(randomPoint);

            if (correctIslandTilemap != null)
            {
                validPoint = randomPoint;
                break; // ZnaleŸliœmy bezpieczny punkt i wiemy, jaka to wyspa!
            }
        }

        // Jeœli po 30 próbach mamy null (brak l¹du)
        if (correctIslandTilemap == null)
        {
            Debug.LogWarning("Spawner nie zrespi³ szkieleta, bo nie znalaz³ wokó³ siebie l¹du! SprawdŸ po³o¿enie spawnera.");
            return;
        }

        GameObject newEnemy = Instantiate(enemyPrefab, validPoint, Quaternion.identity);

        AITilemapPathfinder pathfinder = newEnemy.GetComponent<AITilemapPathfinder>();
        if (pathfinder != null)
        {
            // GENIALNY SKRÓT: Dajemy szkieletowi TYLKO mapê wyspy, na której stoi.
            // Pust¹ tablicê (new Tilemap[0]) dajemy jako dodatkowe wyspy, bo ich nie potrzebuje.
            pathfinder.SetWalkableTilemaps(correctIslandTilemap, new Tilemap[0]);
        }

        spawnedEnemies.Add(newEnemy);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, spawnRadius);
    }
}