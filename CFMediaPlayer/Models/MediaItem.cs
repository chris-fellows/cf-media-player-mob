using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Media item
    /// </summary>
    public class MediaItem
    {        
        public string Path { get; set; } = String.Empty;

        public string Name => System.IO.Path.GetFileName(Path);
    }
}
