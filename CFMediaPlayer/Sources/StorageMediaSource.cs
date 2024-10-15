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
        private readonly MediaLocation _mediaLocation;
        //private string _rootPath = String.Empty;

        public StorageMediaSource(MediaLocation mediaLocation)
        {
            _mediaLocation = mediaLocation;
        }

        public MediaLocation MediaLocation => _mediaLocation;

        //public MediaSourceTypes MediaSourceType => MediaSourceTypes.Storage;

        //public void SetSource(string source)
        //{
        //    _rootPath = source;
        //}

        public bool IsAvailable
        {
            get
            {
                return !String.IsNullOrEmpty(_mediaLocation.Source) &&
                    Directory.Exists(_mediaLocation.Source);                
            }
        }

        public List<Artist> GetArtists()
        {
            var artists = new List<Artist>();

            if (IsAvailable)
            {
                var folders = Directory.GetDirectories(_mediaLocation.Source);
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

            return artists.OrderBy(a => a.Name).ToList();
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            var path = Path.Combine(_mediaLocation.Source, artistName);
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

            return mediaItemCollections.OrderBy(mic => mic.Name).ToList();
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string mediaItemCollectionName)
        {
            var mediaItems = new List<MediaItem>();

            // TODO: Filter by extension
            var path = Path.Combine(_mediaLocation.Source, artistName, mediaItemCollectionName);
            if (Directory.Exists(path))
            {
                var files = Directory.GetFiles(path);
                foreach(var file in files)
                {
                    if (Array.IndexOf(MediaUtilities.AudioFileExtensions, Path.GetExtension(file).ToLower()) != -1)
                    {
                        mediaItems.Add(new MediaItem() 
                        { 
                            FilePath = file,
                            Name = Path.GetFileName(file)
                        });
                    }
                }
            }

            return mediaItems.OrderBy(mi => mi.Name).ToList();
        }

        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            return new List<MediaItemAction>();
        }

        public void ExecuteMediaItemAction(MediaItem mediaItem, MediaItemAction playlistAction)
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
