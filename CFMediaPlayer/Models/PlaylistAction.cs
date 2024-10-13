using CFMediaPlayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    public class PlaylistAction
    {
        public string Name { get; set; } = String.Empty;

        public string File { get; set; } = String.Empty;

        public PlaylistActions SelectedAction { get; set; }
    }
}
