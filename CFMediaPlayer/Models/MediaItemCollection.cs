namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Collection of media items (Album, playlist etc)
    /// </summary>
    public class MediaItemCollection
    {
        /// <summary>
        /// Folder where media item collection stored
        /// </summary>
        public string Path { get; set; } = String.Empty;        

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = String.Empty;
        
        /// <summary>
        /// Path to image (Album artwork)
        /// </summary>
        public string ImagePath
        {
            get
            {
                if (String.IsNullOrEmpty(Path) || !Directory.Exists(Path)) return String.Empty;
                var files = Directory.GetFiles(Path, "Folder.jpg"); // Hard-coding is fine for the moment
                if (files.Any()) return files[0];
                return String.Empty;
            }
        }
    }
}
