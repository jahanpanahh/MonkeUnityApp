using System;
using System.Collections;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using Monke.AI;

namespace Monke.AI.Speech
{
    /// <summary>
    /// Premium Text-to-Speech using Google Cloud (via premium-tts proxy server).
    /// Fetches kid-like audio (Neural2 voice) and plays the returned MP3.
    /// </summary>
    public class GoogleCloudTextToSpeech : MonoBehaviour, ITextToSpeech
    {
        [Header("API Settings")]
        [SerializeField] private string apiUrl = "http://localhost:3100/api/text-to-speech";
        [SerializeField] private float speakingRate = 1.0f;
        [SerializeField] private float pitch = 4.0f;

        [Header("Playback")]
        [SerializeField, Range(0f, 1f)] private float volume = 1.0f;

        public event Action OnSpeechFinished;
        public event Action<string> OnError;

        private AudioSource audioSource;
        private Coroutine speakRoutine;
        private bool isSpeaking;
        private string tempFilePath;

        public bool IsSpeaking => isSpeaking;

        void Awake()
        {
            audioSource = gameObject.AddComponent<AudioSource>();
            audioSource.playOnAwake = false;
            audioSource.loop = false;
        }

        /// <summary>
        /// Configure the provider using AIConfig values.
        /// </summary>
        public void Configure(AIConfig config)
        {
            if (config == null) return;

            if (!string.IsNullOrEmpty(config.googleTTSApiUrl))
                apiUrl = config.googleTTSApiUrl;

            volume = Mathf.Clamp01(config.googleTTSVolume);
            speakingRate = config.googleTTSSpeakingRate;
            pitch = config.googleTTSPitch;
        }

        public void Speak(string text, Action onComplete = null)
        {
            if (string.IsNullOrWhiteSpace(text))
            {
                OnError?.Invoke("Empty text provided");
                return;
            }

            Stop();
            speakRoutine = StartCoroutine(SpeakRoutine(text, onComplete));
        }

        public void Stop()
        {
            if (speakRoutine != null)
            {
                StopCoroutine(speakRoutine);
                speakRoutine = null;
            }

            if (audioSource != null && audioSource.isPlaying)
            {
                audioSource.Stop();
            }

            CleanupTempFile();
            isSpeaking = false;
        }

        private IEnumerator SpeakRoutine(string text, Action onComplete)
        {
            isSpeaking = true;

            // Build request payload (matches premium-tts/server.js)
            GoogleTTSRequest requestPayload = new GoogleTTSRequest
            {
                text = text,
                speakingRate = speakingRate,
                pitch = pitch
            };

            string json = JsonUtility.ToJson(requestPayload);
            byte[] bodyRaw = Encoding.UTF8.GetBytes(json);

            using (UnityWebRequest webRequest = new UnityWebRequest(apiUrl, "POST"))
            {
                webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
                webRequest.downloadHandler = new DownloadHandlerBuffer();
                webRequest.SetRequestHeader("Content-Type", "application/json");
                webRequest.timeout = 20;

                yield return webRequest.SendWebRequest();

                if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                    webRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    string error = $"TTS request failed: {webRequest.error}";
                    OnError?.Invoke(error);
                    isSpeaking = false;
                    yield break;
                }

                GoogleTTSResponse response = null;
                try
                {
                    response = JsonUtility.FromJson<GoogleTTSResponse>(webRequest.downloadHandler.text);
                }
                catch (Exception ex)
                {
                    OnError?.Invoke($"Failed to parse TTS response: {ex.Message}");
                    isSpeaking = false;
                    yield break;
                }

                if (response == null || string.IsNullOrEmpty(response.audio))
                {
                    OnError?.Invoke("Empty TTS audio response");
                    isSpeaking = false;
                    yield break;
                }

                byte[] audioBytes = Convert.FromBase64String(response.audio);
                tempFilePath = Path.Combine(Application.persistentDataPath, $"tts-{Guid.NewGuid()}.mp3");
                File.WriteAllBytes(tempFilePath, audioBytes);
            }

            // Load MP3 from the temp file so Unity can decode it reliably
            using (UnityWebRequest audioRequest = UnityWebRequestMultimedia.GetAudioClip($"file://{tempFilePath}", AudioType.MPEG))
            {
                yield return audioRequest.SendWebRequest();

                if (audioRequest.result == UnityWebRequest.Result.ConnectionError ||
                    audioRequest.result == UnityWebRequest.Result.ProtocolError)
                {
                    string error = $"Failed to load TTS audio: {audioRequest.error}";
                    OnError?.Invoke(error);
                    isSpeaking = false;
                    CleanupTempFile();
                    yield break;
                }

                AudioClip clip = DownloadHandlerAudioClip.GetContent(audioRequest);
                audioSource.clip = clip;
                audioSource.volume = volume;
                audioSource.Play();
            }

            // Wait for playback to finish
            while (audioSource.isPlaying)
            {
                yield return null;
            }

            isSpeaking = false;
            CleanupTempFile();

            onComplete?.Invoke();
            OnSpeechFinished?.Invoke();
        }

        private void CleanupTempFile()
        {
            if (!string.IsNullOrEmpty(tempFilePath) && File.Exists(tempFilePath))
            {
                try
                {
                    File.Delete(tempFilePath);
                }
                catch (Exception ex)
                {
                    Debug.LogWarning($"[GoogleCloudTTS] Failed to delete temp file: {ex.Message}");
                }
            }

            tempFilePath = null;
        }

        [Serializable]
        private class GoogleTTSRequest
        {
            public string text;
            public float speakingRate;
            public float pitch;
        }

        [Serializable]
        private class GoogleTTSResponse
        {
            public string audio;
            public string contentType;
        }
    }
}
