using Android.Media;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from cloud (OneDrive, Google etc)
    /// 
    /// TODO: Complete this.
    /// </summary>
    public class CloudMediaSource : MediaSourceBase, IMediaSource
    {        
        public CloudMediaSource(ICurrentState currentState, 
                            MediaLocation mediaLocation) : base(currentState, mediaLocation)
        {            
        }

        public string ImagePath => GetImagePathByMediaItemTypes();

        public bool IsAvailable
        {
            get
            {
                // TODO: Check specific cloud
                return true;
            }
        }

        public bool IsDisplayInUI => false; // IsAvailable;

        public bool IsShufflePlayAllowed => false;

        public bool IsAutoPlayNextAllowed => false;

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
   
        public List<MediaAction> GetMediaActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var items = new List<MediaAction>();          

            return items;
        }
        public List<MediaAction> GetMediaActionsForMediaLocation(MediaLocation mediaLocation)
        {
            var mediaActions = new List<MediaAction>();
            return mediaActions;
        }

        public void ExecuteMediaAction(MediaAction mediaItemAction)                                
        {
            
        }        

        public List<SearchResult> Search(SearchOptions searchOptions, CancellationToken cancellationToken)
        {
            var searchResults = new List<SearchResult>();                            

            return searchResults;
        }

        public List<Tuple<Artist, MediaItemCollection>> GetAncestorsForMediaItem(MediaItem mediaItem)
        {
            return new();
        }

        public MediaItem? GetMediaItemByFile(string filePath)
        {
            return null;
        }
    }
}
