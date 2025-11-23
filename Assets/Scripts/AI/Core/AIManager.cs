using System;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEngine;
using Monke.AI.Services;
using Monke.AI.Speech;

namespace Monke.AI
{
    /// <summary>
    /// Main AI orchestrator - manages the conversation flow
    /// STT → AI Service → TTS
    /// Singleton pattern, persists across scenes
    /// </summary>
    public class AIManager : MonoBehaviour
    {
        // Singleton instance
        public static AIManager Instance { get; private set; }

        [Header("Configuration")]
        [SerializeField] private AIConfig config;

        [Header("Speech Components")]
        [SerializeField] private SpeechToText_iOS speechToText;
        [SerializeField] private TextToSpeech_iOS textToSpeechIOS;
        [SerializeField] private GoogleCloudTextToSpeech googleTextToSpeech;

        // AI service (runtime initialized based on config)
        private IAIService aiService;
        private ITextToSpeech textToSpeech;

        // State
        private AIState currentState = AIState.Idle;
        private ConversationHistory conversationHistory;

        // Events for UI and external systems
        public event Action<AIState> OnStateChanged;
        public event Action<string> OnUserSpeechRecognized;
        public event Action<string> OnAIResponseReceived;
        public event Action<string> OnAIResponseSpeaking;  // Filtered text for TTS
        public event Action<string> OnError;

        // Properties
        public AIState CurrentState => currentState;
        public AIConfig Config => config;
        public bool IsProcessing => currentState != AIState.Idle && currentState != AIState.Error;

        void Awake()
        {
            // Singleton pattern
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }

            Instance = this;
            DontDestroyOnLoad(gameObject);

            Debug.Log("[AIManager] Initialized");
        }

        void Start()
        {
            Initialize();
        }

        /// <summary>
        /// Initialize AI system
        /// </summary>
        private void Initialize()
        {
            // Validate configuration
            if (config == null)
            {
                Debug.LogError("[AIManager] No AIConfig assigned! Create one via Assets > Create > Monke > AI Configuration");
                return;
            }

            // Initialize conversation history
            conversationHistory = new ConversationHistory(config.maxConversationHistory);

            // Initialize AI service based on config
            InitializeAIService();

            // Initialize speech components
            InitializeSpeechComponents();

            Debug.Log($"[AIManager] Initialized with {config.serviceType} service");
        }

        /// <summary>
        /// Initialize AI service based on configuration
        /// </summary>
        private void InitializeAIService()
        {
            switch (config.serviceType)
            {
                case AIServiceType.OpenAI:
                    OpenAIService openAI = gameObject.AddComponent<OpenAIService>();
                    openAI.Initialize(config);
                    aiService = openAI;
                    break;

                case AIServiceType.Claude:
                    ClaudeService claude = gameObject.AddComponent<ClaudeService>();
                    claude.Initialize(config);
                    aiService = claude;
                    break;

                case AIServiceType.LocalLLM:
                    LocalLLMService localLLM = gameObject.AddComponent<LocalLLMService>();
                    localLLM.Initialize(config);
                    aiService = localLLM;
                    break;

                default:
                    Debug.LogError($"[AIManager] Unknown service type: {config.serviceType}");
                    break;
            }

            if (aiService != null && !aiService.IsConfigured)
            {
                Debug.LogWarning($"[AIManager] {aiService.ServiceName} is not configured. Set API key in config or via SetAPIKey()");
            }
        }

        /// <summary>
        /// Initialize speech components
        /// </summary>
        private void InitializeSpeechComponents()
        {
            // Create components if not assigned
            if (speechToText == null)
            {
                speechToText = gameObject.AddComponent<SpeechToText_iOS>();
            }

            if (config.useGoogleCloudTTS)
            {
                if (googleTextToSpeech == null)
                {
                    googleTextToSpeech = gameObject.AddComponent<GoogleCloudTextToSpeech>();
                }

                googleTextToSpeech.Configure(config);
                textToSpeech = googleTextToSpeech;
                Debug.Log("[AIManager] Using Google Cloud TTS provider");
            }
            else
            {
                if (textToSpeechIOS == null)
                {
                    textToSpeechIOS = gameObject.AddComponent<TextToSpeech_iOS>();
                }

                textToSpeech = textToSpeechIOS;
                Debug.Log("[AIManager] Using native iOS TTS provider");
            }

            // Subscribe to events
            speechToText.OnTextRecognized += HandleSpeechRecognized;
            speechToText.OnError += HandleSpeechError;

            if (textToSpeech != null)
            {
                textToSpeech.OnSpeechFinished += HandleSpeechFinished;
                textToSpeech.OnError += HandleTTSError;
            }

            // Request permissions on first launch
            speechToText.RequestPermission();
        }

