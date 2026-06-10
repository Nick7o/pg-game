using UnityEngine;

[RequireComponent(typeof(AudioSource))]
public class ProximityAudioEmitter : MonoBehaviour
{
    [Header("Loop")]
    [SerializeField] private AudioClip _loopClip;
    [Range(0f, 1f)]
    [SerializeField] private float _loopVolume = 0.6f;
    [SerializeField] private bool _playLoop = true;

    [Header("Random One Shots")]
    [SerializeField] private AudioCue _randomCue;
    [SerializeField] private bool _playRandomSounds = true;
    [SerializeField] private Vector2 _randomInterval = new(4f, 9f);

    [Header("Range")]
    [Min(0f)]
    [SerializeField] private float _fullVolumeRange = 2f;
    [Min(0.01f)]
    [SerializeField] private float _maxRange = 8f;

    private AudioSource _source;
    private float _nextRandomSoundTime;

    private void Awake()
    {
        _source = GetComponent<AudioSource>();
        _source.playOnAwake = false;
        _source.loop = true;
        _source.spatialBlend = 0f;
        _source.clip = _loopClip;
        ScheduleNextRandomSound();
    }

    private void OnEnable()
    {
        ScheduleNextRandomSound();
    }

    private void Update()
    {
        if (Player.Instance == null)
        {
            StopLoop();
            return;
        }

        float distance = Vector2.Distance(transform.position, Player.Instance.transform.position);
        float volumeMultiplier = GetVolumeMultiplier(distance);

        UpdateLoop(volumeMultiplier);
        UpdateRandomSounds(volumeMultiplier);
    }

    private void UpdateLoop(float volumeMultiplier)
    {
        if (!_playLoop || _loopClip == null || volumeMultiplier <= 0f)
        {
            StopLoop();
            return;
        }

        if (_source.clip != _loopClip)
            _source.clip = _loopClip;

        _source.volume = _loopVolume * volumeMultiplier;

        if (!_source.isPlaying)
            _source.Play();
    }

    private void UpdateRandomSounds(float volumeMultiplier)
    {
        if (!_playRandomSounds || _randomCue == null || volumeMultiplier <= 0f || Time.time < _nextRandomSoundTime)
            return;

        _randomCue.PlayAt(transform.position);
        ScheduleNextRandomSound();
    }

    private float GetVolumeMultiplier(float distance)
    {
        if (distance >= _maxRange)
            return 0f;

        if (distance <= _fullVolumeRange)
            return 1f;

        return Mathf.InverseLerp(_maxRange, _fullVolumeRange, distance);
    }

    private void StopLoop()
    {
        if (_source != null && _source.isPlaying)
            _source.Stop();
    }

    private void ScheduleNextRandomSound()
    {
        float minInterval = Mathf.Max(0.1f, Mathf.Min(_randomInterval.x, _randomInterval.y));
        float maxInterval = Mathf.Max(minInterval, Mathf.Max(_randomInterval.x, _randomInterval.y));
        _nextRandomSoundTime = Time.time + Random.Range(minInterval, maxInterval);
    }

    private void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.cyan;
        Gizmos.DrawWireSphere(transform.position, _fullVolumeRange);

        Gizmos.color = Color.blue;
        Gizmos.DrawWireSphere(transform.position, _maxRange);
    }
}
