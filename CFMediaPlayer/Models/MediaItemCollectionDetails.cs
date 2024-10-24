using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Models
{
    public class MediaItemCollectionDetails
    {
        public Artist Artist { get; set; } = new Artist();

        public MediaItemCollection MediaItemCollection { get; set; } = new MediaItemCollection();

        public List<MediaItem> MediaItems { get; set; } = new List<MediaItem>();
    }
}
