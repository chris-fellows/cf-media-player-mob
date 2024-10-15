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

        public List<Artist> GetArtists()
        {
            var artists = new List<Artist>();
            artists.Add(new Artist() { Name = LocalizationResources.Instance["None"].ToString() });   // Dummy artists
            return artists;
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName)
        {
            var mediaItemCollections = new List<MediaItemCollection>();
            mediaItemCollections.Add(new MediaItemCollection() { Name = LocalizationResources.Instance["None"].ToString() });
            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string mediaItemCollectionName)
        {
            var mediaItems = new List<MediaItem>();

            return mediaItems;
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
    }
}
