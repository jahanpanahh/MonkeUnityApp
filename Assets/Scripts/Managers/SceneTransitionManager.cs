using UnityEngine;
using UnityEngine.SceneManagement;
using System.Collections;

public class SceneTransitionManager : MonoBehaviour
{
    private static SceneTransitionManager _instance;
    public CanvasGroup fadeCanvasGroup;

    public float fadeDuration = 0.5f;

    void Awake()
    {
        if (_instance == null)
        {
            _instance = this;
            DontDestroyOnLoad(gameObject);
            Debug.Log("[SceneTransitionManager] Initialized and set to DontDestroyOnLoad");
        }
        else
        {
            Debug.Log("[SceneTransitionManager] Duplicate instance destroyed");
            Destroy(gameObject);
            return;
        }
    }

    public void LoadSceneWithDelay(string sceneName, float delay)
    {
        Debug.Log($"[SceneTransitionManager] LoadSceneWithDelay called: {sceneName} after {delay} seconds");
        StartCoroutine(LoadSceneAfterDelay(sceneName, delay));
    }

    public void LoadSceneWithFade(string sceneName)
    {
        StartCoroutine(LoadSceneWithFadeCoroutine(sceneName));
    }

    IEnumerator LoadSceneAfterDelay(string sceneName, float delay)
    {
        Debug.Log($"[SceneTransitionManager] Waiting {delay} seconds...");
        // Use WaitForSecondsRealtime to ignore Time.timeScale
        yield return new WaitForSecondsRealtime(delay);
        Debug.Log($"[SceneTransitionManager] Loading scene: {sceneName}");
        SceneManager.LoadScene(sceneName);
    }

    IEnumerator LoadSceneWithFadeCoroutine(string sceneName)
    {
        // Fade out
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(Fade(1));
        }

        // Load scene
        SceneManager.LoadScene(sceneName);

        // Fade in
        if (fadeCanvasGroup != null)
        {
            yield return StartCoroutine(Fade(0));
        }
    }

    IEnumerator Fade(float targetAlpha)
    {
        float startAlpha = fadeCanvasGroup.alpha;
        float elapsed = 0;

        while (elapsed < fadeDuration)
        {
            elapsed += Time.deltaTime;
            fadeCanvasGroup.alpha = Mathf.Lerp(startAlpha, targetAlpha, elapsed / fadeDuration);
            yield return null;
        }

        fadeCanvasGroup.alpha = targetAlpha;
    }

    public static SceneTransitionManager Instance
    {
        get { return _instance; }
    }
}
