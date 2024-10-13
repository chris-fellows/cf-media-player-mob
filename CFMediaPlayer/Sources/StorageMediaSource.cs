using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using static Android.Provider.MediaStore.Audio;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from storage (Internal/SD card)
    /// </summary>
    internal class StorageMediaSource : IMediaSource
    {
        private string _rootPath = String.Empty;

        public StorageMediaSource()
        {
            
        }

        public MediaSourceTypes MediaSourceType => MediaSourceTypes.Storage;

        public void SetSource(string source)
        {
            _rootPath = source;
        }

        public bool IsAvailable
        {
            get
            {
                return !String.IsNullOrEmpty(_rootPath) &&
                    Directory.Exists(_rootPath);
            }
        }

        public List<Artist> GetArtists()
        {
            var artists = new List<Artist>();

            if (Directory.Exists(_rootPath))
            {
                var folders = Directory.GetDirectories(_rootPath);
                foreach (var folder in folders)
                {
                    // Check that folder contains albums
                    var isHasMediaItemCollections = false;
                    foreach (var subFolder in Directory.GetDirectories(folder))
                    {
                        if (MediaUtilities.IsFolderHasAudioFiles(subFolder))
                        {
                            isHasMediaItemCollections = true;
                            break;
                        }
                    }

                    if (isHasMediaItemCollections)
                    {
                        artists.Add(new Artist() { Path = folder, Name = new DirectoryInfo(folder).Name });
                    }
                }
            }

            return artists;
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            var path = Path.Combine(_rootPath, artistName);
            if (Directory.Exists(path))
            {
                var folders = Directory.GetDirectories(path);
                foreach (var folder in folders)
                {
                    if (MediaUtilities.IsFolderHasAudioFiles(folder))
                    {
                        mediaItemCollections.Add(new MediaItemCollection() 
                            { 
                                Path = folder,
                                Name = new DirectoryInfo(folder).Name
                            });
                    }
                }
            }

            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string mediaItemCollectionName)
        {
            var mediaItems = new List<MediaItem>();

            // TODO: Filter by extension
            var path = Path.Combine(_rootPath, artistName, mediaItemCollectionName);
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                foreach(var file in files)
                {
                    if (Array.IndexOf(MediaUtilities.AudioFileExtensions, Path.GetExtension(file).ToLower()) != -1)
                    {
                        mediaItems.Add(new MediaItem() { FilePath = file });
                    }
                }
            }

            return mediaItems;
        }

        public List<MediaItemAction> GetActionsForMediaItem(MediaItem mediaItem)
        {
            var items = new List<MediaItemAction>();

            var item1 = new MediaItemAction()
            {
                Name = "Add to queue",
                File = mediaItem.FilePath,
                SelectedAction = MediaItemActions.AddToQueue
            };
            items.Add(item1);

            var item2 = new MediaItemAction()
            {
                Name = "Clear queue",
                File = mediaItem.FilePath,
                SelectedAction = MediaItemActions.ClearQueue
            };
            items.Add(item2);

            //// Add None
            //if (!items.Any())
            //{
            //    var itemNone = new MediaItemAction()
            //    {
            //        Name = "Playlist actions..."
            //    };
            //    items.Add(itemNone);
            //}

            return items;
        }

        public void ExecuteMediaItemAction(string playlistFile, MediaItem mediaItem, MediaItemActions playlistAction)
        {
            // No action
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();

            var artists = GetArtists();

            foreach(var artist in artists)
            {
                if (SearchUtilities.IsValidSearchResult(artist, searchOptions))
                {
                    searchResults.Add(new SearchResult()
                    {
                        EntityType = EntityTypes.Artist,
                        Name = artist.Name,
                        Artist = artist
                    });
                }

                // Get media item collections
                var mediaItemCollections = GetMediaItemCollectionsForArtist(artist.Name);

                searchResults.AddRange(mediaItemCollections.Where(mic => SearchUtilities.IsValidSearchResult(mic, searchOptions))
                    .Select(mic => new SearchResult()
                    {
                        EntityType = EntityTypes.MediaItemCollection,
                        Name = mic.Name,
                        Artist = artist,
                        MediaItemCollection = mic
                    }));
                
                // Check each media item collection
                foreach(var mediaItemCollection in mediaItemCollections)
                {
                    // Get media items for collection
                    var mediaItems = GetMediaItemsForMediaItemCollection(artist.Name, mediaItemCollection.Name);

                    searchResults.AddRange(mediaItems.Where(mi => SearchUtilities.IsValidSearchResult(mi, searchOptions))
                        .Select(mi => new SearchResult()
                        {
                            EntityType = EntityTypes.MediaItem,
                            Name = mi.Name,
                            Artist = artist,
                            MediaItemCollection = mediaItemCollection,
                            MediaItem = mi
                        }));
                }
            }
            
            return searchResults;
        }        
    }
}
