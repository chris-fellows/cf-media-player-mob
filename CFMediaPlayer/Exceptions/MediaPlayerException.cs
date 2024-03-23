using System.Globalization;

namespace CFMediaPlayer.Exceptions
{  
    /// <summary>
   /// General media player locker exception.
   /// </summary>
   /// <remarks>Use this when there is no more specific exception</remarks>
    public class MediaPlayerException : Exception
    {
        public MediaPlayerException()
        {
        }

        public MediaPlayerException(string message) : base(message)
        {
        }

        public MediaPlayerException(string message, Exception innerException) : base(message, innerException)
        {
        }

        public MediaPlayerException(string message, params object[] args)
            : base(string.Format(CultureInfo.CurrentCulture, message, args))
        {
        }
    }
}
