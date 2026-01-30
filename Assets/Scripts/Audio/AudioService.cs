using UnityEngine;
using UnityEngine.Audio;

public class AudioService : MonoBehaviour
{
    public static AudioService Instance { get; private set; }

    [Header("Library")]
    public AudioLibrarySO library;

    [Header("Mixer Groups (optional)")]
    public AudioMixerGroup sfxGroup;
    public AudioMixerGroup ambienceGroup;

    [Header("One-shot Pool")]
    [Tooltip("How many SFX can overlap at once.")]
    public int sfxVoices = 12;

    [Header("Sources")]
    public AudioSource ambienceSource;

    private AudioSource[] _sfxPool;
    private int _nextIndex;

    private void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        BuildPool();
    }

    private void BuildPool()
    {
        // Create a child container to keep hierarchy clean
        var poolRoot = new GameObject("SFX_Pool");
        poolRoot.transform.SetParent(transform);

        _sfxPool = new AudioSource[Mathf.Max(1, sfxVoices)];

        for (int i = 0; i < _sfxPool.Length; i++)
        {
            var go = new GameObject($"SFX_{i:00}");
            go.transform.SetParent(poolRoot.transform);

            var src = go.AddComponent<AudioSource>();
            src.playOnAwake = false;
            src.loop = false;
            src.spatialBlend = 0f; // 2D sound
            if (sfxGroup != null) src.outputAudioMixerGroup = sfxGroup;

            _sfxPool[i] = src;
        }
    }

    public void Play(SfxEvent id)
    {
        if (library == null) return;
        if (!library.TryGet(id, out var e)) return;
        if (e == null || e.clips == null || e.clips.Count == 0) return;

        var clip = e.clips[Random.Range(0, e.clips.Count)];
        if (clip == null) return;

        var src = GetNextFreeSource();
        if (src == null) return;

        src.pitch = Random.Range(e.pitchMin, e.pitchMax);
        src.volume = 1f;

        // PlayOneShot allows overlap even on same source, but we prefer separate voices anyway
        src.PlayOneShot(clip, e.volume);
    }

    private AudioSource GetNextFreeSource()
    {
        // 1) Find a free one starting from the rolling index
        for (int i = 0; i < _sfxPool.Length; i++)
        {
            int idx = (_nextIndex + i) % _sfxPool.Length;
            if (!_sfxPool[idx].isPlaying)
            {
                _nextIndex = (idx + 1) % _sfxPool.Length;
                return _sfxPool[idx];
            }
        }

        // 2) If all busy, steal the oldest (simple voice stealing)
        var steal = _sfxPool[_nextIndex];
        _nextIndex = (_nextIndex + 1) % _sfxPool.Length;
        return steal;
    }

    public void SetAmbience(AudioClip clip, float volume = 0.6f)
    {
        if (ambienceSource == null) return;

        ambienceSource.outputAudioMixerGroup = ambienceGroup;
        ambienceSource.clip = clip;
        ambienceSource.loop = true;
        ambienceSource.volume = volume;

        if (clip != null) ambienceSource.Play();
        else ambienceSource.Stop();
    }
}
