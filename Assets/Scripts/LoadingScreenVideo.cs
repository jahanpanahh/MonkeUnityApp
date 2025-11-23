using UnityEngine;
using UnityEngine.Video;
using UnityEngine.UI;
using System;

public class LoadingScreenVideo : MonoBehaviour
{
    public VideoClip loadingVideo;
    public VideoClip transitionVideo;
    private VideoPlayer videoPlayer;
    private RawImage rawImage;
    private AspectRatioFitter aspectRatioFitter;
    private bool hasStartedPlaying = false;
    private bool isTransitioning = false;

    // Event to notify when transition video completes
    public event Action OnTransitionComplete;

    void Awake()
    {
        Debug.Log("[LoadingScreenVideo] Initializing video player");

        // Get or add VideoPlayer component
        videoPlayer = GetComponent<VideoPlayer>();
        if (videoPlayer == null)
        {
            videoPlayer = gameObject.AddComponent<VideoPlayer>();
        }

        // Get RawImage component (should be on this GameObject)
        rawImage = GetComponent<RawImage>();

        // Get or add AspectRatioFitter to maintain video aspect ratio
        aspectRatioFitter = GetComponent<AspectRatioFitter>();
        if (aspectRatioFitter == null)
        {
            aspectRatioFitter = gameObject.AddComponent<AspectRatioFitter>();
        }

        SetupVideoPlayer();
    }

    void SetupVideoPlayer()
    {
        if (loadingVideo == null)
        {
            Debug.LogError("[LoadingScreenVideo] No loading video clip assigned!");
            return;
        }

        if (transitionVideo == null)
        {
            Debug.LogError("[LoadingScreenVideo] No transition video clip assigned!");
            return;
        }

        if (rawImage == null)
        {
            Debug.LogError("[LoadingScreenVideo] RawImage component not found!");
            return;
        }

        // Configure video player for loading video (looping)
        videoPlayer.clip = loadingVideo;
        videoPlayer.playOnAwake = false;
        videoPlayer.isLooping = true; // Loop the loading video
        videoPlayer.renderMode = VideoRenderMode.RenderTexture;
        videoPlayer.waitForFirstFrame = true;

        // Get video dimensions
        uint videoWidth = loadingVideo.width;
        uint videoHeight = loadingVideo.height;

        Debug.Log($"[LoadingScreenVideo] Loading video dimensions: {videoWidth}x{videoHeight}");

        // Create render texture matching video dimensions
        RenderTexture renderTexture = new RenderTexture((int)videoWidth, (int)videoHeight, 0);
        renderTexture.Create();
        videoPlayer.targetTexture = renderTexture;

        // Assign render texture to RawImage
        rawImage.texture = renderTexture;

        // Configure aspect ratio fitter
        aspectRatioFitter.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
        aspectRatioFitter.aspectRatio = (float)videoWidth / (float)videoHeight;

        // Configure audio - use the video's audio track
        videoPlayer.audioOutputMode = VideoAudioOutputMode.Direct;
        videoPlayer.controlledAudioTrackCount = 1;
        videoPlayer.SetDirectAudioVolume(0, 1.0f); // Full volume for track 0

        // Subscribe to events
        videoPlayer.prepareCompleted += OnVideoPrepared;
        videoPlayer.errorReceived += OnVideoError;
        videoPlayer.started += OnVideoStarted;
        videoPlayer.loopPointReached += OnVideoLoopPointReached;

        // Prepare and play
        Debug.Log("[LoadingScreenVideo] Preparing loading video...");
        videoPlayer.Prepare();
    }

    public void StartTransition()
    {
        if (isTransitioning)
        {
            Debug.LogWarning("[LoadingScreenVideo] Transition already in progress");
            return;
        }

        if (transitionVideo == null)
        {
            Debug.LogError("[LoadingScreenVideo] No transition video assigned!");
            OnTransitionComplete?.Invoke();
            return;
        }

        Debug.Log("[LoadingScreenVideo] Starting transition to transition video");
        isTransitioning = true;

        // Stop current video
        videoPlayer.Stop();

        // Switch to transition video (non-looping)
        videoPlayer.clip = transitionVideo;
        videoPlayer.isLooping = false;

        // Update render texture size if needed
        uint videoWidth = transitionVideo.width;
        uint videoHeight = transitionVideo.height;

        Debug.Log($"[LoadingScreenVideo] Transition video dimensions: {videoWidth}x{videoHeight}");

        // Update aspect ratio for transition video
        aspectRatioFitter.aspectRatio = (float)videoWidth / (float)videoHeight;

        // Prepare and play transition video
        videoPlayer.Prepare();
    }

    void OnVideoPrepared(VideoPlayer vp)
    {
        if (isTransitioning)
        {
            Debug.Log("[LoadingScreenVideo] Transition video prepared, starting playback");
            vp.Play();
        }
        else
        {
            Debug.Log("[LoadingScreenVideo] Loading video prepared, starting playback");
            if (!hasStartedPlaying)
            {
                hasStartedPlaying = true;
                vp.Play();
            }
        }
    }

    void OnVideoStarted(VideoPlayer vp)
    {
        if (isTransitioning)
        {
            Debug.Log("[LoadingScreenVideo] Transition video started playing");
        }
        else
        {
            Debug.Log("[LoadingScreenVideo] Loading video started playing (looping)");
        }
    }

    void OnVideoError(VideoPlayer vp, string message)
    {
        Debug.LogError($"[LoadingScreenVideo] Video error: {message}");
    }

    void OnVideoLoopPointReached(VideoPlayer vp)
    {
        if (isTransitioning)
        {
            // Transition video finished (non-looping)
            Debug.Log("[LoadingScreenVideo] Transition video completed!");
            OnTransitionComplete?.Invoke();
        }
        // If not transitioning, this is just the loading video looping (no action needed)
    }

    void OnDestroy()
    {
        // Unsubscribe from events
        if (videoPlayer != null)
        {
            videoPlayer.prepareCompleted -= OnVideoPrepared;
            videoPlayer.errorReceived -= OnVideoError;
            videoPlayer.started -= OnVideoStarted;
            videoPlayer.loopPointReached -= OnVideoLoopPointReached;

            // Clean up render texture
            if (videoPlayer.targetTexture != null)
            {
                videoPlayer.targetTexture.Release();
                Destroy(videoPlayer.targetTexture);
            }
        }
    }
}
