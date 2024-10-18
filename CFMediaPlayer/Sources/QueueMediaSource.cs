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
        
        public bool IsAvailable => true;        // Always

        public List<Artist> GetArtists(bool includeNonReal)
        {
            return new List<Artist>()
            {
                Artist.InstanceMultiple                
            };
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            if (includeNonReal)
            {
                // Add multiple                
                mediaItemCollections.Insert(0, MediaItemCollection.InstanceMultiple);
            }

            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, bool includeNonReal)
        {
            var mediaItems = new List<MediaItem>();
            mediaItems.AddRange(_mediaItemQueue);

            // Add None if no media item collections           
            if (includeNonReal && !mediaItems.Any())
            {
                mediaItems.Add(MediaItem.InstanceNone);
            }

            return mediaItems;
        }

        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem,
                                                            List<IMediaSource> allMediaSources)
        {
            var items = new List<MediaItemAction>();

            if (mediaItem != null)
            {
                // If queue currently selected then add action to open album
                if (currentMediaLocation.MediaSourceType == MediaSourceTypes.Queue)
                {
                    foreach (IMediaSource mediaSource in allMediaSources.Where(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Storage & ms.IsAvailable))
                    {
                        var ancestors = mediaSource.GetAncestorsForMediaItem(mediaItem);
                        if (ancestors != null)
                        {
                            var item = new MediaItemAction()
                            {
                                ActionToExecute = MediaItemActions.OpenMediaItemCollection,
                                MediaLocationName = mediaSource.MediaLocation.Name,
                                File = mediaItem.FilePath,
                                ImagePath = "picture.png",
                                Name = String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.OpenMediaItemCollection)].ToString(),
                                        ancestors.Item2.Name)
                            };
                            items.Add(item);
                            break;
                        }
                    }
                }

                if (_mediaItemQueue.Any(mi => mi.FilePath == mediaItem.FilePath))   // Queued
                {
                    var item3 = new MediaItemAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.RemoveFromQueue)].ToString(),
                        File = mediaItem.FilePath,
                        ActionToExecute = MediaItemActions.RemoveFromQueue,
                        ImagePath = "cross.png"
                    };
                    items.Add(item3);
                }
                else   // Not queue
                {
                    var item1 = new MediaItemAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.AddToQueueEnd)].ToString(),
                        File = mediaItem.FilePath,
                        ActionToExecute = MediaItemActions.AddToQueueEnd,
                        ImagePath = "plus.png",
                    };
                    items.Add(item1);

                    var item2 = new MediaItemAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.AddToQueueNext)].ToString(),
                        File = mediaItem.FilePath,
                        ActionToExecute = MediaItemActions.AddToQueueNext,
                        ImagePath = "plus.png"
                    };
                    items.Add(item2);
                }            
            }

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
                case MediaItemActions.RemoveFromQueue:
                    _mediaItemQueue.RemoveAll(mi => mi.FilePath == mediaItem.FilePath);
                    break;
            }       
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();         

            searchResults.AddRange(_mediaItemQueue.Where(mi => SearchUtilities.IsValidSearchResult(mi, searchOptions))
                   .Select(mi => new SearchResult()
                   {
                       EntityType = EntityTypes.MediaItem,
                       Name = mi.Name,
                       Artist = Artist.InstanceNone,
                       MediaItemCollection = MediaItemCollection.InstanceNone,
                       MediaItem = mi
                   }));        

            return searchResults;
        }

        public Tuple<Artist, MediaItemCollection>? GetAncestorsForMediaItem(MediaItem mediaItem)
        {
            // Only used for storage source where files are physically stored
            return null;
        }
    }
}
