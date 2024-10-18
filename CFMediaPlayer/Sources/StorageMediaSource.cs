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
    internal class StorageMediaSource : IMediaSource
    {
        private readonly MediaLocation _mediaLocation;

        public StorageMediaSource(MediaLocation mediaLocation)
        {
            _mediaLocation = mediaLocation;
        }

        public MediaLocation MediaLocation => _mediaLocation;
 
        public bool IsAvailable
        {
            get
            {
                return !String.IsNullOrEmpty(_mediaLocation.Source) &&
                    Directory.Exists(_mediaLocation.Source);                
            }
        }

        //private static Artist GetArtistFromFolder(string folder)
        //{
        //    return new Artist() { Path = folder, Name = new DirectoryInfo(folder).Name };
        //}

        public List<Artist> GetArtists(bool includeNonReal)
        {
            var artists = new List<Artist>();

            if (IsAvailable)
            {
                var artistFolders = Directory.GetDirectories(_mediaLocation.Source);
                foreach (var artistFolder in artistFolders)
                {
                    // Check that folder contains albums
                    var isHasMediaItemCollections = false;
                    foreach (var subFolder in Directory.GetDirectories(artistFolder))
                    {
                        if (MediaUtilities.IsFolderHasAudioFiles(subFolder))
                        {
                            isHasMediaItemCollections = true;
                            break;
                        }
                    }

                    if (isHasMediaItemCollections)
                    {
                        artists.Add(new Artist() { Path = artistFolder, Name = new DirectoryInfo(artistFolder).Name });
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
                var artistFolder = Path.Combine(_mediaLocation.Source, artist.Name);                
                var folders = Directory.GetDirectories(artistFolder);
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

        private List<MediaItem> GetMediaItemsFromFolder(string path)
        {
            var mediaItems = new List<MediaItem>();
            var files = Directory.GetFiles(path);
            foreach (var file in files)
            {
                if (Array.IndexOf(MediaUtilities.AudioFileExtensions, Path.GetExtension(file).ToLower()) != -1)
                {
                    mediaItems.Add(new MediaItem()
                    {
                        FilePath = file,
                        Name = Path.GetFileNameWithoutExtension(file)
                    });
                }
            }
            return mediaItems;
        }
      
        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, bool includeNonReal)
        {
            var mediaItems = new List<MediaItem>();

            if (IsAvailable)
            {                
                if (artist.EntityCategory == EntityCategory.All)
                {
                    // All media items for all artists
                    // Media item collection will be [Multiple]
                    foreach (var artistFolder in Directory.GetDirectories(_mediaLocation.Source))
                    {
                        foreach (var albumFolder in Directory.GetDirectories(artistFolder))
                        {
                            mediaItems.AddRange(GetMediaItemsFromFolder(albumFolder));
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
      

        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem, 
                                                            List<IMediaSource> allMediaSources)
        {
            var mediaItemActions = new List<MediaItemAction>();

            foreach (MediaSourceTypes mediaSourceType in new[] { MediaSourceTypes.Playlist, MediaSourceTypes.Queue })
            {
                var mediaSource = allMediaSources.FirstOrDefault(ms => ms.MediaLocation.MediaSourceType == mediaSourceType);
                if (mediaSource != null)
                {
                    mediaItemActions.AddRange(mediaSource.GetActionsForMediaItem(currentMediaLocation, mediaItem, allMediaSources));
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
                            ImageSource = mic.ImagePath
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
                                ImageSource = mediaItemCollection.ImagePath
                            }));
                    }
                }
            }
            
            return searchResults;
        }

        public Tuple<Artist, MediaItemCollection>? GetAncestorsForMediaItem(MediaItem mediaItem)
        {
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
                var localFolder = Path.Combine(_mediaLocation.Source, artist.Name, mediaItemCollection.Name);
                if (Directory.Exists(localFolder))
                {
                    return new Tuple<Artist, MediaItemCollection>(artist, mediaItemCollection);
                }
            }
            return null;
        }
    }
}
