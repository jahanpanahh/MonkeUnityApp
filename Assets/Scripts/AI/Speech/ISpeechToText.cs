using System;

namespace Monke.AI.Speech
{
    /// <summary>
    /// Interface for Speech-to-Text implementations
    /// Allows platform-specific implementations (iOS, Android, etc.)
    /// </summary>
    public interface ISpeechToText
    {
        /// <summary>
        /// Event fired when speech is recognized
        /// </summary>
        event Action<string> OnTextRecognized;

        /// <summary>
        /// Event fired when an error occurs
        /// </summary>
        event Action<string> OnError;

        /// <summary>
        /// Whether currently recording
        /// </summary>
        bool IsRecording { get; }

        /// <summary>
        /// Request microphone and speech recognition permissions
        /// </summary>
        void RequestPermission();

        /// <summary>
        /// Start listening and recording speech
        /// </summary>
        /// <returns>True if successfully started, false otherwise</returns>
        bool StartRecording();

        /// <summary>
        /// Stop listening and recording
        /// </summary>
        void StopRecording();
    }
}
