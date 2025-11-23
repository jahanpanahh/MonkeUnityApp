using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monke.AI.Services
{
    /// <summary>
    /// Local LLM service implementation (llama.cpp server)
    /// FUTURE IMPLEMENTATION - Placeholder for now
    /// Based on MonkeUnityAppPOC implementation
    /// </summary>
    public class LocalLLMService : MonoBehaviour, IAIService
    {
        public string ServiceName => "LocalLLM";
        public bool IsConfigured => false;

        public void Initialize(AIConfig config)
        {
            Debug.LogWarning("[LocalLLMService] Not yet implemented");
        }

        public IEnumerator GetResponse(List<ConversationMessage> messages, Action<string> onSuccess, Action<string> onError)
        {
            onError?.Invoke("Local LLM service not yet implemented");
            yield break;
        }
    }
}
