using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Artist details
    /// </summary>
    public class Artist
    {
        public string Path { get; set; } = String.Empty;

        public string Name => new DirectoryInfo(Path).Name;
    }
}
