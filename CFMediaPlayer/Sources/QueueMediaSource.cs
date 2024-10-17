﻿using CFMediaPlayer.Enums;
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
                new Artist() { Name = LocalizationResources.Instance["MultipleText"].ToString() }
            };
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            if (includeNonReal)
            {
                // Add multiple                
                mediaItemCollections.Insert(0, new MediaItemCollection()
                {
                    Name = LocalizationResources.Instance["MultipleText"].ToString(),
                });                
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
                mediaItems.Add(new MediaItem()
                {
                    Name = LocalizationResources.Instance["NoneText"].ToString(),
                });
            }

            return mediaItems;
        }

        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem,
                                                            List<IMediaSource> allMediaSources)
        {
            var items = new List<MediaItemAction>();

            if (mediaItem != null)
            {
                if (_mediaItemQueue.Any(mi => mi.FilePath == mediaItem.FilePath))   // Queued
                {
                    var item3 = new MediaItemAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.RemoveFromQueue)].ToString(),
                        File = mediaItem.FilePath,
                        ActionToExecute = MediaItemActions.RemoveFromQueue
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
                        ActionToExecute = MediaItemActions.AddToQueueEnd
                    };
                    items.Add(item1);

                    var item2 = new MediaItemAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.AddToQueueNext)].ToString(),
                        File = mediaItem.FilePath,
                        ActionToExecute = MediaItemActions.AddToQueueNext
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
                       Artist = new Artist() { Name = LocalizationResources.Instance["NoneText"].ToString() },
                       MediaItemCollection = new MediaItemCollection() { Name= LocalizationResources.Instance["NoneText"].ToString() },
                       MediaItem = mi
                   }));        

            return searchResults;
        }
    }
}
