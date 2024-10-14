using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from queue.
    /// 
    /// The queue starts off empty and the user can add media items to it or clear it.
    /// </summary>
    public class QueueMediaSource : IMediaSource
    {
        private readonly MediaLocation _mediaLocation;
        private readonly List<MediaItem> _mediaItemQueue = new List<MediaItem>();

        public QueueMediaSource(MediaLocation mediaLocation)
        {
            _mediaLocation = mediaLocation;
        }

        public MediaLocation MediaLocation => _mediaLocation;
        
        public bool IsAvailable => true;        

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

            mediaItems.AddRange(_mediaItemQueue);

            //if (!mediaItems.Any())
            //{
            //    mediaItems.Add(new MediaItem() { Name = "None" });
            //}

            return mediaItems;
        }

        public List<MediaItemAction> GetActionsForMediaItem(MediaItem mediaItem)
        {
            var items = new List<MediaItemAction>();

            if (mediaItem != null)
            {
                var item1 = new MediaItemAction()
                {
                    MediaLocationName = _mediaLocation.Name,
                    Name = "Add to queue (End)",
                    File = mediaItem.FilePath,
                    ActionToExecute = MediaItemActions.AddToQueueEnd
                };
                items.Add(item1);

                var item2 = new MediaItemAction()
                {
                    MediaLocationName = _mediaLocation.Name,
                    Name = "Add to queue (Next)",
                    File = mediaItem.FilePath,
                    ActionToExecute = MediaItemActions.AddToQueueNext
                };
                items.Add(item1);
            }

            var item3 = new MediaItemAction()
            {
                MediaLocationName = _mediaLocation.Name,
                Name = "Clear queue",
                //File = ""
                ActionToExecute = MediaItemActions.ClearQueue
            };
            items.Add(item3);

            //// Add header
            //if (!items.Any())
            //{
            //    var itemNone = new MediaItemAction()
            //    {
            //        Name = "None"
            //    };
            //    items.Add(itemNone);
            //}

            return items;
        }

        public void ExecuteMediaItemAction(MediaItem mediaItem, MediaItemAction mediaItemAction)
        {
            switch (mediaItemAction.ActionToExecute)
            {
                case MediaItemActions.AddToQueueEnd:
                    _mediaItemQueue.Add(mediaItem);
                    break;
                case MediaItemActions.AddToQueueNext:
                    _mediaItemQueue.Insert(0, mediaItem);
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
                       Artist = new Artist() { Name = LocalizationResources.Instance["None"].ToString() },
                       MediaItemCollection = new MediaItemCollection() { Name= LocalizationResources.Instance["None"].ToString() },
                       MediaItem = mi
                   }));        

            return searchResults;
        }
    }
}
