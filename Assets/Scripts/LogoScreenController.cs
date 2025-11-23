using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

// TODO: REFACTOR - This uses Update() timer as a workaround
// See REFACTORING_NOTES.md for details
public class LogoScreenController : MonoBehaviour
{
    public float displayDuration = 3f;
    public CanvasGroup canvasGroup; // Assign the Canvas CanvasGroup in inspector
    private float timer = 0f;
    private bool hasLoaded = false;
    private float audioEndTime = 0f;
    private LogoScreenAudio logoScreenAudio;

    void Start()
    {
        Debug.Log("[LogoScreenController] Start called");

        // Get reference to LogoScreenAudio to access the audio clip
        logoScreenAudio = GetComponent<LogoScreenAudio>();

        if (logoScreenAudio != null && logoScreenAudio.logoSound != null)
        {
            // Calculate when the audio will finish (clip length)
            audioEndTime = logoScreenAudio.logoSound.length;
            Debug.Log($"[LogoScreenController] Logo audio duration: {audioEndTime} seconds");
        }
        else
        {
            Debug.LogWarning("[LogoScreenController] No logo audio found, using default duration");
            audioEndTime = displayDuration;
        }

        // Ensure canvas is fully visible at start
        if (canvasGroup != null)
        {
            canvasGroup.alpha = 1f;
        }
    }

    void Update()
    {
        if (hasLoaded) return;

        timer += Time.unscaledDeltaTime;

        // Log every second for debugging
        if (Mathf.FloorToInt(timer) != Mathf.FloorToInt(timer - Time.unscaledDeltaTime))
        {
            Debug.Log($"[LogoScreenController] Timer: {timer:F2}s / {audioEndTime:F2}s");
        }

        // Transition when audio duration has elapsed
        if (timer >= audioEndTime)
        {
            Debug.Log($"[LogoScreenController] Audio duration complete at {timer} seconds, transitioning to LoadingScreen");
            hasLoaded = true;
            StartCoroutine(LoadNextSceneAdditive());
        }
    }

    IEnumerator LoadNextSceneAdditive()
    {
        // Load LoadingScreen additively (both scenes will be visible)
        Debug.Log("[LogoScreenController] Loading LoadingScreen additively");
        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync("LoadingScreen", LoadSceneMode.Additive);

        // Wait until LoadingScreen is loaded
        while (!asyncLoad.isDone)
        {
            yield return null;
        }

        Debug.Log("[LogoScreenController] LoadingScreen loaded, starting fade out");

        // Fade out LogoScreen canvas
        if (canvasGroup != null)
        {
            float fadeDuration = 0.3f;
            float elapsed = 0f;

            while (elapsed < fadeDuration)
            {
                elapsed += Time.unscaledDeltaTime;
                canvasGroup.alpha = Mathf.Lerp(1f, 0f, elapsed / fadeDuration);
                yield return null;
            }

            canvasGroup.alpha = 0f;
        }

        Debug.Log("[LogoScreenController] Fade complete, unloading LogoScreen");

        // Small delay to ensure LoadingScreen is fully visible
        yield return new WaitForSeconds(0.1f);

        // Unload LogoScreen
        SceneManager.UnloadSceneAsync("LogoScreen");

        Debug.Log("[LogoScreenController] LogoScreen unloaded");
    }
}
