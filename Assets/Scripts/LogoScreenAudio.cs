using UnityEngine;

public class LogoScreenAudio : MonoBehaviour
{
    public AudioClip logoSound;

    void Awake()
    {
        Debug.Log("========================================");
        Debug.Log("[LogoScreenAudio] Awake called");
        Debug.Log($"[LogoScreenAudio] logoSound assigned: {(logoSound != null ? logoSound.name : "NULL")}");

        // Warm up iOS TTS as early as possible (no other audio here) to avoid first-use stutter later
        TTSWarmup.Warmup();

        // Force load the audio clip immediately
        if (logoSound != null)
        {
            logoSound.LoadAudioData();
            Debug.Log("[LogoScreenAudio] Audio data loaded");
        }
        else
        {
            Debug.LogError("[LogoScreenAudio] logoSound is NULL! Please assign in Inspector.");
        }
        Debug.Log("========================================");
    }

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("[LogoScreenAudio] Start called");
        Debug.Log($"[LogoScreenAudio] AudioManager.Instance: {(AudioManager.Instance != null ? "EXISTS" : "NULL")}");
        Debug.Log($"[LogoScreenAudio] logoSound: {(logoSound != null ? logoSound.name : "NULL")}");

        if (AudioManager.Instance != null && logoSound != null)
        {
            // Small delay to ensure audio system is ready on first launch
            StartCoroutine(PlayLogoSoundDelayed());
        }
        else
        {
            if (AudioManager.Instance == null)
                Debug.LogError("[LogoScreenAudio] AudioManager.Instance is NULL!");
            if (logoSound == null)
                Debug.LogError("[LogoScreenAudio] logoSound is NULL!");
        }
        Debug.Log("========================================");
    }

    System.Collections.IEnumerator PlayLogoSoundDelayed()
    {
        // Wait a tiny bit to ensure audio system is fully initialized
        yield return new WaitForSeconds(0.1f);

        Debug.Log("[LogoScreenAudio] Calling AudioManager.PlaySFX");
        AudioManager.Instance.PlaySFX(logoSound);
    }
}
