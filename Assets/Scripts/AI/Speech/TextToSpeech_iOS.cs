using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace Monke.AI.Speech
{
    /// <summary>
    /// iOS implementation of Text-to-Speech using Apple's AVSpeechSynthesizer
    /// Uses kid-friendly voice (Samantha) with optimized parameters
    /// </summary>
    public class TextToSpeech_iOS : MonoBehaviour, ITextToSpeech
    {
        // Events from interface
        public event Action OnSpeechFinished;
        public event Action<string> OnError;

        private bool isSpeaking = false;
        private Action currentCompletionCallback;

#if UNITY_IOS && !UNITY_EDITOR
        // Native iOS function imports
        [DllImport("__Internal")]
        private static extern void _InitTextToSpeech();

        [DllImport("__Internal")]
        private static extern void _Speak(string text, TTSCompletionCallback callback);

        [DllImport("__Internal")]
        private static extern void _StopSpeaking();

        [DllImport("__Internal")]
        private static extern bool _IsSpeaking();

        // Callback delegate that matches native side
        private delegate void TTSCompletionCallback();

        // Static callback function (must be static for native calls)
        [MonoPInvokeCallback(typeof(TTSCompletionCallback))]
        private static void OnTTSComplete()
        {
            // Forward to instance method on main thread
            if (Instance != null)
            {
                Instance.HandleSpeechComplete();
            }
        }

        private static TextToSpeech_iOS Instance;
#endif

        public bool IsSpeaking
        {
            get
            {
#if UNITY_IOS && !UNITY_EDITOR
                return _IsSpeaking();
#else
                return isSpeaking;
#endif
            }
        }

        void Awake()
        {
#if UNITY_IOS && !UNITY_EDITOR
            Instance = this;
            _InitTextToSpeech();
            Debug.Log("[TextToSpeech_iOS] Initialized");
#else
            Debug.Log("[TextToSpeech_iOS] Running in editor mode - TTS will be simulated");
#endif
        }

        /// <summary>
        /// Speak the given text using iOS native TTS
        /// Automatically filters out stage directions (text between asterisks)
        /// </summary>
        public void Speak(string text, Action onComplete = null)
        {
            if (string.IsNullOrEmpty(text))
            {
                Debug.LogWarning("[TextToSpeech_iOS] Empty text provided");
                OnError?.Invoke("Empty text provided");
                return;
            }

            currentCompletionCallback = onComplete;

#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log($"[TextToSpeech_iOS] Starting to speak: {text}");
            isSpeaking = true;
            _Speak(text, OnTTSComplete);
#else
            Debug.Log($"[TextToSpeech_iOS] Would speak (simulated): {text}");
            // Simulate completion after a delay in editor
            isSpeaking = true;
            // Simulate roughly 1 second per 10 characters
            float duration = Mathf.Max(1f, text.Length / 10f);
            Invoke(nameof(SimulateComplete), duration);
#endif
        }

        /// <summary>
        /// Stop current speech playback immediately
        /// </summary>
        public void Stop()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (isSpeaking)
            {
                Debug.Log("[TextToSpeech_iOS] Stopping speech");
                _StopSpeaking();
                isSpeaking = false;
                currentCompletionCallback = null;
            }
#else
            Debug.Log("[TextToSpeech_iOS] Stopped speaking (simulated)");
            CancelInvoke(nameof(SimulateComplete));
            isSpeaking = false;
            currentCompletionCallback = null;
#endif
        }

        /// <summary>
        /// Handle speech completion callback from native plugin
        /// </summary>
        private void HandleSpeechComplete()
        {
            Debug.Log("[TextToSpeech_iOS] Speech completed");
            isSpeaking = false;

            // Fire completion callback if provided
            currentCompletionCallback?.Invoke();
            currentCompletionCallback = null;

            // Fire event
            OnSpeechFinished?.Invoke();
        }

#if UNITY_EDITOR
        /// <summary>
        /// Simulate speech completion in editor
        /// </summary>
        private void SimulateComplete()
        {
            HandleSpeechComplete();
        }
#endif

        void OnDestroy()
        {
            if (isSpeaking)
            {
                Stop();
            }

#if UNITY_IOS && !UNITY_EDITOR
            Instance = null;
#endif
        }
    }
}
