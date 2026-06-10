using UnityEngine;

[RequireComponent(typeof(AIEntity))]
public class GoldDropper : MonoBehaviour
{
    [Header("Drop")]
    [SerializeField] private bool _dropGold = true;
    [Range(0f, 1f)]
    [SerializeField] private float _dropChance = 1f;
    [Min(0)]
    [SerializeField] private int _minGold = 3;
    [Min(0)]
    [SerializeField] private int _maxGold = 8;
    [Min(1)]
    [SerializeField] private int _minPickups = 1;
    [Min(1)]
    [SerializeField] private int _maxPickups = 3;
    [Min(0f)]
    [SerializeField] private float _scatterRadius = 0.35f;

    [Header("Prefab")]
    [SerializeField] private GoldPickup _goldPickupPrefab;

    [Header("Audio")]
    [SerializeField] private SoundCue _dropSound;

    private AIEntity _aiEntity;

    private void Awake()
    {
        _aiEntity = GetComponent<AIEntity>();
    }

    private void OnValidate()
    {
        _maxGold = Mathf.Max(_minGold, _maxGold);
        _maxPickups = Mathf.Max(_minPickups, _maxPickups);
    }

    private void OnEnable()
    {
        if (_aiEntity == null)
            _aiEntity = GetComponent<AIEntity>();

        if (_aiEntity != null)
            _aiEntity.Died += OnEntityDied;
    }

    private void OnDisable()
    {
        if (_aiEntity != null)
            _aiEntity.Died -= OnEntityDied;
    }

    private void OnEntityDied(AIEntity entity)
    {
        DropGold();
    }

    private void DropGold()
    {
        if (!_dropGold || Random.value > _dropChance)
            return;

        int totalGold = Random.Range(_minGold, _maxGold + 1);
        if (totalGold <= 0)
            return;

        if (_dropSound != null && _dropSound.HasClips)
            _dropSound.PlayAt(transform.position);

        int pickupCount = Mathf.Min(totalGold, Random.Range(_minPickups, _maxPickups + 1));
        int remainingGold = totalGold;

        for (int i = 0; i < pickupCount; i++)
        {
            int remainingPickups = pickupCount - i;
            int pickupAmount = Mathf.Max(1, remainingGold / remainingPickups);
            remainingGold -= pickupAmount;

            Vector2 spawnPosition = (Vector2)transform.position + Random.insideUnitCircle * _scatterRadius;
            SpawnPickup(spawnPosition, pickupAmount);
        }
    }

    private void SpawnPickup(Vector2 position, int amount)
    {
        if (_goldPickupPrefab == null)
        {
            GoldPickup.CreateRuntimePickup(position, amount);
            return;
        }

        GoldPickup pickup = Instantiate(_goldPickupPrefab, position, Quaternion.identity);
        pickup.SetAmount(amount);
    }
}
