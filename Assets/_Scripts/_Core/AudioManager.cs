using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TextQuestReader.Settings;
using UnityEngine;
using UnityEngine.Networking;

public class AudioManager : MonoBehaviour
{
    [SerializeField] private AudioSource musicSourceA;
    [SerializeField] private AudioSource musicSourceB;
    [SerializeField] private AudioSource sfxSource;
    [SerializeField] private float musicFadeDuration = 0.5f;
    [SerializeField] private AudioClip clickClip;
    [SerializeField] private AudioClip hoverClip;

    public static AudioManager Instance;

    private Coroutine musicCoroutine;
    private string currentMusicName;
    private float defaultMusicVolume;
    private float baseMusicVolume;
    private float baseSfxVolume;

    private AudioSource activeMusicSource;
    private AudioSource inactiveMusicSource;

    private readonly Dictionary<string, AudioClip> audioCache = new();
    private string remoteQuestFolder;

    private static readonly string[] audioExtensions =
    {
        ".ogg",
        ".mp3",
        ".wav",
        ".aif",
        ".aiff"
    };

    public string CurrentMusicName => currentMusicName;

    private void Awake()
    {
        Instance = this;

        activeMusicSource = musicSourceA;
        inactiveMusicSource = musicSourceB;

        baseMusicVolume = musicSourceA.volume <= 0f ? 0.6f : musicSourceA.volume;
        baseSfxVolume = sfxSource.volume <= 0f ? 1f : sfxSource.volume;
        defaultMusicVolume = baseMusicVolume * GameSettings.EffectiveMusicVolume;

        musicSourceA.playOnAwake = false;
        musicSourceB.playOnAwake = false;
        sfxSource.playOnAwake = false;

        musicSourceA.loop = true;
        musicSourceB.loop = true;

        musicSourceA.volume = 0f;
        musicSourceB.volume = 0f;

        ApplyVolumeSettings();
        GameSettings.AudioVolumesChanged += ApplyVolumeSettings;
    }

    private void OnDestroy()
    {
        GameSettings.AudioVolumesChanged -= ApplyVolumeSettings;
    }

    public void ApplyVolumeSettings()
    {
        defaultMusicVolume = Mathf.Clamp01(baseMusicVolume * GameSettings.EffectiveMusicVolume);
        sfxSource.volume = Mathf.Clamp01(baseSfxVolume * GameSettings.EffectiveSfxVolume);

        if (activeMusicSource != null && activeMusicSource.isPlaying)
            activeMusicSource.volume = defaultMusicVolume;
    }

    public void PlayMusic(string musicName, string questName, bool stoppable)
    {
        if (string.IsNullOrEmpty(musicName))
        {
            if (stoppable)
                StopMusic();

            return;
        }

        if (activeMusicSource.isPlaying && currentMusicName == musicName)
            return;

        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        musicCoroutine = StartCoroutine(CrossfadeToMusic(musicName, questName));
    }

    public void PlayMusicClip(AudioClip clip, bool stoppable)
    {
        if (clip == null)
        {
            if (stoppable)
                StopMusic();

            return;
        }

        if (activeMusicSource.isPlaying && activeMusicSource.clip == clip)
            return;

        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        musicCoroutine = StartCoroutine(CrossfadeToClip(clip));
    }

    private IEnumerator CrossfadeToMusic(string musicName, string questName)
    {
        AudioClip newClip = null;

        yield return StartCoroutine(LoadAudioClip("Musics", musicName, questName, clip => newClip = clip));

        if (newClip == null)
        {
            musicCoroutine = null;
            yield break;
        }

        if (activeMusicSource.isPlaying && activeMusicSource.clip == newClip && currentMusicName == musicName)
        {
            musicCoroutine = null;
            yield break;
        }

        inactiveMusicSource.Stop();
        inactiveMusicSource.clip = newClip;
        inactiveMusicSource.loop = true;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.Play();

        float fromActive = activeMusicSource.isPlaying ? activeMusicSource.volume : 0f;

        if (musicFadeDuration <= 0f)
        {
            if (activeMusicSource.isPlaying)
            {
                activeMusicSource.Stop();
                activeMusicSource.clip = null;
                activeMusicSource.volume = 0f;
            }

            inactiveMusicSource.volume = defaultMusicVolume;
        }
        else
        {
            float time = 0f;

            while (time < musicFadeDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / musicFadeDuration);

                if (activeMusicSource.isPlaying)
                    activeMusicSource.volume = Mathf.Lerp(fromActive, 0f, t);

                inactiveMusicSource.volume = Mathf.Lerp(0f, defaultMusicVolume, t);

                yield return null;
            }

            if (activeMusicSource.isPlaying)
            {
                activeMusicSource.Stop();
                activeMusicSource.clip = null;
                activeMusicSource.volume = 0f;
            }

            inactiveMusicSource.volume = defaultMusicVolume;
        }

