using System.Collections;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Monke.AI.UI
{
    /// <summary>
    /// Debug UI panel for AI system
    /// Shows mic button, status, conversation history, and settings
    /// Can be hidden for production builds via AIConfig.enableDebugUI
    /// </summary>
    public class AIDebugPanel : MonoBehaviour
    {
        [Header("UI References")]
        [SerializeField] private Button micButton;
        [SerializeField] private Image micButtonImage;
        [SerializeField] private TextMeshProUGUI statusText;
        [SerializeField] private TextMeshProUGUI conversationText;
        [SerializeField] private ScrollRect conversationScrollRect;
        [SerializeField] private Button settingsButton;
        [SerializeField] private GameObject settingsPanel;
        [SerializeField] private TMP_InputField apiKeyInput;
        [SerializeField] private Button saveApiKeyButton;
        [SerializeField] private Button clearHistoryButton;

        [Header("Visual Settings")]
        [SerializeField] private Color idleColor = Color.gray;
        [SerializeField] private Color listeningColor = Color.red;
        [SerializeField] private Color processingColor = Color.yellow;
        [SerializeField] private Color speakingColor = Color.green;
        [SerializeField] private Color errorColor = new Color(1f, 0.5f, 0f); // Orange

        private string conversationHistory = "";
        private bool shouldScrollToBottom = false;

        void Start()
        {
            // Check if debug UI is enabled
            if (AIManager.Instance != null && AIManager.Instance.Config != null)
            {
                if (!AIManager.Instance.Config.enableDebugUI)
                {
                    // Hide entire panel
                    gameObject.SetActive(false);
                    return;
                }
            }

            // Subscribe to AIManager events
            if (AIManager.Instance != null)
            {
                AIManager.Instance.OnStateChanged += OnAIStateChanged;
                AIManager.Instance.OnUserSpeechRecognized += OnUserSpeechRecognized;
                AIManager.Instance.OnAIResponseReceived += OnAIResponseReceived;
                AIManager.Instance.OnError += OnAIError;
            }
            else
            {
                Debug.LogWarning("[AIDebugPanel] AIManager not found! Make sure it exists in the scene.");
            }

            // Setup button listeners
            if (micButton != null)
            {
                micButton.onClick.AddListener(OnMicButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.AddListener(ToggleSettings);
            }

            if (saveApiKeyButton != null)
            {
                saveApiKeyButton.onClick.AddListener(SaveApiKey);
            }

            if (clearHistoryButton != null)
            {
                clearHistoryButton.onClick.AddListener(ClearHistory);
            }

            // Hide settings panel initially
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(false);
            }

            // Initialize UI
            UpdateStatusText(AIState.Idle);
            UpdateMicButton(AIState.Idle);
        }

        void Update()
        {
            // Auto-scroll conversation to bottom when new messages arrive
            if (shouldScrollToBottom && conversationScrollRect != null)
            {
                Canvas.ForceUpdateCanvases();
                conversationScrollRect.verticalNormalizedPosition = 0f;
                shouldScrollToBottom = false;
            }
        }

        /// <summary>
        /// Handle mic button click
        /// </summary>
        private void OnMicButtonClicked()
        {
            if (AIManager.Instance == null)
            {
                Debug.LogError("[AIDebugPanel] AIManager not found!");
                return;
            }

            AIState currentState = AIManager.Instance.CurrentState;

            switch (currentState)
            {
                case AIState.Idle:
                    // Start listening
                    AIManager.Instance.StartListening();
                    break;

                case AIState.Listening:
                    // Stop listening (cancel)
                    AIManager.Instance.StopListening();
                    break;

                case AIState.Processing:
                case AIState.Speaking:
                    // Cancel current operation
                    AIManager.Instance.Cancel();
                    break;

                case AIState.Error:
                    // Reset to idle
                    AIManager.Instance.Cancel();
                    break;
            }
        }

        /// <summary>
        /// Handle AI state changes
        /// </summary>
        private void OnAIStateChanged(AIState newState)
        {
            UpdateStatusText(newState);
            UpdateMicButton(newState);
        }

        /// <summary>
        /// Update status text based on state
        /// </summary>
        private void UpdateStatusText(AIState state)
        {
            if (statusText == null) return;

            switch (state)
            {
                case AIState.Idle:
                    statusText.text = "Tap to speak";
                    statusText.color = Color.white;
                    break;

                case AIState.Listening:
                    statusText.text = "Listening... (speak naturally)";
                    statusText.color = listeningColor;
                    break;

                case AIState.Processing:
                    statusText.text = "Monke is thinking...";
                    statusText.color = processingColor;
                    break;

                case AIState.Speaking:
                    statusText.text = "Monke is speaking...";
                    statusText.color = speakingColor;
                    break;

                case AIState.Error:
                    statusText.text = "Error! Tap to retry";
                    statusText.color = errorColor;
                    break;
            }
        }

        /// <summary>
        /// Update mic button visual based on state
        /// </summary>
        private void UpdateMicButton(AIState state)
        {
            if (micButtonImage == null) return;

            switch (state)
            {
                case AIState.Idle:
                    micButtonImage.color = idleColor;
                    if (micButton != null) micButton.interactable = true;
                    break;

                case AIState.Listening:
                    micButtonImage.color = listeningColor;
                    if (micButton != null) micButton.interactable = true;
                    break;

                case AIState.Processing:
                    micButtonImage.color = processingColor;
                    if (micButton != null) micButton.interactable = true; // Allow cancel
                    break;

                case AIState.Speaking:
                    micButtonImage.color = speakingColor;
                    if (micButton != null) micButton.interactable = true; // Allow cancel
                    break;

                case AIState.Error:
                    micButtonImage.color = errorColor;
                    if (micButton != null) micButton.interactable = true;
                    break;
            }
        }

        /// <summary>
        /// Handle user speech recognized
        /// </summary>
        private void OnUserSpeechRecognized(string text)
        {
            AddMessage("You", text);
        }

        /// <summary>
        /// Handle AI response received
        /// </summary>
        private void OnAIResponseReceived(string text)
        {
            AddMessage("Monke", text);
        }

        /// <summary>
        /// Handle AI error
        /// </summary>
        private void OnAIError(string error)
        {
            AddMessage("Error", error, errorColor);
        }

        /// <summary>
        /// Add a message to the conversation display
        /// </summary>
        private void AddMessage(string sender, string message, Color? senderColor = null)
        {
            if (conversationText == null) return;

            Color color = senderColor ?? Color.white;
            string colorHex = ColorUtility.ToHtmlStringRGB(color);

            conversationHistory += $"<b><color=#{colorHex}>{sender}:</color></b> {message}\n\n";
            conversationText.text = conversationHistory;
            shouldScrollToBottom = true;
        }

        /// <summary>
        /// Toggle settings panel
        /// </summary>
        private void ToggleSettings()
        {
            if (settingsPanel != null)
            {
                settingsPanel.SetActive(!settingsPanel.activeSelf);
            }
        }

        /// <summary>
        /// Save API key
        /// </summary>
        private void SaveApiKey()
        {
            if (apiKeyInput == null || AIManager.Instance == null) return;

            string key = apiKeyInput.text.Trim();
            if (!string.IsNullOrEmpty(key))
            {
                AIManager.Instance.SetAPIKey(key);
                Debug.Log("[AIDebugPanel] API key saved");
                AddMessage("System", "API key saved successfully!", Color.green);

                // Hide settings panel
                if (settingsPanel != null)
                {
                    settingsPanel.SetActive(false);
                }
            }
            else
            {
                AddMessage("System", "Please enter a valid API key", errorColor);
            }
        }

        /// <summary>
        /// Clear conversation history
        /// </summary>
        private void ClearHistory()
        {
            if (AIManager.Instance != null)
            {
                AIManager.Instance.ClearHistory();
            }

            conversationHistory = "";
            if (conversationText != null)
            {
                conversationText.text = "";
            }

            Debug.Log("[AIDebugPanel] Conversation history cleared");
        }

        void OnDestroy()
        {
            // Unsubscribe from events
            if (AIManager.Instance != null)
            {
                AIManager.Instance.OnStateChanged -= OnAIStateChanged;
                AIManager.Instance.OnUserSpeechRecognized -= OnUserSpeechRecognized;
                AIManager.Instance.OnAIResponseReceived -= OnAIResponseReceived;
                AIManager.Instance.OnError -= OnAIError;
            }

            // Remove button listeners
            if (micButton != null)
            {
                micButton.onClick.RemoveListener(OnMicButtonClicked);
            }

            if (settingsButton != null)
            {
                settingsButton.onClick.RemoveListener(ToggleSettings);
            }

            if (saveApiKeyButton != null)
            {
                saveApiKeyButton.onClick.RemoveListener(SaveApiKey);
            }

            if (clearHistoryButton != null)
            {
                clearHistoryButton.onClick.RemoveListener(ClearHistory);
            }
        }
    }
}
