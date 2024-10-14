namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Search options
    /// </summary>
    public class SearchOptions
    {
        /// <summary>
        /// Text to search
        /// </summary>
        public string Text { get; set; } = String.Empty;

        /// <summary>
        /// Media locations to search in
        /// </summary>
        public List<MediaLocation> MediaLocations { get; set; } = new();
    }
}
