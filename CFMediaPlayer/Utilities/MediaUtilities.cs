using CFMediaPlayer.Models;

namespace CFMediaPlayer.Utilities
{
    /// <summary>
    /// Media utilities
    /// </summary>
    internal class MediaUtilities
    {
        /// <summary>
        /// Whether folder contains audio files in root
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFolderHasAudioFiles(string path)
        {            
            foreach (var extension in AudioFileExtensions)
            {
                if (Directory.GetFiles(path, $"*{extension}").Any())
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] AudioFileExtensions
        {
            get { return new[] { ".flac", ".mp3", ".ogg", ".wma", ".wav" }; }
        }

        public static bool IsNoneMediaItem(MediaItem mediaItem)
        {
            return String.IsNullOrEmpty(mediaItem.FilePath);
        }

        public static bool IsNoneArtist(Artist artist)
        {
            return String.IsNullOrEmpty(artist.Path);
        }

        public static bool IsNoneMediaItemCollection(MediaItemCollection mediaItemCollection)
        {
            return String.IsNullOrEmpty(mediaItemCollection.Path);
        }
    }
}
