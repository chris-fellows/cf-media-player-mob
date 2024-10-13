using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    public class SearchResult
    {
        public EntityTypes EntityType { get; set; }

        public string Name { get; set; } = "Entity Name XYX";

        public string MediaLocationName { get; set; } = String.Empty;

        public Artist? Artist { get; set; }

        public MediaItemCollection? MediaItemCollection { get; set; }

        public MediaItem? MediaItem { get; set; }

        public object Entity { get; set; }
    }
}
