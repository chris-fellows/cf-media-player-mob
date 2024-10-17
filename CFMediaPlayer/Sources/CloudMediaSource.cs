using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from cloud (OneDrive, Google etc)
    /// </summary>
    public class CloudMediaSource : IMediaSource
    {
        private readonly MediaLocation _mediaLocation;

        public CloudMediaSource(MediaLocation mediaLocation)
        {
            _mediaLocation = mediaLocation;
        }

        public MediaLocation MediaLocation => _mediaLocation;
        
        public bool IsAvailable
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

        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, bool includeNonReal)
        {
            // TODO: Implement this
            return new List<MediaItem>();
        }

        //public List<MediaItem> GetMediaItemsForArtist(Artist artist, bool includeNonReal)
        //{
        //    return new List<MediaItem>();            
        //}

        //public List<MediaItem> GetMediaItemsForAllArtists(bool includeNonReal)
        //{
        //    return new List<MediaItem>();
        //}

        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem,
                                                    List<IMediaSource> allMediaSources)
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
    }
}
