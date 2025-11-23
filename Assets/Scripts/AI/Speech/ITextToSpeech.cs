using System;

namespace Monke.AI.Speech
{
    /// <summary>
    /// Interface for Text-to-Speech implementations
    /// Allows platform-specific implementations (iOS, Android, etc.)
    /// </summary>
    public interface ITextToSpeech
    {
        /// <summary>
        /// Event fired when speech playback finishes
        /// </summary>
        event Action OnSpeechFinished;

        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        event Action<string> OnError;

        /// <summary>
        /// Whether currently speaking
        /// </summary>
        bool IsSpeaking { get; }

        /// <summary>
        /// Speak the given text
        /// </summary>
        /// <param name="text">Text to speak</param>
        /// <param name="onComplete">Optional callback when speech completes</param>
        void Speak(string text, Action onComplete = null);

        /// <summary>
        /// Stop current speech playback
        /// </summary>
        void Stop();
    }
}
