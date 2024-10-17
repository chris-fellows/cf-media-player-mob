namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Artist details
    /// </summary>
    public class Artist
    {
        /// <summary>
        /// Folder containing media item collections for artist
        /// </summary>
        public string Path { get; set; } = String.Empty;
     
        /// <summary>
        /// Artist name
        /// </summary>
        public string Name { get; set; } = String.Empty;
    }
}
