using System;
using System.Collections;
using System.Collections.Generic;

namespace Monke.AI.Services
{
    /// <summary>
    /// Interface for AI service implementations (OpenAI, Claude, LocalLLM, etc.)
    /// Allows swapping between different AI backends
    /// </summary>
    public interface IAIService
    {
        /// <summary>
        /// Name of the service (e.g., "OpenAI", "Claude")
        /// </summary>
        string ServiceName { get; }

        /// <summary>
        /// Whether the service is properly configured and ready to use
        /// </summary>
        bool IsConfigured { get; }

        /// <summary>
        /// Get a response from the AI service
        /// </summary>
        /// <param name="messages">Full conversation history (including system prompt)</param>
        /// <param name="onSuccess">Callback with the AI response text</param>
        /// <param name="onError">Callback with error message if something goes wrong</param>
        /// <returns>Coroutine to yield</returns>
        IEnumerator GetResponse(List<ConversationMessage> messages, Action<string> onSuccess, Action<string> onError);
    }
}
