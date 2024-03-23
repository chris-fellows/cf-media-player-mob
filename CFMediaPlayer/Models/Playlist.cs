using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    public class Playlist
    {
        public string Name { get; set; } = String.Empty;

        public List<PlaylistItem> Items = new List<PlaylistItem>();
    }
}
