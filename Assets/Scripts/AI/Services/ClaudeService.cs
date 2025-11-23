using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Monke.AI.Services
{
    /// <summary>
    /// Claude API service implementation (Anthropic)
    /// FUTURE IMPLEMENTATION - Placeholder for now
    /// </summary>
    public class ClaudeService : MonoBehaviour, IAIService
    {
        public string ServiceName => "Claude";
        public bool IsConfigured => false;

        public void Initialize(AIConfig config)
        {
            Debug.LogWarning("[ClaudeService] Not yet implemented");
        }

        public IEnumerator GetResponse(List<ConversationMessage> messages, Action<string> onSuccess, Action<string> onError)
        {
            onError?.Invoke("Claude service not yet implemented");
            yield break;
        }
    }
}
