using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System.Linq;
using static Android.Provider.MediaStore.Audio;

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

        public QueueMediaSource(MediaLocation mediaLocation) : base(mediaLocation)
        {
            
        }

        public string ImagePath => InternalUtilities.DefaultImagePath;

        public bool IsAvailable => true;        // Always

        public bool IsDisplayInUI => true;

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

        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var items = new List<MediaItemAction>();

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
                            var item = new MediaItemAction()
                            {
                                ActionToExecute = MediaItemActions.OpenMediaItemCollection,
                                MediaLocationName = mediaSource.MediaLocation.Name,
                                MediaItemFile = mediaItem.FilePath,
                                ImagePath = ancestors.Item2.ImagePath,                                
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
                        MediaItemFile = mediaItem.FilePath,
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
                        MediaItemFile = mediaItem.FilePath,
                        ActionToExecute = MediaItemActions.AddToQueueEnd,
                        ImagePath = "plus.png",
                    };
                    items.Add(item1);

                    var item2 = new MediaItemAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.AddToQueueNext)].ToString(),
                        MediaItemFile = mediaItem.FilePath,
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
    }
}
