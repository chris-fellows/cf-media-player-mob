using System.Xml.Serialization;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Media item
    /// </summary>
    public class MediaItem
    {   
        /// <summary>
        /// Path to media item file
        /// </summary>
        public string FilePath { get; set; } = String.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; } = String.Empty;
    }
}
