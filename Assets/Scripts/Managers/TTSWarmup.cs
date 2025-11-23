using System.Runtime.InteropServices;
using UnityEngine;

public static class TTSWarmup
{
#if UNITY_IOS && !UNITY_EDITOR
    [DllImport("__Internal")]
    private static extern void _WarmupTTS();
#endif

    private static bool _warmed;

    /// <summary>
    /// Warm up iOS TTS pipeline early (during splash/loading) to avoid first-use stutter later.
    /// Safe to call multiple times; runs once per app launch.
    /// </summary>
    public static void Warmup()
    {
        if (_warmed)
            return;

#if UNITY_IOS && !UNITY_EDITOR
        try
        {
            _WarmupTTS();
            Debug.Log("[TTSWarmup] Warmup invoked");
        }
        catch (System.Exception ex)
        {
            Debug.LogWarning($"[TTSWarmup] Warmup failed: {ex.Message}");
        }
#else
        Debug.Log("[TTSWarmup] Warmup skipped (not iOS device build)");
#endif

        _warmed = true;
    }
}
