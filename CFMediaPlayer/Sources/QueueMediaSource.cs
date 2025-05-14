using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System.Diagnostics;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from local queue.
    /// 
    /// The queue starts off empty and the user can add media items to it or clear it. We should consider persisting
    /// the queue to a playlist.
    /// </summary>
    public class QueueMediaSource : MediaSourceBase, IMediaSource
    {
        private const int _maxItems = 100;
        private const int _itemsToAddRandomly = 50;     // Number of media items to add randomly
        private readonly List<MediaItem> _mediaItemQueue = new List<MediaItem>();

        public QueueMediaSource(ICurrentState currentState,
                                MediaLocation mediaLocation) : base(currentState, mediaLocation)
        {
            
        }

        public string ImagePath => GetImagePathByMediaItemTypes();

        public bool IsAvailable => true;        // Always

        public bool IsDisplayInUI => true;      // Always

        public bool IsShufflePlayAllowed => false;    // Play in queue order

        public bool IsAutoPlayNextAllowed => true;

        /// <summary>
        /// Adds media item to queue, removes if too many
        /// </summary>
        /// <param name="mediaItem"></param>
        private void AddMediaItem(MediaItem mediaItem)
        {
            while (_mediaItemQueue.Count >= _maxItems)
            {
                _mediaItemQueue.RemoveAt(0);
            }
            _mediaItemQueue.Add(mediaItem);
        }

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

                    //// Add "Add to queue (Next)" if we're playing item from queue
                    //if (_currentState.CurrentMediaItem != null)   // Current item selected
                    //{
                    //    var queueMediaItem = _mediaItemQueue.FirstOrDefault(mi => mi.FilePath == _currentState.CurrentMediaItem.FilePath);
                    //    if (queueMediaItem != null)  // Current item in queue
                    //    {
                    //        var item2 = new MediaAction()
                    //        {
                    //            MediaLocationName = _mediaLocation.Name,
                    //            Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.AddToQueueNext)].ToString(),
                    //            MediaItemFile = mediaItem.FilePath,
                    //            ActionType = MediaActionTypes.AddToQueueNext,
                    //            ImagePath = "plus.png"
                    //        };
                    //        mediaActions.Add(item2);
                    //    }                        
                    //}
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

            // Add action to add random items
            var mediaAction2 = new MediaAction()
            {
                ActionType = MediaActionTypes.AddRandomItemsToQueue,
                MediaLocationName = mediaLocation.Name,
                ImagePath = "plus.png",
                Name = String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.AddRandomItemsToQueue)].ToString(), _itemsToAddRandomly)
            };
            mediaActions.Add(mediaAction2);

            return mediaActions;
        }

        /// <summary>
        /// Adds N random music items to queue
        /// </summary>
        private void AddRandomItemsToQueue(int itemsToAdd)
        {
            // Add N media items            
            var random = new Random();
            var queueMediaItems = new List<MediaItem>();
            var artistsByMediaSource = new Dictionary<string, List<Artist>>();  // Key=MediaLocation.Name
            var stopwatch = new Stopwatch();
            stopwatch.Start();

            do
            {
                foreach (var mediaSource in _allMediaSources.Where(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Storage &&
                                                       ms.MediaLocation.MediaItemTypes.Contains(MediaItemTypes.Music) &&
                                                       ms.IsAvailable))
                {                    
                    // Check if artists cached for media source
                    var artists = artistsByMediaSource.ContainsKey(mediaSource.MediaLocation.Name) ?
                                    artistsByMediaSource[mediaSource.MediaLocation.Name] : null;

                    if (artists == null)   // Not cached, cache artists for media location
                    {
                        artists = mediaSource.GetArtists(false);
                        artistsByMediaSource.Add(mediaSource.MediaLocation.Name, artists);
                    }
                    if (artists.Any())
                    {
                        // Select random artist
                        var artist = artists[random.Next(0, artists.Count - 1)];

                        // Get media item collections for artist
                        var mediaItemCollections = mediaSource.GetMediaItemCollectionsForArtist(artist, false);
                        if (mediaItemCollections.Any())
                        {
                            // Select random media item collection
                            var mediaItemCollection = mediaItemCollections[random.Next(0, mediaItemCollections.Count - 1)];

                            // Get media items for media item collection
                            var mediaItems = mediaSource.GetMediaItemsForMediaItemCollection(artist, mediaItemCollection, false);
                            if (mediaItems.Any())
                            {
                                // Get random media item
                                var mediaItemX = mediaItems[random.Next(0, mediaItems.Count - 1)];

                                // Add to list for queue
                                if (!queueMediaItems.Any(mi => mi.FilePath == mediaItemX.FilePath) &&   // Not in list to add to queue
                                    !_mediaItemQueue.Any(mi => mi.FilePath == mediaItemX.FilePath))     // Not in queue
                                {
                                    // Set image to be album image
                                    mediaItemX.ImagePath = mediaItemCollection.ImagePath;

                                    queueMediaItems.Add(mediaItemX);
                                }
                            }
                        }
                    }
                }
                System.Threading.Thread.Sleep(5);
            } while (queueMediaItems.Count < itemsToAdd &&
                    stopwatch.Elapsed < TimeSpan.FromSeconds(10));   // Avoid infinite loop if insufficient media items (TODO: Do it a different way)
            artistsByMediaSource.Clear();

            // Add to queue
            queueMediaItems.ForEach(mi => AddMediaItem(mi));            
        }

        public void ExecuteMediaAction(MediaAction mediaAction)
        {
            // Load media item if any
            MediaItem? mediaItem = String.IsNullOrEmpty(mediaAction.MediaItemFile) ? null :
                                    GetMediaItemByFileFromOriginalSource(mediaAction.MediaItemFile);

            SystemEventTypes systemEventType = SystemEventTypes.Unknown;
            switch (mediaAction.ActionType)
            {
                case MediaActionTypes.AddRandomItemsToQueue:
                    {
                        AddRandomItemsToQueue(_itemsToAddRandomly);
                        systemEventType = SystemEventTypes.QueueItemsAdded;
                    }
                    break;
                case MediaActionTypes.AddToQueueEnd:
                    {
                        // Clone media item, set album image for display
                        var mediaItemCopy = (MediaItem)mediaItem.Clone();
                        mediaItemCopy.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                        AddMediaItem(mediaItemCopy);                       
                        systemEventType = SystemEventTypes.QueueItemAdded;
                    }
                    break;
                //case MediaActionTypes.AddToQueueNext:
                //    {
                //        // Clone media item, set album image for display
                //        var mediaItemCopy = (MediaItem)mediaItem.Clone();
                //        mediaItemCopy.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                        
                //        // Add media item after current queue item being played
                //        if (_currentState.CurrentMediaItem != null)
                //        {
                //            var queueMediaItem = _mediaItemQueue.FirstOrDefault(mi => mi.FilePath == _currentState.CurrentMediaItem.FilePath);
                //            if (queueMediaItem != null)
                //            {
                //                _mediaItemQueue.Insert(_mediaItemQueue.IndexOf(queueMediaItem) + 1, mediaItemCopy);
                //            }
                //        }                        
                //    }
                //    break;
                case MediaActionTypes.ClearQueue:
                    _mediaItemQueue.Clear();
                    systemEventType = SystemEventTypes.QueueCleared;
                    break;
                case MediaActionTypes.RemoveFromQueue:
                    _mediaItemQueue.RemoveAll(mi => mi.FilePath == mediaItem.FilePath);
                    systemEventType = SystemEventTypes.QueueItemRemoved;
                    break;
            }

            // Notify queue updated
            _currentState.Events.RaiseOnQueueUpdated(systemEventType, mediaItem);            
        }
       
        public List<SearchResult> Search(SearchOptions searchOptions, CancellationToken cancellationToken)
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
