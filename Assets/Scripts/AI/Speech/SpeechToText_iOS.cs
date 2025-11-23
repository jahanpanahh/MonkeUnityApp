using System;
using System.Runtime.InteropServices;
using UnityEngine;
using AOT;

namespace Monke.AI.Speech
{
    /// <summary>
    /// iOS implementation of Speech-to-Text using Apple's SFSpeechRecognizer
    /// Features auto-silence detection (2 seconds)
    /// </summary>
    public class SpeechToText_iOS : MonoBehaviour, ISpeechToText
    {
        // Events from interface
        public event Action<string> OnTextRecognized;
        public event Action<string> OnError;

        private bool isRecording = false;

#if UNITY_IOS && !UNITY_EDITOR
        // Native iOS function imports
        [DllImport("__Internal")]
        private static extern void _InitSpeechRecognizer();

        [DllImport("__Internal")]
        private static extern void _RequestSpeechPermission();

        [DllImport("__Internal")]
        private static extern bool _StartSpeechRecognition(RecognitionCallback callback);

        [DllImport("__Internal")]
        private static extern void _StopSpeechRecognition();

        // Callback delegate that matches native side
        private delegate void RecognitionCallback(string text);

        // Static callback function (must be static for native calls)
        [MonoPInvokeCallback(typeof(RecognitionCallback))]
        private static void OnRecognitionCallback(string text)
        {
            // Forward to instance method on main thread
            if (Instance != null)
            {
                Instance.HandleRecognitionResult(text);
            }
        }

        private static SpeechToText_iOS Instance;
#endif

        public bool IsRecording => isRecording;

        void Awake()
        {
#if UNITY_IOS && !UNITY_EDITOR
            Instance = this;
            _InitSpeechRecognizer();
            Debug.Log("[SpeechToText_iOS] Initialized");
#else
            Debug.Log("[SpeechToText_iOS] Running in editor mode - speech recognition will be simulated");
#endif
        }

        /// <summary>
        /// Request microphone and speech recognition permissions
        /// </summary>
        public void RequestPermission()
        {
#if UNITY_IOS && !UNITY_EDITOR
            Debug.Log("[SpeechToText_iOS] Requesting speech permission...");
            _RequestSpeechPermission();
#else
            Debug.Log("[SpeechToText_iOS] Permission request simulated in editor");
#endif
        }

        /// <summary>
        /// Start listening and recording speech
        /// Auto-stops after 2 seconds of silence
        /// </summary>
        public bool StartRecording()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (!isRecording)
            {
                Debug.Log("[SpeechToText_iOS] Starting speech recognition...");
                bool success = _StartSpeechRecognition(OnRecognitionCallback);
                if (success)
                {
                    isRecording = true;
                    Debug.Log("[SpeechToText_iOS] Speech recognition started successfully");
                }
                else
                {
                    Debug.LogError("[SpeechToText_iOS] Failed to start speech recognition");
                    OnError?.Invoke("Failed to start speech recognition. Check permissions.");
                }
                return success;
            }
            else
            {
                Debug.LogWarning("[SpeechToText_iOS] Already recording");
                return false;
            }
#else
            // Simulate recognition in editor for testing
            Debug.Log("[SpeechToText_iOS] Simulating speech recognition in editor");
            isRecording = true;

            // Simulate a delay and then return a test result
            Invoke(nameof(SimulateRecognition), 2f);
            return true;
#endif
        }

        /// <summary>
        /// Stop listening and recording
        /// </summary>
        public void StopRecording()
        {
#if UNITY_IOS && !UNITY_EDITOR
            if (isRecording)
            {
                Debug.Log("[SpeechToText_iOS] Stopping speech recognition...");
                _StopSpeechRecognition();
                isRecording = false;
            }
#else
            Debug.Log("[SpeechToText_iOS] Stopped speech recognition (simulated)");
            CancelInvoke(nameof(SimulateRecognition));
            isRecording = false;
#endif
        }

        /// <summary>
        /// Handle final recognition result from native plugin
        /// </summary>
        private void HandleRecognitionResult(string text)
        {
            Debug.Log($"[SpeechToText_iOS] Recognized text (final): {text}");

            // Recording has automatically stopped (via auto-silence detection)
            isRecording = false;

            if (!string.IsNullOrEmpty(text))
            {
                // Fire event with recognized text
                OnTextRecognized?.Invoke(text);
            }
            else
            {
                Debug.LogWarning("[SpeechToText_iOS] Empty transcription received");
                OnError?.Invoke("No speech detected");
            }
        }

#if UNITY_EDITOR
        /// <summary>
        /// Simulate speech recognition in editor for testing
        /// </summary>
        private void SimulateRecognition()
        {
            Debug.Log("[SpeechToText_iOS] Simulating recognized speech");
            isRecording = false;
            OnTextRecognized?.Invoke("Hello Monke! This is simulated speech from the editor.");
        }
#endif

        void OnDestroy()
        {
            if (isRecording)
            {
                StopRecording();
            }

#if UNITY_IOS && !UNITY_EDITOR
            Instance = null;
#endif
        }
    }
}
