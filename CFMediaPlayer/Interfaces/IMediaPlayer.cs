﻿using CFMediaPlayer.Enums;
using CFMediaPlayer.Exceptions;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Media player interface
    /// </summary>
    public interface IMediaPlayer
    {
        /// <summary>
        /// Plays media
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="action"></param>
        void Play(string filePath, Action<System.Exception> action);

        /// <summary>
        /// Pauses media
        /// </summary>
        void Pause();

        /// <summary>
        /// Stops media
        /// </summary>
        void Stop();

        /// <summary>
        /// Elapsed playing time
        /// </summary>
        /// <returns></returns>
        TimeSpan GetElapsedPlayTime();

        /// <summary>
        /// Sets elapsed playing time (Jumps to time)
        /// </summary>
        /// <param name="elapsedPlayTime"></param>
        void SetElapsedPlayTime(TimeSpan elapsedPlayTime);

        TimeSpan GetTotalDuration();        

        /// <summary>
        /// Sets status action
        /// </summary>
        /// <param name="action"></param>
        void SetStatusAction(Action<MediaPlayerStatuses, MediaPlayerException?> action);

        /// <summary>
        /// Sets debug action
        /// </summary>
        /// <param name="action"></param>
        void SetDebugAction(Action<string> action);

        /// <summary>
        /// Whether media is currently playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Whether media is current paused
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Current file being played (if any)
        /// </summary>
        string CurrentFilePath { get; }

        void ApplyEqualizerTest();
        
        /// <summary>
        /// Audio equalizer
        /// </summary>
        IAudioEqualizer AudioEqualizer { get; }
    }
}