        /// <summary>
        /// Start listening for user speech
        /// </summary>
        public void StartListening()
        {
            if (currentState != AIState.Idle)
            {
                Debug.LogWarning($"[AIManager] Cannot start listening in state: {currentState}");
                return;
            }

            if (!aiService.IsConfigured)
            {
                string error = $"{aiService.ServiceName} is not configured. Please set your API key.";
                Debug.LogError($"[AIManager] {error}");
                HandleError(error);
                return;
            }

            Debug.Log("[AIManager] Starting to listen...");
            SetState(AIState.Listening);

            bool success = speechToText.StartRecording();
            if (!success)
            {
                Debug.LogError("[AIManager] Failed to start speech recognition");
                HandleError("Failed to start listening. Check microphone permissions.");
                SetState(AIState.Error);
            }
        }

        /// <summary>
        /// Stop listening (manual cancel)
        /// </summary>
        public void StopListening()
        {
            if (currentState == AIState.Listening)
            {
                Debug.Log("[AIManager] Stopping listening...");
                speechToText.StopRecording();
                SetState(AIState.Idle);
            }
        }

        /// <summary>
        /// Cancel current operation
        /// </summary>
        public void Cancel()
        {
            Debug.Log("[AIManager] Cancelling current operation...");

            if (speechToText.IsRecording)
            {
                speechToText.StopRecording();
            }

            if (textToSpeech != null && textToSpeech.IsSpeaking)
            {
                textToSpeech.Stop();
            }

            StopAllCoroutines();
            SetState(AIState.Idle);
        }

        /// <summary>
        /// Clear conversation history
        /// </summary>
        public void ClearHistory()
        {
            Debug.Log("[AIManager] Clearing conversation history");
            conversationHistory?.Clear();
        }

        /// <summary>
        /// Set API key at runtime
        /// </summary>
        public void SetAPIKey(string key)
        {
            if (aiService is OpenAIService openAI)
            {
                openAI.SetAPIKey(key);
                Debug.Log("[AIManager] API key updated");
            }
        }

        /// <summary>
        /// Handle speech recognized from STT
        /// </summary>
        private void HandleSpeechRecognized(string text)
        {
            Debug.Log($"[AIManager] User said: {text}");

            // Fire event
            OnUserSpeechRecognized?.Invoke(text);

            // Add to conversation history
            if (config.enableConversationHistory)
            {
                conversationHistory.AddMessage("user", text);
            }

            // Start AI processing
            StartCoroutine(ProcessAIRequest(text));
        }

        /// <summary>
        /// Process AI request and handle response
        /// </summary>
        private IEnumerator ProcessAIRequest(string userMessage)
        {
            SetState(AIState.Processing);

            // Build messages list (system prompt + conversation history)
            List<ConversationMessage> messages = new List<ConversationMessage>();

            // Add system prompt
            messages.Add(new ConversationMessage("system", config.systemPrompt));

            // Add conversation history (if enabled)
            if (config.enableConversationHistory)
            {
                messages.AddRange(conversationHistory.GetMessages());
            }
            else
            {
                // Just add this single user message
                messages.Add(new ConversationMessage("user", userMessage));
            }

            // Call AI service
            string aiResponse = null;
            bool hasError = false;

            yield return aiService.GetResponse(
                messages,
                (response) => { aiResponse = response; },
                (error) =>
                {
                    hasError = true;
                    HandleError($"AI Error: {error}");
                }
            );

            // Check for errors
            if (hasError)
            {
                SetState(AIState.Error);
                yield break;
            }

            if (string.IsNullOrEmpty(aiResponse))
            {
                HandleError("AI returned empty response");
                SetState(AIState.Error);
                yield break;
            }

            Debug.Log($"[AIManager] AI response: {aiResponse}");

            // Fire event with full response (including stage directions)
            OnAIResponseReceived?.Invoke(aiResponse);

            // Add to conversation history
            if (config.enableConversationHistory)
            {
                conversationHistory.AddMessage("assistant", aiResponse);
            }

            // Filter stage directions for TTS (remove text between asterisks)
            string filteredResponse = FilterStageDirections(aiResponse);

            Debug.Log($"[AIManager] Filtered for speech: {filteredResponse}");

            // Fire event with filtered text
            OnAIResponseSpeaking?.Invoke(filteredResponse);

            // Speak the response
            if (textToSpeech == null)
            {
                HandleError("Text-to-Speech not initialized");
                SetState(AIState.Error);
            }
            else
            {
                SetState(AIState.Speaking);
                textToSpeech.Speak(filteredResponse);
            }
        }

