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
    public class QueueMediaSource : MediaSourceBase, IMediaSource
    {        
        private readonly List<MediaItem> _mediaItemQueue = new List<MediaItem>();

        public QueueMediaSource(ICurrentState currentState,
                                MediaLocation mediaLocation) : base(currentState, mediaLocation)
        {
            
        }

        public string ImagePath => InternalUtilities.DefaultImagePath;

        public bool IsAvailable => true;        // Always

        public bool IsDisplayInUI => true;

        public bool IsShufflePlayAllowed => false;    // Play in queue order

        public bool IsAutoPlayNextAllowed => true;

        public List<Artist> GetArtists(bool includeNonReal)
        {
            if (includeNonReal)
            {
                return new List<Artist>()
                {
                    Artist.InstanceAll
                };
            }
            return new List<Artist>();
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            if (includeNonReal)
            {                
                mediaItemCollections.Insert(0, MediaItemCollection.InstanceAll);
            }

            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection,
                                            bool includeNonReal)
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

        public List<MediaAction> GetMediaActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var mediaActions = new List<MediaAction>();

            if (mediaItem != null)
            {
                // If queue currently selected then add action to open album
                if (currentMediaLocation.MediaSourceType == MediaSourceTypes.Queue)
                {
                    foreach (IMediaSource mediaSource in _allMediaSources.Where(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Storage & ms.IsAvailable))
                    {
                        var ancestors = mediaSource.GetAncestorsForMediaItem(mediaItem).FirstOrDefault();
                        if (ancestors != null)
                        {
                            var mediaAction = new MediaAction()
                            {
                                ActionType = MediaActionTypes.OpenMediaItemCollection,
                                MediaLocationName = mediaSource.MediaLocation.Name,
                                MediaItemFile = mediaItem.FilePath,
                                //ImagePath = ancestors.Item2.ImagePath,                                
                                ImagePath = "picture.png",  // No need to display album image, it's the main logo image
                                Name = String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.OpenMediaItemCollection)].ToString(),
                                        ancestors.Item2.Name)
                            };
                            mediaActions.Add(mediaAction);
                            break;
                        }
                    }
                }

                if (_mediaItemQueue.Any(mi => mi.FilePath == mediaItem.FilePath))   // Queued
                {
                    var item3 = new MediaAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.RemoveFromQueue)].ToString(),
                        MediaItemFile = mediaItem.FilePath,
                        ActionType = MediaActionTypes.RemoveFromQueue,
                        ImagePath = "cross.png"
                    };
                    mediaActions.Add(item3);
                }
                else   // Not queue
                {
                    var item1 = new MediaAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.AddToQueueEnd)].ToString(),
                        MediaItemFile = mediaItem.FilePath,
                        ActionType = MediaActionTypes.AddToQueueEnd,
                        ImagePath = "plus.png",
                    };
                    mediaActions.Add(item1);

                    var item2 = new MediaAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.AddToQueueNext)].ToString(),
                        MediaItemFile = mediaItem.FilePath,
                        ActionType = MediaActionTypes.AddToQueueNext,
                        ImagePath = "plus.png"
                    };
                    mediaActions.Add(item2);
                }            
            }

            return mediaActions;
        }

        public List<MediaAction> GetMediaActionsForMediaLocation(MediaLocation mediaLocation)
        {
            var mediaActions = new List<MediaAction>();

            if (_mediaItemQueue.Any())
            {
                // Add action to clear queue
                var mediaAction = new MediaAction()
                {
                    ActionType = MediaActionTypes.ClearQueue,
                    MediaLocationName = mediaLocation.Name,
                    ImagePath = "cross.png",
                    Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.ClearQueue)].ToString()
                };
                mediaActions.Add(mediaAction);
            }

            return mediaActions;
        }

        public void ExecuteMediaAction(MediaAction mediaAction)
        {
            // Load media item
            MediaItem? mediaItem = String.IsNullOrEmpty(mediaAction.MediaItemFile) ? null : GetMediaItemByFileFromOriginalSource(mediaAction.MediaItemFile);

            switch (mediaAction.ActionType)
            {
                case MediaActionTypes.AddToQueueEnd:
                    {
                        // Clone media item, set album image for display
                        var mediaItemCopy = (MediaItem)mediaItem.Clone();
                        mediaItemCopy.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                        _mediaItemQueue.Add(mediaItemCopy);
                    }
                    break;
                case MediaActionTypes.AddToQueueNext:
                    {
                        // Clone media item, set album image for display
                        var mediaItemCopy = (MediaItem)mediaItem.Clone();
                        mediaItemCopy.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                        _mediaItemQueue.Insert(0, mediaItemCopy);
                    }
                    break;
                case MediaActionTypes.ClearQueue:
                    _mediaItemQueue.Clear();
                    break;
                case MediaActionTypes.RemoveFromQueue:
                    _mediaItemQueue.RemoveAll(mi => mi.FilePath == mediaItem.FilePath);
                    break;
            }

            // Notify queue updated
            if (_currentState.QueueUpdatedAction != null)
            {
                _currentState.QueueUpdatedAction();
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
                       MediaItem = mi,                
                       ImagePath = GetMediaItemCollectionImagePath(mi)
                   }));

            searchResults = searchResults.OrderBy(s => s.Name).ToList();

            return searchResults;
        }

        public List<Tuple<Artist, MediaItemCollection>> GetAncestorsForMediaItem(MediaItem mediaItem)
        {
            // Only used for storage source where files are physically stored
            return new();
        }


        ///// <summary>
        ///// Gets media item collection image path for media item
        ///// </summary>
        ///// <param name="mediaItem"></param>
        ///// <param name="allMediaSources"></param>
        ///// <returns></returns>
        //private string GetMediaItemCollectionImagePath(MediaItem mediaItem, List<IMediaSource> allMediaSources)
        //{
        //    foreach (var mediaSource in allMediaSources.Where(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Storage))
        //    {
        //        var ancestors = mediaSource.GetAncestorsForMediaItem(mediaItem);
        //        if (ancestors != null && !String.IsNullOrEmpty(ancestors.Item2.ImagePath))
        //        {
        //            return ancestors.Item2.ImagePath;
        //        }
        //    }
        //    return null;
        //}

        public MediaItem? GetMediaItemByFile(string filePath)
        {
            return null;
        }
    }
}
