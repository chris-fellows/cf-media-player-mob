using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from cloud (OneDrive, Google etc)
    /// </summary>
    public class CloudMediaSource : MediaSourceBase, IMediaSource
    {        
        public CloudMediaSource(MediaLocation mediaLocation) : base(mediaLocation)
        {            
        }

        public string ImagePath => InternalUtilities.DefaultImagePath;

        public bool IsAvailable
        {
            get
            {
                // TODO: Check specific cloud
                return true;
            }
        }

        public bool HasMediaItems
        {
            get
            {
                // TODO: Check specific cloud
                return true;
            }
        }

        public List<Artist> GetArtists(bool includeNonReal)
        {
            return new List<Artist>();
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal)
        {
            return new List<MediaItemCollection>();
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, 
                            bool includeNonReal)
        {
            // TODO: Implement this
            return new List<MediaItem>();
        }
   
        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var items = new List<MediaItemAction>();          

            return items;
        }

        public void ExecuteMediaItemAction(MediaItem mediaItem, MediaItemAction mediaItemAction)                                
        {
            
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();                            

            return searchResults;
        }

        public List<Tuple<Artist, MediaItemCollection>> GetAncestorsForMediaItem(MediaItem mediaItem)
        {
            return new();
        }
    }
}
