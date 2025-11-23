using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

// TODO: REFACTOR - This uses Update() instead of coroutine as a workaround
// See REFACTORING_NOTES.md for details
public class LoadingScreenController : MonoBehaviour
{
    public Slider progressBar;
    public float minimumLoadTime = 2f;
    public LoadingScreenVideo loadingScreenVideo;

    private AsyncOperation asyncLoad;
    private float startTime;
    private float displayProgress = 0f;
    private bool hasLoaded = false;
    private bool isPausingAt100 = false;
    private float pauseTimer = 0f;
    private bool hasStartedTransition = false;

    void Start()
    {
        Debug.Log("[LoadingScreenController] Start called");
        startTime = Time.unscaledTime;

        // Find LoadingScreenVideo if not assigned
        if (loadingScreenVideo == null)
        {
            loadingScreenVideo = FindObjectOfType<LoadingScreenVideo>();
            if (loadingScreenVideo == null)
            {
                Debug.LogError("[LoadingScreenController] LoadingScreenVideo not found in scene!");
            }
        }

        // Subscribe to transition complete event
        if (loadingScreenVideo != null)
        {
            loadingScreenVideo.OnTransitionComplete += OnTransitionVideoComplete;
        }

        // Start loading the main scene asynchronously
        asyncLoad = SceneManager.LoadSceneAsync("MonkeScene");
        asyncLoad.allowSceneActivation = false;
    }

    void Update()
    {
        if (hasLoaded) return;

        float elapsedTime = Time.unscaledTime - startTime;
        float loadProgress = Mathf.Clamp01(asyncLoad.progress / 0.9f);
        float timeProgress = Mathf.Clamp01(elapsedTime / minimumLoadTime);

        // Use the slower of the two progresses for smooth animation
        float targetProgress = Mathf.Min(loadProgress, timeProgress);

        // Smoothly lerp to target progress
        displayProgress = Mathf.Lerp(displayProgress, targetProgress, Time.unscaledDeltaTime * 2f);

        if (progressBar != null)
        {
            progressBar.value = displayProgress;
        }

        // Check if both loading is done and minimum time has passed
        if (asyncLoad.progress >= 0.9f && elapsedTime >= minimumLoadTime)
        {
            if (!isPausingAt100)
            {
                // Fill to 100%
                if (progressBar != null)
                {
                    progressBar.value = 1f;
                }
                isPausingAt100 = true;
                pauseTimer = 0f;
                Debug.Log("[LoadingScreenController] Loading complete, pausing at 100%");
            }
            else
            {
                // Pause at 100% for 0.3 seconds before starting transition
                pauseTimer += Time.unscaledDeltaTime;
                if (pauseTimer >= 0.3f && !hasStartedTransition)
                {
                    Debug.Log("[LoadingScreenController] Starting video transition!");
                    hasStartedTransition = true;

                    // Start transition video
                    if (loadingScreenVideo != null)
                    {
                        loadingScreenVideo.StartTransition();
                    }
                    else
                    {
                        // If no video player, activate scene immediately
                        Debug.LogWarning("[LoadingScreenController] No LoadingScreenVideo found, activating scene immediately");
                        hasLoaded = true;
                        asyncLoad.allowSceneActivation = true;
                    }
                }
            }
        }
    }

    void OnTransitionVideoComplete()
    {
        Debug.Log("[LoadingScreenController] Transition video complete, activating MonkeScene!");
        hasLoaded = true;
        asyncLoad.allowSceneActivation = true;
    }

    void OnDestroy()
    {
        // Unsubscribe from event
        if (loadingScreenVideo != null)
        {
            loadingScreenVideo.OnTransitionComplete -= OnTransitionVideoComplete;
        }
    }
}
