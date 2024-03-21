using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Collection of media items (Album, playlist etc)
    /// </summary>
    public class MediaItemCollection
    {
        public string Path { get; set; } = String.Empty;

        public string Name => new DirectoryInfo(Path).Name;
        
        /// <summary>
        /// Path to image (Album artwork)
        /// </summary>
        public string ImagePath
        {
            get
            {
                if (String.IsNullOrEmpty(Path)) return String.Empty;
                var files = Directory.GetFiles(Path, "Folder.jpg");
                if (files.Any()) return files[0];
                return String.Empty;
            }
        }
    }
}
