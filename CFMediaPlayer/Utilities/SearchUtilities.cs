using CFMediaPlayer.Models;

namespace CFMediaPlayer.Utilities
{
    /// <summary>
    /// Search utilities
    /// </summary>
    internal static class SearchUtilities
    {
        public static bool IsValidSearchResult(Artist artist, SearchOptions searchOptions)
        {
            return (artist.Name.Contains(searchOptions.Text, StringComparison.InvariantCultureIgnoreCase));            
        }

        public static bool IsValidSearchResult(MediaItemCollection mediaItemCollection, SearchOptions searchOptions)
        {
            return (mediaItemCollection.Name.Contains(searchOptions.Text, StringComparison.InvariantCultureIgnoreCase));
        }

        public static bool IsValidSearchResult(MediaItem mediaItem, SearchOptions searchOptions)
        {
            return (mediaItem.Name.Contains(searchOptions.Text, StringComparison.InvariantCultureIgnoreCase));
        }
    }
}
