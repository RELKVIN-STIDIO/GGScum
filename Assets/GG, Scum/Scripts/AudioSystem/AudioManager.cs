using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [SerializeField] private List<SoundLibrary> libraries = new();
    [SerializeField] private int poolSize = 20;
    [SerializeField, Range(0f, 1f)] private float masterVolume = 1f;

    private readonly Dictionary<string, SoundData> soundMap = new();
    private readonly List<AudioSource> allSources = new();
    private readonly Queue<AudioSource> audioPool = new();

    private void Awake()
    {
        if (Instance != null) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);

        CreatePool();
        LoadLibraries();
    }

    private void CreatePool()
    {
        for (int i = 0; i < poolSize; i++)
        {
            var go = new GameObject($"AudioSource_{i}");
            go.transform.parent = transform;
            var source = go.AddComponent<AudioSource>();
            source.playOnAwake = false;
            audioPool.Enqueue(source);
            allSources.Add(source);
        }
    }

    private void LoadLibraries()
    {
        foreach (var lib in libraries)
        {
            foreach (var sound in lib.sounds)
            {
                if (!soundMap.ContainsKey(sound.soundName))
                    soundMap[sound.soundName] = sound;
                else
                    Debug.LogWarning($"Повтор имени звука: {sound.soundName}");
            }
        }
    }

    private AudioSource GetSource()
    {
        var source = audioPool.Dequeue();
        audioPool.Enqueue(source);
        return source;
    }

    public void Play(string soundName)
    {
        PlayAt(soundName, Camera.main.transform.position);
    }

    public void PlayAt(string soundName, Vector3 position)
    {
        if (!soundMap.TryGetValue(soundName, out var data))
        {
            Debug.LogWarning($"Звук не найден: {soundName}");
            return;
        }

        var source = GetSource();
        source.clip = data.clips[Random.Range(0, data.clips.Length)];
        source.volume = data.volume * masterVolume;
        source.pitch = data.pitch;
        source.loop = data.loop;
        source.spatialBlend = data.spatial ? 1f : 0f;
        source.transform.position = position;

        source.Play();
    }

    public void Stop(string soundName)
    {
        foreach (var s in allSources)
        {
            if (s.isPlaying && s.clip != null && s.clip.name == soundName)
            {
                s.Stop();
                return;
            }
        }
    }

    public void StopAll()
    {
        foreach (var s in allSources)
            s.Stop();
    }

    public bool IsPlaying(string soundName)
    {
        foreach (var s in allSources)
        {
            if (s.isPlaying && s.clip != null && s.clip.name == soundName)
                return true;
        }
        return false;
    }

    public void SetVolume(float volume)
    {
        masterVolume = Mathf.Clamp01(volume);
        foreach (var s in allSources)
        {
            if (s.isPlaying && soundMap.TryGetValue(s.clip.name, out var data))
                s.volume = data.volume * masterVolume;
        }
    }

    public float GetVolume() => masterVolume;

    public void FadeIn(string soundName, float duration = 1f)
    {
        StartCoroutine(FadeInRoutine(soundName, duration));
    }

    public void FadeOut(string soundName, float duration = 1f)
    {
        StartCoroutine(FadeOutRoutine(soundName, duration));
    }

    private IEnumerator FadeInRoutine(string soundName, float duration)
    {
        if (!soundMap.TryGetValue(soundName, out var data))
            yield break;

        var source = GetSource();
        source.clip = data.clips[Random.Range(0, data.clips.Length)];
        source.volume = 0f;
        source.pitch = data.pitch;
        source.loop = data.loop;
        source.spatialBlend = data.spatial ? 1f : 0f;
        source.transform.position = Camera.main.transform.position;
        source.Play();

        float t = 0;
        while (t < duration)
        {
            source.volume = Mathf.Lerp(0f, data.volume * masterVolume, t / duration);
            t += Time.deltaTime;
            yield return null;
        }
        source.volume = data.volume * masterVolume;
    }

    private IEnumerator FadeOutRoutine(string soundName, float duration)
    {
        foreach (var s in allSources)
        {
            if (s.isPlaying && s.clip != null && s.clip.name == soundName)
            {
                float startVol = s.volume;
                float t = 0f;
                while (t < duration)
                {
                    s.volume = Mathf.Lerp(startVol, 0f, t / duration);
                    t += Time.deltaTime;
                    yield return null;
                }
                s.Stop();
                yield break;
            }
        }
    }
}
