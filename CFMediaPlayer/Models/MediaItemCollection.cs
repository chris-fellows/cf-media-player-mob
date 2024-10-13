namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Collection of media items (Album, playlist etc)
    /// </summary>
    public class MediaItemCollection
    {
        public string Path { get; set; } = String.Empty;

        //public string Name => !String.IsNullOrEmpty(Path) && Directory.Exists(Path) ?
        //                new DirectoryInfo(Path).Name : String.Empty; 

        public string Name { get; set; } = String.Empty;
        
        /// <summary>
        /// Path to image (Album artwork)
        /// </summary>
        public string ImagePath
        {
            get
            {
                if (String.IsNullOrEmpty(Path) || !Directory.Exists(Path)) return String.Empty;
                var files = Directory.GetFiles(Path, "Folder.jpg");
                if (files.Any()) return files[0];
                return String.Empty;
            }
        }
    }
}
