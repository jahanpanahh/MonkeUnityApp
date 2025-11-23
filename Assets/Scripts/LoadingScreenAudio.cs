using UnityEngine;

public class LoadingScreenAudio : MonoBehaviour
{
    public AudioClip loadingSound;

    void Awake()
    {
        Debug.Log("========================================");
        Debug.Log("[LoadingScreenAudio] Awake called");
        Debug.Log($"[LoadingScreenAudio] AudioManager.Instance: {(AudioManager.Instance != null ? "EXISTS" : "NULL")}");
        Debug.Log($"[LoadingScreenAudio] loadingSound: {(loadingSound != null ? loadingSound.name : "NULL")}");

        // Play immediately on scene load
        if (AudioManager.Instance != null && loadingSound != null)
        {
            Debug.Log("[LoadingScreenAudio] Calling AudioManager.PlaySFX");
            AudioManager.Instance.PlaySFX(loadingSound);
        }
        else
        {
            if (AudioManager.Instance == null)
                Debug.LogError("[LoadingScreenAudio] AudioManager.Instance is NULL!");
            if (loadingSound == null)
                Debug.LogError("[LoadingScreenAudio] loadingSound is NULL!");
        }
        Debug.Log("========================================");
    }
}
