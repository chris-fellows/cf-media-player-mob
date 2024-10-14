using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Search result
    /// </summary>
    public class SearchResult
    {
        public EntityTypes EntityType { get; set; }

        public string Name { get; set; } = String.Empty;           

        public string MediaLocationName { get; set; } = String.Empty;

        public Artist? Artist { get; set; }

        public MediaItemCollection? MediaItemCollection { get; set; }

        public MediaItem? MediaItem { get; set; }        
    }
}
