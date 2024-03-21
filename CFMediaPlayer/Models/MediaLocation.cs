using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Location of media (Local store, playlist etc)
    /// </summary>
    public class MediaLocation
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// Indicates media source name (IMediaSource.Name) for accesing media items
        /// </summary>
        public string MediaSourceName { get; set; }

        public string RootFolderPath { get; set; }
    }
}