        /// <summary>
        /// Filter out stage directions and markdown formatting for TTS
        /// Example: "Hello! *waves excitedly* How are you?" → "Hello! How are you?"
        /// Example: "Look at the **glowing star**!" → "Look at the glowing star!"
        /// </summary>
        private string FilterStageDirections(string text)
        {
            // Remove markdown bold (**text**)
            string filtered = Regex.Replace(text, @"\*\*([^*]+)\*\*", "$1");

            // Remove markdown italic and stage directions (*text*)
            filtered = Regex.Replace(filtered, @"\*([^*]+)\*", "$1");

            // Remove markdown italic with underscores (_text_ or __text__)
            filtered = Regex.Replace(filtered, @"__([^_]+)__", "$1");
            filtered = Regex.Replace(filtered, @"_([^_]+)_", "$1");

            // Remove inline code (`code`)
            filtered = Regex.Replace(filtered, @"`([^`]+)`", "$1");

            // Remove code blocks (```code```)
            filtered = Regex.Replace(filtered, @"```[^`]*```", "");

            // Remove markdown headers (# Header)
            filtered = Regex.Replace(filtered, @"^#+\s*", "", RegexOptions.Multiline);

            // Remove markdown links [text](url) - keep only the text
            filtered = Regex.Replace(filtered, @"\[([^\]]+)\]\([^\)]+\)", "$1");

            // Clean up extra whitespace
            filtered = Regex.Replace(filtered, @"\s+", " ").Trim();

            return filtered;
        }

        /// <summary>
        /// Handle TTS speech finished
        /// </summary>
        private void HandleSpeechFinished()
        {
            Debug.Log("[AIManager] Speech finished");
            SetState(AIState.Idle);
        }

        /// <summary>
        /// Handle STT error
        /// </summary>
        private void HandleSpeechError(string error)
        {
            Debug.LogError($"[AIManager] Speech recognition error: {error}");
            HandleError($"Speech error: {error}");
        }

        /// <summary>
        /// Handle TTS error
        /// </summary>
        private void HandleTTSError(string error)
        {
            Debug.LogError($"[AIManager] TTS error: {error}");
            HandleError($"Speech synthesis error: {error}");
        }

        /// <summary>
        /// Handle generic errors
        /// </summary>
        private void HandleError(string error)
        {
            Debug.LogError($"[AIManager] Error: {error}");

            // Fire error event
            OnError?.Invoke(error);

            // Optionally speak the error (if configured)
            if (config.speakErrors && textToSpeech != null && !textToSpeech.IsSpeaking)
            {
                string friendlyError = GetFriendlyErrorMessage(error);
                textToSpeech.Speak(friendlyError);
            }

            // Return to idle state after a delay
            Invoke(nameof(ReturnToIdle), 2f);
        }

        /// <summary>
        /// Convert technical errors to child-friendly messages
        /// </summary>
        private string GetFriendlyErrorMessage(string error)
        {
            if (error.Contains("API key") || error.Contains("not configured"))
                return "Oops! I need to be set up first. Ask a grown-up to help!";

            if (error.Contains("network") || error.Contains("Network") || error.Contains("connection"))
                return "Oh no! I can't connect right now. Check your internet!";

            if (error.Contains("timeout") || error.Contains("too long"))
                return "That's taking too long. Let's try again!";

            if (error.Contains("permission") || error.Contains("Permission"))
                return "I need permission to hear you. Can you allow it in settings?";

            if (error.Contains("rate limit"))
                return "I'm a bit tired! Can you try again in a minute?";

            return "Oops! Something went wrong. Let's try again!";
        }

        /// <summary>
        /// Return to idle state
        /// </summary>
        private void ReturnToIdle()
        {
            SetState(AIState.Idle);
        }

        /// <summary>
        /// Set current state and fire event
        /// </summary>
        private void SetState(AIState newState)
        {
            if (currentState != newState)
            {
                Debug.Log($"[AIManager] State: {currentState} → {newState}");
                currentState = newState;
                OnStateChanged?.Invoke(newState);
            }
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (speechToText != null)
            {
                speechToText.OnTextRecognized -= HandleSpeechRecognized;
                speechToText.OnError -= HandleSpeechError;
            }

            if (textToSpeech != null)
            {
                textToSpeech.OnSpeechFinished -= HandleSpeechFinished;
                textToSpeech.OnError -= HandleTTSError;
            }

            if (Instance == this)
            {
                Instance = null;
            }
        }
    }

    /// <summary>
    /// Manages conversation history with a rolling window
    /// </summary>
    public class ConversationHistory
    {
        private List<ConversationMessage> messages;
        private int maxMessages;

        public ConversationHistory(int maxMessages)
        {
            this.maxMessages = maxMessages;
            this.messages = new List<ConversationMessage>();
        }

        public void AddMessage(string role, string content)
        {
            messages.Add(new ConversationMessage(role, content));

            // Remove oldest messages if we exceed the limit
            while (messages.Count > maxMessages)
            {
                messages.RemoveAt(0);
            }
        }

        public List<ConversationMessage> GetMessages()
        {
            return new List<ConversationMessage>(messages);
        }

        public void Clear()
        {
            messages.Clear();
        }

        public int Count => messages.Count;
    }
}
