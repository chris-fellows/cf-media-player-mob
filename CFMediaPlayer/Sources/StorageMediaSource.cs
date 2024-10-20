using Bumptech.Glide.Manager;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using static Java.Util.Jar.Attributes;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from storage (Internal/SD card)
    /// </summary>
    internal class StorageMediaSource : MediaSourceBase, IMediaSource
    {        
        public StorageMediaSource(MediaLocation mediaLocation) : base(mediaLocation)
        {
     
        }

        public string ImagePath => InternalUtilities.DefaultImagePath;        

        public bool IsAvailable
        {
            get
            {
                return _mediaLocation.Sources.Any(folder => Directory.Exists(folder));
            }
        }
        public bool HasMediaItems => GetArtists(false).Any();

        /// <summary>
        /// Whether artist folder should be checked for media items. If this media source is for music then we ignore
        /// folders that contain other media item types because they're handled in another IMediaSource. It's necessary
        /// because these folders may be sub-folders of the Music folder. E.g. ..\Music\Podcasts, ..\Music\Playlists
        /// </summary>
        /// <param name="artistFolder"></param>
        /// <returns></returns>
        private bool IsCheckArtistFolder(string artistFolder)
        {
            if (_mediaLocation.MediaItemTypes.Contains(MediaItemTypes.Music))
            {
                var names = new[] { Android.OS.Environment.DirectoryAudiobooks,
                                "Playlists",
                                Android.OS.Environment.DirectoryPodcasts,
                                "Radiostreams" };
                var folderName = new DirectoryInfo(artistFolder).Name;
                return !names.Contains(folderName);
            }

            return true;
        }

        public List<Artist> GetArtists(bool includeNonReal)
        {
            var artists = new List<Artist>();

            if (IsAvailable)
            {
                foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                {
                    var artistFolders = Directory.GetDirectories(mediaLocationSource);
                    foreach (var artistFolder in artistFolders.Where(f => IsCheckArtistFolder(f)))
                    {
                        // Check that folder contains albums
                        var isHasMediaItemCollections = false;
                        foreach (var mediaItemCollectionFolder in Directory.GetDirectories(artistFolder))
                        {
                            if (!new DirectoryInfo(mediaItemCollectionFolder).Name.StartsWith(".")) // Ignore folders prefixed with . (E.g. .RadioStreams)
                            {
                                if (MediaUtilities.IsFolderHasAudioFiles(mediaItemCollectionFolder))
                                {
                                    isHasMediaItemCollections = true;
                                    break;
                                }
                            }
                        }

                        if (isHasMediaItemCollections)
                        {
                            artists.Add(new Artist() { Path = artistFolder, Name = new DirectoryInfo(artistFolder).Name });
                        }
                    }
                }
            }

            if (includeNonReal)
            {
                // Add all artist option           
                if (artists.Count > 1)
                {
                    artists.Insert(0, Artist.InstanceAll);
                }

                // Add None if no artists
                if (!artists.Any())
                {
                    artists.Add(Artist.InstanceNone);
                }
            }

            return artists.OrderBy(a => a.Name).ToList();
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            // If all artist then we just display [Multiple]
            if (includeNonReal && artist.EntityCategory == EntityCategory.All)
            {
                mediaItemCollections.Insert(0, MediaItemCollection.InstanceAll);

                return mediaItemCollections;
            }          
            
            if (IsAvailable &&
                artist.EntityCategory == EntityCategory.Real)
            {
                foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                {
                    var artistFolder = Path.Combine(mediaLocationSource, artist.Name);
                    if (Directory.Exists(artistFolder))
                    {
                        var mediaItemCollectionFolders = Directory.GetDirectories(artistFolder);
                        foreach (var mediaItemCollectionFolder in mediaItemCollectionFolders)
                        {
                            if (MediaUtilities.IsFolderHasAudioFiles(mediaItemCollectionFolder))
                            {
                                mediaItemCollections.Add(new MediaItemCollection()
                                {
                                    //Path = mediaLocationSource,     // TODO: Is this correct
                                    Path = mediaItemCollectionFolder,
                                    Name = new DirectoryInfo(mediaItemCollectionFolder).Name
                                });
                            }
                        }
                    }
                }
            }

            if (includeNonReal)
            {
                // Add all media item collection option
                if (artist.EntityCategory != EntityCategory.All &&
                    mediaItemCollections.Count > 1)
                {
                    mediaItemCollections.Insert(0, MediaItemCollection.InstanceAll);
                }

                // Add None if no media item collections
                if (!mediaItemCollections.Any())
                {
                    mediaItemCollections.Add(MediaItemCollection.InstanceNone);
                }
            }

            return mediaItemCollections.OrderBy(mic => mic.Name).ToList();
        }

        private static List<MediaItem> GetMediaItemsFromFolder(string path)
        {
            var mediaItems = new List<MediaItem>();
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (Array.IndexOf(MediaUtilities.AudioFileExtensions, Path.GetExtension(file).ToLower()) != -1)
                {
                    // Don't set image path so that system uses default (Album cover)
                    mediaItems.Add(new MediaItem()
                    {
                        FilePath = file,
                        Name = Path.GetFileNameWithoutExtension(file)
                    });
                }
            }
            return mediaItems;
        }
      
        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, 
                                                            bool includeNonReal)
        {
            var mediaItems = new List<MediaItem>();

            if (IsAvailable)
            {                           
                if (artist.EntityCategory == EntityCategory.All)
                {
                    // All media items for all artists
                    // Media item collection will be [Multiple]
                    foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                    {
                        foreach (var artistFolder in Directory.GetDirectories(mediaLocationSource).Where(f => IsCheckArtistFolder(f)))
                        {
                            foreach (var albumFolder in Directory.GetDirectories(artistFolder))
                            {
                                mediaItems.AddRange(GetMediaItemsFromFolder(albumFolder));
                            }
                        }
                    }
                }
                else if (mediaItemCollection.EntityCategory == EntityCategory.All)
                {
                    // All media items for artist
                    // Artist will be real artist
                    foreach (var albumFolder in Directory.GetDirectories(artist.Path))
                    {
                        mediaItems.AddRange(GetMediaItemsFromFolder(albumFolder));
                    }                    
                }
                else if (mediaItemCollection.EntityCategory == EntityCategory.Real)
                {
                    mediaItems.AddRange(GetMediaItemsFromFolder(mediaItemCollection.Path));
                }
            }

            if (includeNonReal)
            {
                // Add None if no media items
                if (!mediaItems.Any())
                {
                    mediaItems.Add(MediaItem.InstanceNone);
                }
            }

            return mediaItems;
        }
      

        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var mediaItemActions = new List<MediaItemAction>();

            foreach (MediaSourceTypes mediaSourceType in new[] { MediaSourceTypes.Playlist, MediaSourceTypes.Queue })
            {
                var mediaSource = _allMediaSources.FirstOrDefault(ms => ms.MediaLocation.MediaSourceType == mediaSourceType);
                if (mediaSource != null)
                {
                    mediaItemActions.AddRange(mediaSource.GetActionsForMediaItem(currentMediaLocation, mediaItem));
                }
            }
       
            return mediaItemActions;
        }     

        public void ExecuteMediaItemAction(MediaItem mediaItem, MediaItemAction playlistAction)
        {
            
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();

            if (IsAvailable)
            {
                var artists = GetArtists(false);

                foreach (var artist in artists.Where(a => a.EntityCategory == EntityCategory.Real))
                {
                    if (SearchUtilities.IsValidSearchResult(artist, searchOptions))
                    {
                        searchResults.Add(new SearchResult()
                        {
                            EntityType = EntityTypes.Artist,
                            Name = artist.Name,
                            Artist = artist,
                            MediaLocationName = MediaLocation.Name
                        });
                    }

                    // Get media item collections
                    var mediaItemCollections = GetMediaItemCollectionsForArtist(artist, false).Where(mic => mic.EntityCategory == EntityCategory.Real);

                    searchResults.AddRange(mediaItemCollections.Where(mic => SearchUtilities.IsValidSearchResult(mic, searchOptions))
                        .Select(mic => new SearchResult()
                        {
                            EntityType = EntityTypes.MediaItemCollection,
                            Name = mic.Name,
                            Artist = artist,
                            MediaItemCollection = mic,
                            MediaLocationName = MediaLocation.Name,
                            ImagePath = mic.ImagePath
                        }));

                    // Check each media item collection
                    foreach (var mediaItemCollection in mediaItemCollections)
                    {
                        // Get media items for collection
                        var mediaItems = GetMediaItemsForMediaItemCollection(artist, mediaItemCollection, false).Where(mi => mi.EntityCategory == EntityCategory.Real);

                        searchResults.AddRange(mediaItems.Where(mi => SearchUtilities.IsValidSearchResult(mi, searchOptions))
                            .Select(mi => new SearchResult()
                            {
                                EntityType = EntityTypes.MediaItem,
                                Name = mi.Name,
                                Artist = artist,
                                MediaItemCollection = mediaItemCollection,
                                MediaItem = mi,
                                MediaLocationName = MediaLocation.Name,
                                ImagePath = mediaItemCollection.ImagePath
                            }));
                    }
                }
            }
            
            return searchResults;
        }

        public List<Tuple<Artist, MediaItemCollection>> GetAncestorsForMediaItem(MediaItem mediaItem)
        {
            var ancestors = new List<Tuple<Artist, MediaItemCollection>>();

            if (IsAvailable && mediaItem.EntityCategory == EntityCategory.Real)
            {
                // Get media item collection
                var mediaItemCollectionFolder = new FileInfo(mediaItem.FilePath).DirectoryName;
                var mediaItemCollection = new MediaItemCollection()
                {
                    Path = mediaItemCollectionFolder,
                    Name = new DirectoryInfo(mediaItemCollectionFolder).Name
                };

                // Get artist
                var artistFolder = new DirectoryInfo(mediaItemCollectionFolder).Parent.FullName;
                var artist = new Artist()
                {
                    Path = artistFolder,
                    Name = new DirectoryInfo(artistFolder).Name
                };

                // Check that media item collection exists in this media source.
                foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                {
                    var localFolder = Path.Combine(mediaLocationSource, artist.Name, mediaItemCollection.Name);
                    if (Directory.Exists(localFolder))
                    {
                        ancestors.Add(new Tuple<Artist, MediaItemCollection>(artist, mediaItemCollection));
                        break;
                    }
                }
            }
            return ancestors;
        }
    }
}
