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

        //public string Name => !String.IsNullOrEmpty(Path) && Directory.Exists(Path) ? 
        //                new DirectoryInfo(Path).Name : String.Empty;

        public string Name { get; set; } = String.Empty;
    }
}
