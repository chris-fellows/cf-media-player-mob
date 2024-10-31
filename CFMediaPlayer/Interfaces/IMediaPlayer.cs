using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Media player interface
    /// </summary>
    public interface IMediaPlayer
    {
        /// <summary>
        /// Media player events
        /// </summary>
        public MediaPlayerEvents Events { get; }

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

        ///// <summary>
        ///// Sets debug action
        ///// </summary>
        ///// <param name="action"></param>
        //void SetDebugAction(Action<string> action);

        /// <summary>
        /// Whether media is currently playing
        /// </summary>
        bool IsPlaying { get; }

        /// <summary>
        /// Whether media is starting
        /// </summary>
        bool IsStarting { get; }

        /// <summary>
        /// Whether media is current paused
        /// </summary>
        bool IsPaused { get; }

        /// <summary>
        /// Current file being played (if any)
        /// </summary>
        string CurrentFilePath { get; }

        //void ApplyEqualizerTest();
        
        /// <summary>
        /// Audio equalizer
        /// </summary>
        IAudioEqualizer AudioEqualizer { get; }       

        /// <summary>
        /// Remaining time for current media item
        /// </summary>
        TimeSpan RemainingTime { get; }

        /// <summary>
        /// Duration of current media item
        /// </summary>
        TimeSpan DurationTime { get; }

        /// <summary>
        /// Elapsed time for current media item. Setting will skip to the selected position.
        /// </summary>
        TimeSpan ElapsedTime { get; set; }
    }
}
