using UnityEngine;

[RequireComponent(typeof(CircleCollider2D))]
public class GoldPickup : MonoBehaviour
{
    private static Sprite _generatedGoldSprite;

    [Header("Gold")]
    [Min(1)]
    [SerializeField] private int _amount = 1;

    [Header("Pickup")]
    [Min(0f)]
    [SerializeField] private float _pickupDelay = 0.45f;
    [Min(0f)]
    [SerializeField] private float _magnetDelay = 0.25f;
    [Min(0f)]
    [SerializeField] private float _collectDistance = 0.35f;
    [Min(0f)]
    [SerializeField] private float _magnetDistance = 2f;
    [Min(0f)]
    [SerializeField] private float _magnetStartSpeed = 2f;
    [Min(0f)]
    [SerializeField] private float _magnetSpeed = 9f;
    [Min(0f)]
    [SerializeField] private float _magnetAcceleration = 22f;
    [SerializeField] private bool _lockOnAfterEnteringRange = true;
    [Min(0f)]
    [SerializeField] private float _maxLockedMagnetDistance = 5f;
    [Min(0f)]
    [SerializeField] private float _destroyAfterSeconds = 30f;

    [Header("Visual")]
    [SerializeField] private SpriteRenderer _spriteRenderer;
    [SerializeField] private Sprite _fallbackSprite;
    [SerializeField] private Color _goldColor = new(1f, 0.78f, 0.16f, 1f);

    private bool _collected;
    private bool _isMagnetized;
    private float _spawnTime;
    private float _currentMagnetSpeed;
    private Rigidbody2D _rb;
    private Collider2D _playerCollider;

    public int Amount => _amount;

    public static GoldPickup CreateRuntimePickup(Vector2 position, int amount)
    {
        GameObject pickupObject = new("Gold Pickup");
        pickupObject.transform.position = position;

        GoldPickup pickup = pickupObject.AddComponent<GoldPickup>();
        pickup.SetAmount(amount);
        return pickup;
    }

    private void Awake()
    {
        _spawnTime = Time.time;

        Collider2D pickupCollider = GetComponent<Collider2D>();
        if (pickupCollider == null)
            pickupCollider = gameObject.AddComponent<CircleCollider2D>();

        pickupCollider.isTrigger = true;

        _rb = GetComponent<Rigidbody2D>();
        if (_rb == null)
            _rb = gameObject.AddComponent<Rigidbody2D>();

        _rb.bodyType = RigidbodyType2D.Kinematic;
        _rb.gravityScale = 0f;
        _rb.interpolation = RigidbodyInterpolation2D.Interpolate;

        EnsureVisual();

        if (_destroyAfterSeconds > 0f)
            Destroy(gameObject, _destroyAfterSeconds);
    }

    private void FixedUpdate()
    {
        if (_collected || Player.Instance == null)
            return;

        float aliveTime = Time.time - _spawnTime;
        Vector2 playerPosition = GetPlayerPickupPosition();
        Vector2 currentPosition = _rb != null ? _rb.position : (Vector2)transform.position;
        float distance = Vector2.Distance(currentPosition, playerPosition);

        if (aliveTime >= _pickupDelay && distance <= _collectDistance)
        {
            Collect();
            return;
        }

        if (aliveTime < _magnetDelay)
            return;

        if (!_isMagnetized && distance <= _magnetDistance)
            StartMagnet();

        if (!_isMagnetized)
            return;

        if (_lockOnAfterEnteringRange && _maxLockedMagnetDistance > 0f && distance > _maxLockedMagnetDistance)
        {
            StopMagnet();
            return;
        }

        _currentMagnetSpeed = Mathf.Min(
            _magnetSpeed,
            _currentMagnetSpeed + _magnetAcceleration * Time.fixedDeltaTime);

        Vector2 nextPosition = Vector2.MoveTowards(
            currentPosition,
            playerPosition,
            _currentMagnetSpeed * Time.fixedDeltaTime);

        if (_rb != null)
            _rb.MovePosition(nextPosition);
        else
            transform.position = nextPosition;
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        TryCollectFromCollider(other);
    }

    private void OnTriggerStay2D(Collider2D other)
    {
        TryCollectFromCollider(other);
    }

    public void SetAmount(int amount)
    {
        _amount = Mathf.Max(1, amount);
    }

    private void TryCollectFromCollider(Collider2D other)
    {
        if (_collected || Time.time - _spawnTime < _pickupDelay || other.GetComponentInParent<Player>() == null)
            return;

        Collect();
    }

    private void StartMagnet()
    {
        _isMagnetized = true;
        _currentMagnetSpeed = Mathf.Max(_currentMagnetSpeed, _magnetStartSpeed);
    }

    private void StopMagnet()
    {
        _isMagnetized = false;
        _currentMagnetSpeed = 0f;
    }

    private Vector2 GetPlayerPickupPosition()
    {
        if (Player.Instance == null)
            return transform.position;

        Collider2D playerCollider = GetPlayerCollider();
        if (playerCollider != null)
            return playerCollider.bounds.center;

        return Player.Instance.transform.position;
    }

    private Collider2D GetPlayerCollider()
    {
        if (_playerCollider != null && _playerCollider.enabled)
            return _playerCollider;

        if (Player.Instance == null)
            return null;

        _playerCollider = Player.Instance.GetComponent<Collider2D>();
        if (_playerCollider != null && _playerCollider.enabled && !_playerCollider.isTrigger)
            return _playerCollider;

        Collider2D[] colliders = Player.Instance.GetComponentsInChildren<Collider2D>();
        for (int i = 0; i < colliders.Length; i++)
        {
            if (colliders[i] != null && colliders[i].enabled && !colliders[i].isTrigger)
            {
                _playerCollider = colliders[i];
                return _playerCollider;
            }
        }

        return null;
    }

    private void Collect()
    {
        if (_collected)
            return;

        _collected = true;

        if (GameManager.Instance != null)
            GameManager.Instance.AddGold(_amount);

        Destroy(gameObject);
    }

    private void EnsureVisual()
    {
        if (_spriteRenderer == null)
            _spriteRenderer = GetComponentInChildren<SpriteRenderer>();

        if (_spriteRenderer == null)
            _spriteRenderer = gameObject.AddComponent<SpriteRenderer>();

        if (_spriteRenderer.sprite == null)
            _spriteRenderer.sprite = _fallbackSprite != null ? _fallbackSprite : GetGeneratedGoldSprite();

        _spriteRenderer.color = _goldColor;
    }

    private static Sprite GetGeneratedGoldSprite()
    {
        if (_generatedGoldSprite != null)
            return _generatedGoldSprite;

        const int size = 16;
        Texture2D texture = new(size, size)
        {
            filterMode = FilterMode.Point,
            wrapMode = TextureWrapMode.Clamp
        };

        Color clear = new(0f, 0f, 0f, 0f);
        Color edge = new(0.86f, 0.52f, 0.06f, 1f);
        Color fill = new(1f, 0.78f, 0.16f, 1f);
        Vector2 center = new((size - 1) * 0.5f, (size - 1) * 0.5f);
        float radius = size * 0.42f;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float distance = Vector2.Distance(new Vector2(x, y), center);
                Color color = clear;

                if (distance <= radius)
                    color = distance >= radius - 1.4f ? edge : fill;

                texture.SetPixel(x, y, color);
            }
        }

        texture.Apply();
        _generatedGoldSprite = Sprite.Create(
            texture,
            new Rect(0f, 0f, size, size),
            new Vector2(0.5f, 0.5f),
            size);

        return _generatedGoldSprite;
    }
}
