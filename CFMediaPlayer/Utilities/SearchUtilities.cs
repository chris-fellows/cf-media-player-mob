using CFMediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Utilities
{
    public static class SearchUtilities
    {
        public static bool IsValidSearchResult(Artist artist, SearchOptions searchOptions)
        {
            return (artist.Name.Contains(searchOptions.Text, StringComparison.InvariantCulture));            
        }

        public static bool IsValidSearchResult(MediaItemCollection mediaItemCollection, SearchOptions searchOptions)
        {
            return (mediaItemCollection.Name.Contains(searchOptions.Text, StringComparison.InvariantCulture));
        }

        public static bool IsValidSearchResult(MediaItem mediaItem, SearchOptions searchOptions)
        {
            return (mediaItem.Name.Contains(searchOptions.Text, StringComparison.InvariantCulture));
        }
    }
}
