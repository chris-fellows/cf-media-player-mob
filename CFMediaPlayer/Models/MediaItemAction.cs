using CFMediaPlayer.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    public class MediaItemAction
    {
        public string Name { get; set; } = String.Empty;

        public string File { get; set; } = String.Empty;

        public MediaItemActions SelectedAction { get; set; }
    }
}
