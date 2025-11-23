using UnityEngine;

public class MonkeSceneAudio : MonoBehaviour
{
    public AudioClip backgroundAmbience;

    void Start()
    {
        Debug.Log("========================================");
        Debug.Log("[MonkeSceneAudio] Start called");
        Debug.Log($"[MonkeSceneAudio] AudioManager.Instance: {(AudioManager.Instance != null ? "EXISTS" : "NULL")}");
        Debug.Log($"[MonkeSceneAudio] backgroundAmbience: {(backgroundAmbience != null ? backgroundAmbience.name : "NULL")}");

        if (AudioManager.Instance != null && backgroundAmbience != null)
        {
            Debug.Log("[MonkeSceneAudio] Calling AudioManager.PlayMusic");
            AudioManager.Instance.PlayMusic(backgroundAmbience, true);
        }
        else
        {
            if (AudioManager.Instance == null)
                Debug.LogError("[MonkeSceneAudio] AudioManager.Instance is NULL!");
            if (backgroundAmbience == null)
                Debug.LogError("[MonkeSceneAudio] backgroundAmbience is NULL!");
        }
        Debug.Log("========================================");
    }
}
