using UnityEngine;

namespace Monke.AI
{
    /// <summary>
    /// Configuration settings for the AI system
    /// Create via Assets > Create > Monke > AI Configuration
    /// </summary>
    [CreateAssetMenu(fileName = "MonkeAIConfig", menuName = "Monke/AI Configuration")]
    public class AIConfig : ScriptableObject
    {
        [Header("AI Service")]
        [Tooltip("Which AI service to use")]
        public AIServiceType serviceType = AIServiceType.OpenAI;

        [Tooltip("API key for the selected service. Can be set at runtime via PlayerPrefs")]
        public string apiKey = "";

        [Header("OpenAI Settings")]
        [Tooltip("OpenAI model to use (e.g., gpt-4o-mini, gpt-4, gpt-3.5-turbo)")]
        public string openAIModel = "gpt-4o-mini";

        [Tooltip("Maximum tokens in the AI response (recommended: 100-200 for child-friendly responses)")]
        [Range(50, 500)]
        public int maxTokens = 150;

        [Tooltip("Response randomness (0 = focused, 1 = creative)")]
        [Range(0f, 1f)]
        public float temperature = 0.7f;

        [Tooltip("Request timeout in seconds")]
        [Range(10, 60)]
        public int requestTimeout = 30;

        [Header("Claude Settings (Future)")]
        public string claudeModel = "claude-3-5-sonnet-20241022";

        [Header("Local LLM Settings (Future)")]
        public string localServerUrl = "http://localhost:8080/completion";

        [Header("Conversation")]
        [Tooltip("Enable conversation history (recommended for better UX)")]
        public bool enableConversationHistory = true;

        [Tooltip("Maximum number of messages to keep in history (5 exchanges = 10 messages)")]
        [Range(2, 20)]
        public int maxConversationHistory = 10;

        [Header("Personality")]
        [Tooltip("System prompt that defines Monke's personality")]
        [TextArea(5, 10)]
        public string systemPrompt = "You are Monke, a curious and friendly monkey companion for children aged 7-12. You love learning, exploring, and helping kids discover new things! Keep your responses short (2-3 sentences), fun, and age-appropriate. Use simple language and be encouraging. IMPORTANT: Do NOT use action descriptions like *waves* or **bold formatting** - your responses will be spoken aloud, so only use natural spoken dialogue. Be playful but educational!";

        [Header("Speech Settings")]
        [Tooltip("Seconds of silence before auto-stopping STT")]
        [Range(1f, 5f)]
        public float silenceTimeout = 2.0f;

        [Tooltip("iOS voice identifier (e.g., com.apple.ttsbundle.Samantha-compact)")]
        public string voiceIdentifier = "com.apple.ttsbundle.Samantha-compact";

        [Tooltip("Speech rate (0.5 = normal, lower = slower)")]
        [Range(0.3f, 0.7f)]
        public float speechRate = 0.48f;

        [Tooltip("Speech pitch multiplier (1.0 = normal, higher = more childlike)")]
        [Range(0.8f, 1.5f)]
        public float speechPitch = 1.15f;

        [Header("Premium Voice (Google Cloud)")]
        [Tooltip("Enable premium kid-like Google Cloud TTS (via premium-tts server)")]
        public bool useGoogleCloudTTS = false;

        [Tooltip("Base URL for the premium TTS proxy server")]
        public string googleTTSApiUrl = "http://localhost:3100/api/text-to-speech";

        [Tooltip("Playback volume for premium TTS")]
        [Range(0f, 1f)]
        public float googleTTSVolume = 0.9f;

        [Tooltip("Speaking rate passed to Google TTS (1.0 = normal)")]
        [Range(0.5f, 1.5f)]
        public float googleTTSSpeakingRate = 1.0f;

        [Tooltip("Pitch passed to Google TTS (4.0 ~ kid-like)")]
        [Range(-20f, 20f)]
        public float googleTTSPitch = 4.0f;

        [Header("Debug")]
        [Tooltip("Show debug UI (mic button, status text, conversation log). Disable for production")]
        public bool enableDebugUI = true;

        [Tooltip("Log all API requests and responses to console")]
        public bool logAPIRequests = false;

        [Tooltip("Speak error messages via TTS")]
        public bool speakErrors = false;

        /// <summary>
        /// Get the API key, checking multiple sources in priority order
        /// 1. PlayerPrefs (runtime)
        /// 2. ScriptableObject (inspector)
        /// 3. Environment variable (editor only)
        /// </summary>
        public string GetAPIKey()
        {
            // Check PlayerPrefs first (for runtime configuration)
            string playerPrefsKey = $"Monke.AI.{serviceType}.APIKey";
            if (PlayerPrefs.HasKey(playerPrefsKey))
            {
                string key = PlayerPrefs.GetString(playerPrefsKey);
                if (!string.IsNullOrEmpty(key))
                    return key;
            }

            // Fall back to ScriptableObject
            if (!string.IsNullOrEmpty(apiKey))
                return apiKey;

#if UNITY_EDITOR
            // Fall back to environment variable (editor only)
            string envVar = System.Environment.GetEnvironmentVariable($"MONKE_{serviceType.ToString().ToUpper()}_API_KEY");
            if (!string.IsNullOrEmpty(envVar))
                return envVar;
#endif

            return "";
        }

        /// <summary>
        /// Set the API key at runtime (stored in PlayerPrefs)
        /// </summary>
        public void SetAPIKey(string key)
        {
            string playerPrefsKey = $"Monke.AI.{serviceType}.APIKey";
            PlayerPrefs.SetString(playerPrefsKey, key);
            PlayerPrefs.Save();
        }

        /// <summary>
        /// Clear the stored API key
        /// </summary>
        public void ClearAPIKey()
        {
            string playerPrefsKey = $"Monke.AI.{serviceType}.APIKey";
            PlayerPrefs.DeleteKey(playerPrefsKey);
            PlayerPrefs.Save();
        }
    }
}