        SwapMusicSources();
        currentMusicName = musicName;
        musicCoroutine = null;
    }

    private IEnumerator CrossfadeToClip(AudioClip newClip)
    {
        inactiveMusicSource.Stop();
        inactiveMusicSource.clip = newClip;
        inactiveMusicSource.loop = true;
        inactiveMusicSource.volume = 0f;
        inactiveMusicSource.Play();

        float fromActive = activeMusicSource.isPlaying ? activeMusicSource.volume : 0f;

        if (musicFadeDuration <= 0f)
        {
            if (activeMusicSource.isPlaying)
            {
                activeMusicSource.Stop();
                activeMusicSource.clip = null;
                activeMusicSource.volume = 0f;
            }

            inactiveMusicSource.volume = defaultMusicVolume;
        }
        else
        {
            float time = 0f;

            while (time < musicFadeDuration)
            {
                time += Time.deltaTime;
                float t = Mathf.Clamp01(time / musicFadeDuration);

                if (activeMusicSource.isPlaying)
                    activeMusicSource.volume = Mathf.Lerp(fromActive, 0f, t);

                inactiveMusicSource.volume = Mathf.Lerp(0f, defaultMusicVolume, t);

                yield return null;
            }

            if (activeMusicSource.isPlaying)
            {
                activeMusicSource.Stop();
                activeMusicSource.clip = null;
                activeMusicSource.volume = 0f;
            }

            inactiveMusicSource.volume = defaultMusicVolume;
        }

        SwapMusicSources();
        currentMusicName = newClip.name;
        musicCoroutine = null;
    }

    public void StopMusic()
    {
        if (musicCoroutine != null)
            StopCoroutine(musicCoroutine);

        musicCoroutine = StartCoroutine(StopMusicSmooth());
    }

    private IEnumerator StopMusicSmooth()
    {
        AudioSource sourceToStop = activeMusicSource;

        if (sourceToStop.isPlaying && sourceToStop.volume > 0f)
            yield return FadeVolume(sourceToStop, sourceToStop.volume, 0f, musicFadeDuration);

        sourceToStop.Stop();
        sourceToStop.clip = null;
        sourceToStop.volume = 0f;

        inactiveMusicSource.Stop();
        inactiveMusicSource.clip = null;
        inactiveMusicSource.volume = 0f;

        currentMusicName = null;
        musicCoroutine = null;
    }

    private void SwapMusicSources()
    {
        AudioSource temp = activeMusicSource;
        activeMusicSource = inactiveMusicSource;
        inactiveMusicSource = temp;
    }

    public void PlaySfx(string sfxName, string questName)
    {
        if (string.IsNullOrEmpty(sfxName))
            return;

        StartCoroutine(PlaySfxRoutine(sfxName, questName));
    }

    private IEnumerator PlaySfxRoutine(string sfxName, string questName)
    {
        AudioClip clip = null;

        yield return StartCoroutine(LoadAudioClip("Sounds", sfxName, questName, loadedClip => clip = loadedClip));

        if (clip == null)
            yield break;

        sfxSource.PlayOneShot(clip);
    }

    public void PlaySfx(SoundType soundType)
    {
        switch (soundType)
        {
            case SoundType.Click:
                if (clickClip != null) sfxSource.PlayOneShot(clickClip);
                break;
            case SoundType.Hover:
                if (hoverClip != null) sfxSource.PlayOneShot(hoverClip);
                break;
        }
    }

    public void PlaySfxClip(AudioClip clip, float volumeScale = 1f)
    {
        if (clip == null) return;
        sfxSource.PlayOneShot(clip, Mathf.Clamp01(volumeScale));
    }

    public void StopSfx()
    {
        sfxSource.Stop();
    }

    private IEnumerator LoadAudioClip(string folderName, string audioNameWithoutExtensionOrWithIt, string questName, Action<AudioClip> onLoaded)
    {
        string resolvedPath = FindAudioPath(folderName, audioNameWithoutExtensionOrWithIt, questName);

        if (string.IsNullOrEmpty(resolvedPath))
        {
            Debug.LogWarning($"Audio not found. Quest: {questName}, Folder: {folderName}, Name: {audioNameWithoutExtensionOrWithIt}");
            onLoaded?.Invoke(null);
            yield break;
        }

        string cacheKey = resolvedPath;

        if (audioCache.TryGetValue(cacheKey, out AudioClip cachedClip) && cachedClip != null)
        {
            onLoaded?.Invoke(cachedClip);
            yield break;
        }

        string url = ToFileUrl(resolvedPath);
        AudioType audioType = GetAudioTypeFromExtension(resolvedPath);

        using UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(url, audioType);
        yield return request.SendWebRequest();

        if (request.result != UnityWebRequest.Result.Success)
        {
            Debug.LogWarning($"Failed to load audio: {resolvedPath}\n{request.error}");
            onLoaded?.Invoke(null);
            yield break;
        }

        AudioClip clip = DownloadHandlerAudioClip.GetContent(request);

        if (clip == null)
        {
            Debug.LogWarning($"Failed to decode audio: {resolvedPath}");
            onLoaded?.Invoke(null);
            yield break;
        }

        clip.name = Path.GetFileNameWithoutExtension(resolvedPath);
        audioCache[cacheKey] = clip;

        onLoaded?.Invoke(clip);
    }

    private string FindAudioPath(string folderName, string audioNameWithoutExtensionOrWithIt, string questName)
    {
        if (!string.IsNullOrEmpty(remoteQuestFolder))
        {
            string remoteFolder = Path.Combine(remoteQuestFolder, folderName);
            string remotePath = FindAudioPathInFolder(remoteFolder, audioNameWithoutExtensionOrWithIt);

            if (!string.IsNullOrEmpty(remotePath))
                return remotePath;
        }

        string persistentFolder = Path.Combine(Application.persistentDataPath, "Quests", questName, folderName);
        string streamingFolder = Path.Combine(Application.streamingAssetsPath, "Quests", questName, folderName);

        string path = FindAudioPathInFolder(persistentFolder, audioNameWithoutExtensionOrWithIt);

        if (!string.IsNullOrEmpty(path))
            return path;

        return FindAudioPathInFolder(streamingFolder, audioNameWithoutExtensionOrWithIt);
    }

    private string FindAudioPathInFolder(string baseFolder, string audioNameWithoutExtensionOrWithIt)
    {
        if (!Directory.Exists(baseFolder) || string.IsNullOrWhiteSpace(audioNameWithoutExtensionOrWithIt))
            return null;

        string name = Path.GetFileNameWithoutExtension(audioNameWithoutExtensionOrWithIt);

        foreach (string ext in audioExtensions)
        {
            string fullPath = Path.Combine(baseFolder, name + ext);

            if (File.Exists(fullPath))
                return fullPath;
        }

        return null;
    }

    private static string ToFileUrl(string path)
    {
        string normalizedPath = path.Replace("\\", "/");

#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        return "file:///" + normalizedPath;
#else
        return "file://" + normalizedPath;
#endif
    }

    private static AudioType GetAudioTypeFromExtension(string path)
    {
        string extension = Path.GetExtension(path).ToLowerInvariant();

        return extension switch
        {
            ".mp3" => AudioType.MPEG,
            ".wav" => AudioType.WAV,
            ".ogg" => AudioType.OGGVORBIS,
            ".aif" => AudioType.AIFF,
            ".aiff" => AudioType.AIFF,
            _ => AudioType.UNKNOWN
        };
    }

    private IEnumerator FadeVolume(AudioSource source, float from, float to, float duration)
    {
        if (duration <= 0f)
        {
            source.volume = to;
            yield break;
        }

        float time = 0f;
        source.volume = from;

        while (time < duration)
        {
            time += Time.deltaTime;
            float t = Mathf.Clamp01(time / duration);
            source.volume = Mathf.Lerp(from, to, t);
            yield return null;
        }

        source.volume = to;
    }

    public void SetRemoteQuestFolder(string folderPath) => remoteQuestFolder = folderPath;

    public void ClearRemoteQuestFolder()
    {
        if (!string.IsNullOrEmpty(remoteQuestFolder))
            ClearLoadedAudioFromFolder(remoteQuestFolder);

        remoteQuestFolder = null;
    }

    private void ClearLoadedAudioFromFolder(string folderPath)
    {
        string normalizedFolder = folderPath.Replace("\\", "/");
        List<string> keysToRemove = new();

        foreach (var pair in audioCache)
        {
            string normalizedKey = pair.Key.Replace("\\", "/");

            if (!normalizedKey.StartsWith(normalizedFolder, StringComparison.OrdinalIgnoreCase))
                continue;

            if (pair.Value != null)
                Destroy(pair.Value);

            keysToRemove.Add(pair.Key);
        }

        foreach (string key in keysToRemove)
            audioCache.Remove(key);
    }

    public void ClearLoadedQuestAudio(string questName)
    {
        if (string.IsNullOrEmpty(questName))
            return;

        List<string> keysToRemove = new();

        foreach (var pair in audioCache)
        {
            string normalized = pair.Key.Replace("\\", "/");
            string questPart = "/Quests/" + questName + "/";

            if (normalized.Contains(questPart, StringComparison.OrdinalIgnoreCase))
            {
                if (pair.Value != null)
                    Destroy(pair.Value);

                keysToRemove.Add(pair.Key);
            }
        }

        foreach (string key in keysToRemove)
            audioCache.Remove(key);
    }

    public void ClearAllLoadedQuestAudio()
    {
        foreach (var pair in audioCache)
        {
            if (pair.Value != null)
                Destroy(pair.Value);
        }

        audioCache.Clear();
    }
}

public enum SoundType
{
    None,
    Click,
    Hover
}