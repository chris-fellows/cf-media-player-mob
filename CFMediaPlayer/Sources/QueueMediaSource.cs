using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from queue.
    /// 
    /// Media items can be added to the queue.
    /// </summary>
    public class QueueMediaSource : IMediaSource
    {
        private readonly List<MediaItem> _mediaItemQueue = new List<MediaItem>();

        public QueueMediaSource()
        {
            
        }

        public MediaSourceTypes MediaSourceType => MediaSourceTypes.Queue;

        public bool IsAvailable
        {
            get
            {
                return true;
            }
        }

        public void SetSource(string source)
        {
            
        }


        public List<Artist> GetArtists()
        {
            var artists = new List<Artist>();
            artists.Add(new Artist() { Path = "None", Name = "None" });   // Dummy artists
            return artists;
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName)
        {
            var mediaItemCollections = new List<MediaItemCollection>();
            mediaItemCollections.Add(new MediaItemCollection() { Path = "None", Name = "None" });
            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string mediaItemCollectionName)
        {
            var mediaItems = new List<MediaItem>();

            mediaItems.AddRange(_mediaItemQueue);

            return mediaItems;
        }

        public List<MediaItemAction> GetActionsForMediaItem(MediaItem mediaItem)
        {
            var items = new List<MediaItemAction>();           

            // Add header
            if (!items.Any())
            {
                var itemNone = new MediaItemAction()
                {
                    Name = "Playlist actions..."
                };
                items.Add(itemNone);
            }

            return items;
        }

        public void ExecuteMediaItemAction(string playlistFile, MediaItem mediaItem, MediaItemActions mediaItemAction)
        {
            switch (mediaItemAction)
            {
                case MediaItemActions.AddToQueue:
                    _mediaItemQueue.Add(mediaItem);
                    break;
                case MediaItemActions.ClearQueue:
                    _mediaItemQueue.Clear();
                    break;
            }       
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();

            var mediaItems = GetMediaItemsForMediaItemCollection("", "");

            searchResults.AddRange(mediaItems.Where(mi => SearchUtilities.IsValidSearchResult(mi, searchOptions))
                   .Select(mi => new SearchResult()
                   {
                       EntityType = EntityTypes.MediaItem,
                       Name = mi.Name,
                       Artist = new Artist() { Name = "None" },
                       MediaItemCollection = new MediaItemCollection() { Name= "None" },
                       MediaItem = mi
                   }));        

            return searchResults;
        }
    }
}
