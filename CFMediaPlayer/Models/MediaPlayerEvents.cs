using CFMediaPlayer.Enums;
using CFMediaPlayer.Exceptions;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Events for media player
    /// </summary>
    public class MediaPlayerEvents
    {
        public delegate void StatusChange(MediaPlayerStatuses status, MediaPlayerException? mediaPlayerException);
        public StatusChange? OnStatusChange;

        public delegate void Debug(string message);
        public Debug? OnDebug;
    }
}
