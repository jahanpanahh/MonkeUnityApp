using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;

namespace Monke.AI.Services
{
    /// <summary>
    /// OpenAI API service implementation
    /// Supports GPT-4, GPT-4o-mini, GPT-3.5-turbo, etc.
    /// </summary>
    public class OpenAIService : MonoBehaviour, IAIService
    {
        private const string API_URL = "https://api.openai.com/v1/chat/completions";

        private AIConfig config;
        private string apiKey;

        public string ServiceName => "OpenAI";
        public bool IsConfigured => !string.IsNullOrEmpty(apiKey);

        /// <summary>
        /// Initialize the service with configuration
        /// </summary>
        public void Initialize(AIConfig config)
        {
            this.config = config;
            this.apiKey = config.GetAPIKey();

            if (string.IsNullOrEmpty(apiKey))
            {
                Debug.LogWarning("[OpenAIService] No API key configured. Set it in AIConfig or via SetAPIKey()");
            }
            else if (config.logAPIRequests)
            {
                Debug.Log($"[OpenAIService] Initialized with model: {config.openAIModel}");
            }
        }

        /// <summary>
        /// Update the API key at runtime
        /// </summary>
        public void SetAPIKey(string key)
        {
            this.apiKey = key;
            if (config != null)
            {
                config.SetAPIKey(key);
            }
        }

        /// <summary>
        /// Get a response from OpenAI API
        /// </summary>
        public IEnumerator GetResponse(List<ConversationMessage> messages, Action<string> onSuccess, Action<string> onError)
        {
            if (!IsConfigured)
            {
                onError?.Invoke("OpenAI API key not configured");
                yield break;
            }

            if (messages == null || messages.Count == 0)
            {
                onError?.Invoke("No messages provided");
                yield break;
            }

            // Build request
            OpenAIRequest request = new OpenAIRequest(
                config.openAIModel,
                messages,
                config.temperature,
                config.maxTokens
            );

            string jsonRequest = JsonUtility.ToJson(request);

            if (config.logAPIRequests)
            {
                Debug.Log($"[OpenAIService] Request: {jsonRequest}");
            }

            // Create HTTP request
            byte[] bodyRaw = Encoding.UTF8.GetBytes(jsonRequest);
            UnityWebRequest webRequest = new UnityWebRequest(API_URL, "POST");
            webRequest.uploadHandler = new UploadHandlerRaw(bodyRaw);
            webRequest.downloadHandler = new DownloadHandlerBuffer();
            webRequest.timeout = config.requestTimeout;

            // Set headers
            webRequest.SetRequestHeader("Content-Type", "application/json");
            webRequest.SetRequestHeader("Authorization", $"Bearer {apiKey}");

            // Send request
            yield return webRequest.SendWebRequest();

            // Handle response
            if (webRequest.result == UnityWebRequest.Result.ConnectionError ||
                webRequest.result == UnityWebRequest.Result.ProtocolError)
            {
                string errorMessage = ParseError(webRequest);

                if (config.logAPIRequests)
                {
                    Debug.LogError($"[OpenAIService] Error: {errorMessage}");
                }

                onError?.Invoke(errorMessage);
            }
            else
            {
                string responseText = webRequest.downloadHandler.text;

                if (config.logAPIRequests)
                {
                    Debug.Log($"[OpenAIService] Response: {responseText}");
                }

                try
                {
                    OpenAIResponse response = JsonUtility.FromJson<OpenAIResponse>(responseText);

                    if (response.choices != null && response.choices.Length > 0)
                    {
                        string content = response.choices[0].message.content;

                        if (config.logAPIRequests)
                        {
                            Debug.Log($"[OpenAIService] Tokens used: {response.usage.total_tokens}");
                        }

                        onSuccess?.Invoke(content);
                    }
                    else
                    {
                        onError?.Invoke("No response from OpenAI");
                    }
                }
                catch (Exception e)
                {
                    Debug.LogError($"[OpenAIService] Failed to parse response: {e.Message}");
                    onError?.Invoke($"Failed to parse response: {e.Message}");
                }
            }

            webRequest.Dispose();
        }

        /// <summary>
        /// Parse error from OpenAI API response
        /// </summary>
        private string ParseError(UnityWebRequest request)
        {
            try
            {
                string errorText = request.downloadHandler.text;
                OpenAIError error = JsonUtility.FromJson<OpenAIError>(errorText);

                if (error?.error != null && !string.IsNullOrEmpty(error.error.message))
                {
                    return error.error.message;
                }
            }
            catch
            {
                // Ignore parse errors, fall through to generic error
            }

            // Generic error based on status code
            switch (request.responseCode)
            {
                case 401:
                    return "Invalid API key";
                case 429:
                    return "Rate limit exceeded. Please try again later";
                case 500:
                case 502:
                case 503:
                    return "OpenAI server error. Please try again";
                default:
                    return $"Network error: {request.error}";
            }
        }
    }
}
