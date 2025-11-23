using System;
using System.Collections.Generic;

namespace Monke.AI
{
    /// <summary>
    /// AI state machine states
    /// </summary>
    public enum AIState
    {
        Idle,           // Ready to listen
        Listening,      // STT active, recording
        Processing,     // Sending to AI, waiting for response
        Speaking,       // TTS playing response
        Error           // Something went wrong
    }

    /// <summary>
    /// Available AI service types
    /// </summary>
    public enum AIServiceType
    {
        OpenAI,
        Claude,         // Future support
        LocalLLM        // Future support
    }

    /// <summary>
    /// Represents a single message in the conversation
    /// </summary>
    [Serializable]
    public class ConversationMessage
    {
        public string role;     // "system", "user", or "assistant"
        public string content;  // The message text

        public ConversationMessage(string role, string content)
        {
            this.role = role;
            this.content = content;
        }
    }

    /// <summary>
    /// OpenAI API request structure
    /// </summary>
    [Serializable]
    public class OpenAIRequest
    {
        public string model;
        public List<ConversationMessage> messages;
        public float temperature;
        public int max_tokens;

        public OpenAIRequest(string model, List<ConversationMessage> messages, float temperature, int maxTokens)
        {
            this.model = model;
            this.messages = messages;
            this.temperature = temperature;
            this.max_tokens = maxTokens;
        }
    }

    /// <summary>
    /// OpenAI API response structure
    /// </summary>
    [Serializable]
    public class OpenAIResponse
    {
        public string id;
        public string @object;
        public long created;
        public string model;
        public Choice[] choices;
        public Usage usage;

        [Serializable]
        public class Choice
        {
            public int index;
            public ConversationMessage message;
            public string finish_reason;
        }

        [Serializable]
        public class Usage
        {
            public int prompt_tokens;
            public int completion_tokens;
            public int total_tokens;
        }
    }

    /// <summary>
    /// OpenAI API error response structure
    /// </summary>
    [Serializable]
    public class OpenAIError
    {
        public Error error;

        [Serializable]
        public class Error
        {
            public string message;
            public string type;
            public string param;
            public string code;
        }
    }
}
