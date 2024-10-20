using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Location of media (Local store, playlist, queue etc)
    /// </summary>
    public class MediaLocation
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Indicates media source name (IMediaSource.Name) for accesing media items
        /// </summary>
        public MediaSourceTypes MediaSourceType { get; set; }
        
        /// <summary>
        /// Sources for media items
        /// </summary>
        public List<string> Sources { get; set; } = new List<string>();

        /// <summary>
        /// Media item types
        /// </summary>
        public List<MediaItemTypes> MediaItemTypes { get; set; } = new List<MediaItemTypes>();
    }
}
