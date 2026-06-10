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

    private void Awake()
    {
        // Spawner szuka ziemi na samym starcie gry
        GroundTilemap[] allGrounds = Object.FindObjectsByType<GroundTilemap>(FindObjectsSortMode.None);
        if (allGrounds.Length > 0)
        {
            mainGround = allGrounds[0].GetComponent<Tilemap>();
            if (allGrounds.Length > 1)
            {
                additionalGrounds = new Tilemap[allGrounds.Length - 1];
                for (int i = 1; i < allGrounds.Length; i++)
                {
                    additionalGrounds[i - 1] = allGrounds[i].GetComponent<Tilemap>();
                }
            }
        }
        else
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
        if (mainGround == null) return false;

        Vector3Int cellPosition = mainGround.WorldToCell(point);

        // Sprawdzamy g³ówn¹ wyspê
        if (mainGround.HasTile(cellPosition)) return true;

        // Sprawdzamy ewentualne inne wyspy
        if (additionalGrounds != null)
        {
            foreach (Tilemap tm in additionalGrounds)
            {
                if (tm != null && tm.HasTile(cellPosition)) return true;
            }
        }

        return false; // W tym miejscu nie ma ¿adnego kafelka ziemi!
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