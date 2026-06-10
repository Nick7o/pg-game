using UnityEngine;

public class AudioCue : MonoBehaviour
{
    [SerializeField] private AudioClip[] _clips;
    [SerializeField] private AudioSource _audioSource;
    [Range(0f, 1f)]
    [SerializeField] private float _volume = 1f;
    [SerializeField] private Vector2 _pitchRange = new(0.95f, 1.05f);
    [SerializeField] private bool _spatial = false;
    [Min(0f)]
    [SerializeField] private float _minDistance = 1f;
    [Min(0.01f)]
    [SerializeField] private float _maxDistance = 12f;

    private void Awake()
    {
        if (_audioSource == null)
            _audioSource = GetComponent<AudioSource>();

        if (_audioSource == null)
            _audioSource = gameObject.AddComponent<AudioSource>();

        ConfigureSource(_audioSource);
    }

    public void Play()
    {
        PlayAt(transform.position);
    }

    public void PlayAt(Vector3 position)
    {
        AudioClip clip = GetRandomClip();
        if (clip == null)
            return;

        if (_audioSource != null && Vector3.Distance(transform.position, position) <= 0.01f)
        {
            ConfigureSource(_audioSource);
            _audioSource.pitch = GetRandomPitch();
            _audioSource.PlayOneShot(clip, _volume);
            return;
        }

        PlayClipAt(clip, position, _volume, GetRandomPitch(), _spatial, _minDistance, _maxDistance);
    }

    public AudioClip GetRandomClip()
    {
        if (_clips == null || _clips.Length == 0)
            return null;

        return _clips[Random.Range(0, _clips.Length)];
    }

    public static void PlayClipAt(
        AudioClip clip,
        Vector3 position,
        float volume = 1f,
        float pitch = 1f,
        bool spatial = false,
        float minDistance = 1f,
        float maxDistance = 12f)
    {
        if (clip == null)
            return;

        GameObject audioObject = new("One Shot Audio");
        audioObject.transform.position = position;

        AudioSource source = audioObject.AddComponent<AudioSource>();
        source.clip = clip;
        source.volume = Mathf.Clamp01(volume);
        source.pitch = Mathf.Max(0.01f, pitch);
        source.spatialBlend = spatial ? 1f : 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = minDistance;
        source.maxDistance = maxDistance;
        source.Play();

        Destroy(audioObject, clip.length / source.pitch + 0.1f);
    }

    private void ConfigureSource(AudioSource source)
    {
        source.playOnAwake = false;
        source.spatialBlend = _spatial ? 1f : 0f;
        source.rolloffMode = AudioRolloffMode.Linear;
        source.minDistance = _minDistance;
        source.maxDistance = _maxDistance;
    }

    private float GetRandomPitch()
    {
        float minPitch = Mathf.Max(0.01f, Mathf.Min(_pitchRange.x, _pitchRange.y));
        float maxPitch = Mathf.Max(minPitch, Mathf.Max(_pitchRange.x, _pitchRange.y));
        return Random.Range(minPitch, maxPitch);
    }
}
