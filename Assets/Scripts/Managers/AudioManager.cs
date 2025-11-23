using UnityEngine;
using System;
#if UNITY_IOS && !UNITY_EDITOR
using System.Runtime.InteropServices;
#endif

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public AudioSource musicSource;
    public AudioSource sfxSource;

    [Header("Volume Settings")]
    [SerializeField, Range(0f, 1f)] private float musicVolume = 0.3f;
    [SerializeField, Range(0f, 1f)] private float sfxVolume = 1.0f;

#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _InitAudioSessionPlayback();
#endif

    void Awake()
    {
        Debug.Log("========================================");
        Debug.Log("[AudioManager] Awake called");
        Debug.Log($"[AudioManager] Platform: {Application.platform}");

#if UNITY_IOS && !UNITY_EDITOR
        try
        {
            _InitAudioSessionPlayback();
            Debug.Log("[AudioManager] Initialized iOS audio session for playback");
        }
        catch (Exception ex)
        {
            Debug.LogError($"[AudioManager] Failed to init iOS audio session: {ex.Message}");
        }

        AudioSettings.Mobile.stopAudioOutputOnMute = false;
        Debug.Log($"[AudioManager] stopAudioOutputOnMute set to {AudioSettings.Mobile.stopAudioOutputOnMute}");
#endif

        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[AudioManager] Instance created and set to DontDestroyOnLoad");

            // Verify audio sources are assigned
            if (musicSource == null)
            {
                Debug.LogError("[AudioManager] musicSource is NULL! Please assign in Inspector.");
            }
            else
            {
                Debug.Log($"[AudioManager] musicSource assigned: {musicSource.name}");
            }

            if (sfxSource == null)
            {
                Debug.LogError("[AudioManager] sfxSource is NULL! Please assign in Inspector.");
            }
            else
            {
                Debug.Log($"[AudioManager] sfxSource assigned: {sfxSource.name}");
            }

            // Set initial volumes
            if (musicSource != null)
            {
                musicSource.volume = musicVolume;
                Debug.Log($"[AudioManager] Music volume set to {musicVolume}");
            }
            if (sfxSource != null)
            {
                sfxSource.volume = sfxVolume;
                Debug.Log($"[AudioManager] SFX volume set to {sfxVolume}");
            }
        }
        else
        {
            Debug.Log("[AudioManager] Duplicate instance found, destroying");
            Destroy(gameObject);
            return;
        }
        Debug.Log("========================================");
    }

    public void PlayMusic(AudioClip clip, bool loop = true)
    {
        Debug.Log($"[AudioManager] PlayMusic called - Clip: {(clip != null ? clip.name : "NULL")}");

        if (clip == null)
        {
            Debug.LogError("[AudioManager] Cannot play NULL music clip!");
            return;
        }

        if (musicSource == null)
        {
            Debug.LogError("[AudioManager] musicSource is NULL!");
            return;
        }

        if (musicSource.clip == clip && musicSource.isPlaying)
        {
            Debug.Log("[AudioManager] Already playing this music, skipping");
            return; // Already playing this music
        }

        musicSource.clip = clip;
        musicSource.loop = loop;
        musicSource.Play();
        Debug.Log($"[AudioManager] Music started playing: {clip.name}");
    }

    public void StopMusic()
    {
        musicSource.Stop();
    }

    public void PlaySFX(AudioClip clip)
    {
        Debug.Log($"[AudioManager] PlaySFX called - Clip: {(clip != null ? clip.name : "NULL")}");

        if (clip == null)
        {
            Debug.LogError("[AudioManager] Cannot play NULL SFX clip!");
            return;
        }

        if (sfxSource == null)
        {
            Debug.LogError("[AudioManager] sfxSource is NULL!");
            return;
        }

        sfxSource.PlayOneShot(clip);
        Debug.Log($"[AudioManager] SFX played: {clip.name}");
    }

    public void FadeOutMusic(float duration)
    {
        StartCoroutine(FadeOutCoroutine(duration));
    }

    public void FadeInMusic(float duration)
    {
        StartCoroutine(FadeInCoroutine(duration));
    }

    System.Collections.IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = musicSource.volume;
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(startVolume, 0, elapsed / duration);
            yield return null;
        }

        musicSource.volume = 0;
        musicSource.Stop();
    }

    System.Collections.IEnumerator FadeInCoroutine(float duration)
    {
        musicSource.volume = 0;
        musicSource.Play();
        float elapsed = 0;

        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0, musicVolume, elapsed / duration);
            yield return null;
        }

        musicSource.volume = musicVolume;
    }

    public static AudioManager Instance
    {
        get { return _instance; }
    }
}
