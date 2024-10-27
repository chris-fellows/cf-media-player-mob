using Bumptech.Glide.Manager;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using Kotlin.Reflect;
using static Android.Provider.MediaStore.Audio;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from storage (Internal/SD card)
    /// 
    /// Notes:
    /// - Media items can be in any of these folder structures:
    ///      1) \[Source]\[Artist]\[Album]   : Default for music.
    ///      2) \[Source]\[Artist]           : E.g. Podcasts.
    /// </summary>
    internal class StorageMediaSource : MediaSourceBase, IMediaSource
    {
        private const int _maxParallelThreads = 4;        

        public StorageMediaSource(ICurrentState currentState,
                                MediaLocation mediaLocation) : base(currentState, mediaLocation)
        {
         
        }

        public string ImagePath => GetImagePathByMediaItemTypes();        

        public bool IsAvailable
        {
            get
            {
                return _mediaLocation.Sources.Any(folder => Directory.Exists(folder));
            }
        }

        /// <summary>
        /// Whether to display in UI. Always display music folder and other folders only if media items
        /// </summary>
        public bool IsDisplayInUI => IsAvailable && 
                                    (_mediaLocation.MediaItemTypes.Contains(MediaItemTypes.Music) || GetArtists(false).Any());

        public bool IsShufflePlayAllowed => true;

        public bool IsAutoPlayNextAllowed => true;

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
                var names = new[] { Android.OS.Environment.DirectoryAudiobooks.ToLower(),
                                "playlists",
                                Android.OS.Environment.DirectoryPodcasts.ToLower(),
                                "radiostreams" };
                var folderName = new DirectoryInfo(artistFolder).Name.ToLower();
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
                    if (artistFolders.Any())
                    {                        
                        MediaItemCollectionDetails? rootMediaItemCollectionDetails = null;

                        Parallel.ForEach(artistFolders,
                                            new ParallelOptions { MaxDegreeOfParallelism = _maxParallelThreads },
                                            artistFolder =>
                                            {                                                
                                                if (IsCheckArtistFolder(artistFolder))
                                                {                                                    
                                                    // Check if media items in \[Source]\[Artist] folder. E.g. Podcasts.
                                                    // E.g. \[Source]\[Russell Brand Podcasts]\Episode 1.mp3
                                                    // We only need to find one folder because they will all be for the same artist.
                                                    if (rootMediaItemCollectionDetails == null)                                                        
                                                    {
                                                        rootMediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(artistFolder, false);                                                        
                                                    }

                                                    // Check if media items in [Source]\[Artist]\[Album] folder                                                        
                                                    var isHasMediaItemCollections = false;
                                                    MediaItemCollectionDetails? mediaItemCollectionDetails = null;
                                                    if (!isHasMediaItemCollections)
                                                    {
                                                        foreach (var mediaItemCollectionFolder in Directory.GetDirectories(artistFolder))
                                                        {
                                                            mediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(mediaItemCollectionFolder, false);
                                                            if (mediaItemCollectionDetails != null)
                                                            {                                                                
                                                                break;
                                                            }                                              
                                                        }
                                                    }
                                                    
                                                    if (mediaItemCollectionDetails != null)
                                                    {                                                        
                                                        artists.Add(new Artist()
                                                        {
                                                            Path = mediaItemCollectionDetails.Artist.Path,
                                                            Name = mediaItemCollectionDetails.Artist.Name
                                                        });
                                                    }
                                                }                                       
                                            });

                        // If media items in \[Source]\[Artist] folder (E.g. \Podcasts\Artist) then add source name artist (E.g. "Podcasts")
                        // and media item collection name will be \[Source]\[Artist]. When GetMediaItemCollections is called then it will
                        // return the correct an instance for each \[Source]\[Artist] folder.
                        // TODO: Should we change the artist name? It will be the source folder name (E.g. "Podcasts")
                        if (rootMediaItemCollectionDetails != null)
                        {
                            artists.Add(new Artist()
                            {
                                Path = rootMediaItemCollectionDetails.Artist.Path,
                                Name = rootMediaItemCollectionDetails.Artist.Name
                            });
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
                // MediaItem: D:\Podcasts\Russell Brand Podcasts\Episode 01.mp3
                // Artist.Path=D:\Podcasts
                // MediaItemCollection.Path=D:\Podcasts\Russell Brand Podcasts
                if (Directory.Exists(artist.Path))
                {
                    // Get media item collections. Note that Artist.Path may refer to \[Source]\[Artist]\[Album] (E.g. Music) or
                    // \[Source]\[Artist] (E.g. Podcasts)
                    var mediaItemCollectionFolders = Directory.GetDirectories(artist.Path);
                    foreach (var mediaItemCollectionFolder in mediaItemCollectionFolders)
                    {
                        var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(mediaItemCollectionFolder, false);
                        if (mediaItemCollectionDetails != null)   
                        {
                            mediaItemCollections.Add(new MediaItemCollection()
                            {
                                Path = mediaItemCollectionDetails.MediaItemCollection.Path,
                                Name = mediaItemCollectionDetails.MediaItemCollection.Name,
                                ImagePath = mediaItemCollectionDetails.MediaItemCollection.ImagePath
                            });
                        }                    
                    }
                }

                /*
                // This doesn't handle media items in \[Source]\[Artist]
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
                */
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
      
        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, 
                                                            bool includeNonReal)
        {
            var mediaItems = new List<MediaItem>();

            if (IsAvailable)
            {                           
                if (artist.EntityCategory == EntityCategory.All)    // All media items for all artists
                {                    
                    // Media item collection will be [Multiple]
                    foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                    {
                        var artistFolders = Directory.GetDirectories(mediaLocationSource);

                        Parallel.ForEach(artistFolders,
                                          new ParallelOptions { MaxDegreeOfParallelism = _maxParallelThreads },
                                          artistFolder =>
                                          {
                                              if (IsCheckArtistFolder(artistFolder))
                                              {                                                  
                                                  // Check for media items in \[Artist]. E.g. Podcasts           
                                                  var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(artistFolder, true);
                                                  if (mediaItemCollectionDetails != null)
                                                  {
                                                      mediaItems.AddRange(mediaItemCollectionDetails.MediaItems);
                                                  }

                                                  // Check for media items in \[Artist]\[Album]
                                                  foreach (var albumFolder in Directory.GetDirectories(artistFolder))
                                                  {
                                                      mediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(albumFolder, true);
                                                      if (mediaItemCollectionDetails != null)
                                                      {
                                                          mediaItems.AddRange(mediaItemCollectionDetails.MediaItems);
                                                      }                                                      
                                                  }
                                              }
                                          });                   
                    }
                }
                else if (mediaItemCollection.EntityCategory == EntityCategory.All)  // All media items for artist
                {                    
                    // Artist will be real artist
                    // Check for media items in \[Artist]. E.g. Podcasts                    
                    var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(artist.Path, true);
                    if (mediaItemCollectionDetails != null)
                    {
                        mediaItems.AddRange(mediaItemCollectionDetails.MediaItems);
                    }

                    // Check for media items in \[Artist]\[Album]
                    foreach (var albumFolder in Directory.GetDirectories(artist.Path))
                    {
                        mediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(albumFolder, true);
                        if (mediaItemCollectionDetails != null)
                        {
                            mediaItems.AddRange(mediaItemCollectionDetails.MediaItems);
                        }                        
                    }                    
                }
                else if (mediaItemCollection.EntityCategory == EntityCategory.Real)    // All media items for media item collection
                {                   
                    var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(mediaItemCollection.Path, true);
                    if (mediaItemCollectionDetails != null)
                    {
                        mediaItems.AddRange(mediaItemCollectionDetails.MediaItems);
                    }
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
      

        public List<MediaAction> GetMediaActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var mediaActions = new List<MediaAction>();

            // Get actions from playlists & queue
            foreach (MediaSourceTypes mediaSourceType in new[] { MediaSourceTypes.Playlist, MediaSourceTypes.Queue })
            {
                var mediaSource = _allMediaSources.FirstOrDefault(ms => ms.MediaLocation.MediaSourceType == mediaSourceType);
                if (mediaSource != null)
                {
                    mediaActions.AddRange(mediaSource.GetMediaActionsForMediaItem(currentMediaLocation, mediaItem));
                }
            }
       
            return mediaActions;
        }

        public List<MediaAction> GetMediaActionsForMediaLocation(MediaLocation mediaLocation)
        {
            var mediaActions = new List<MediaAction>();
            return mediaActions;
        }

        public void ExecuteMediaAction(MediaAction mediaAction)
        {
            switch (mediaAction.ActionType)
            {
                case MediaActionTypes.OpenMediaItemCollection:
                    // Get MediaItem
                    var mediaItem = GetMediaItemByFile(mediaAction.MediaItemFile);

                    // Get Artist & MediaItemCollection
                    var ancestors = GetAncestorsForMediaItem(mediaItem).FirstOrDefault();

                    // Select MediaItemCollection
                    // TODO: Consider selecting media item
                    _currentState.SelectMediaItemCollectionAction!(_mediaLocation, ancestors.Item1, ancestors.Item2);

                    break;
            }           
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();

            if (IsAvailable)
            {
                var artists = GetArtists(false);

                if (artists.Any())
                {
                    var defaultMediaItemImagePath = GetImagePathByMediaItemTypes();

                    Parallel.ForEach(artists,
                                new ParallelOptions { MaxDegreeOfParallelism = _maxParallelThreads },
                                artist =>
                                {
                                    if (SearchUtilities.IsValidSearchResult(artist, searchOptions))
                                    {
                                        searchResults.Add(new SearchResult()
                                        {
                                            EntityType = EntityTypes.Artist,
                                            Name = artist.Name,
                                            Artist = artist,
                                            MediaLocationName = MediaLocation.Name,
                                            ImagePath = defaultMediaItemImagePath
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
                                });
                }
            }

            searchResults = searchResults.OrderBy(s => s.Name).ToList();
            
            return searchResults;
        }

        public List<Tuple<Artist, MediaItemCollection>> GetAncestorsForMediaItem(MediaItem mediaItem)
        {
            var ancestors = new List<Tuple<Artist, MediaItemCollection>>();

            // Example of podcast media item:
            // MediaItem.Path=\[Podcasts]\[Album]\01 Episode 1.mp3
            // Need to be careful if final check due to folder struck

            if (IsAvailable && mediaItem.EntityCategory == EntityCategory.Real)
            {
                // Get MediaItemCollectionDetails
                var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForFolder(Path.GetDirectoryName(mediaItem.FilePath), false);

                // Check that media item collection exists in this media source. We need to be careful that if we have
                // internal and SD card storage instances then only the instance that contains the media item returns
                // ancestors.
                if (mediaItemCollectionDetails != null)
                {
                    // Example 1:
                    // Source=D:\Data\Music
                    // Artist=D:\Data\Music\Slash
                    // MediaItemCollection=D:\Data\Music\Slash\Greatest Hits

                    // Example 2:
                    // Source=D:\Data\Podcasts
                    // Artist=D:\Data\Podcasts
                    // MediaItemCollection=D:\Data\Podcast\Russell Brand Podcasts
                    foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                    {
                        if (mediaItemCollectionDetails.Artist.Path.StartsWith(mediaLocationSource, StringComparison.InvariantCultureIgnoreCase))
                        {
                            ancestors.Add(new Tuple<Artist, MediaItemCollection>(mediaItemCollectionDetails.Artist, mediaItemCollectionDetails.MediaItemCollection));
                            break;
                        }                        
                    }
                }

                //// Get media item collection
                //var mediaItemCollectionFolder = new FileInfo(mediaItem.FilePath).DirectoryName;
                //var mediaItemCollection = new MediaItemCollection()
                //{
                //    Path = mediaItemCollectionFolder,
                //    Name = new DirectoryInfo(mediaItemCollectionFolder).Name
                //};

                //// Get artist
                //var artistFolder = new DirectoryInfo(mediaItemCollectionFolder).Parent.FullName;
                //var artist = new Artist()
                //{
                //    Path = artistFolder,
                //    Name = new DirectoryInfo(artistFolder).Name
                //};

                //// Check that media item collection exists in this media source. We need to be careful that if we have
                //// internal and SD card storage instances then only the instance that contains the media item returns
                //// ancestors.                
                //foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                //{
                //    //var localFolder = Path.Combine(mediaLocationSource, artist.Name, mediaItemCollection.Name);
                //    if (Directory.Exists(Path.Combine(mediaLocationSource, artist.Name, mediaItemCollection.Name)))
                //    {
                //        ancestors.Add(new Tuple<Artist, MediaItemCollection>(artist, mediaItemCollection));
                //        break;
                //    }
                //    else if (Directory.Exists(Path.Combine(mediaLocationSource, mediaItemCollection.Name))) // E.g. Podcasts in \[Podcasts]\[Album]
                //    {
                //        // TODO: Do we need to be careful of an artist have the same name as an album?
                //        ancestors.Add(new Tuple<Artist, MediaItemCollection>(artist, mediaItemCollection));
                //        break;
                //    }
                //}
            }
            return ancestors;
        }


        /// <summary>
        /// Gets MediaItemCollectionDetails for folder if it contains audio files. Does not check sub-folders. For performance
        /// then it's possible to not return all media files.
        /// </summary>
        /// <param name="folder">Folder to check</param>
        /// <param name="getMediaItems">Whether to return media item list</param>
        /// <returns>MediaItemCollectionDetails is folder contains audio files else null</returns>
        private MediaItemCollectionDetails? GetMediaItemCollectionDetailsForFolder(string folder, bool getMediaItems)
        {
            if (Directory.Exists(folder))
            {
                var isContainsMediaItems = false;
                string firstMediaItemFile = "";
                var mediaItems = new List<MediaItem>();

                string mediaItemImagePath = GetImagePathByMediaItemTypes();                

                // Check folder for audio files
                var files = Directory.GetFiles(folder);
                foreach (var file in files)
                {
                    if (Array.IndexOf(MediaUtilities.AudioFileExtensions, Path.GetExtension(file).ToLower()) != -1)
                    {                        
                        isContainsMediaItems = true;
                        if (String.IsNullOrEmpty(firstMediaItemFile)) firstMediaItemFile = file;

                        if (getMediaItems)
                        {
                            mediaItems.Add(new MediaItem()
                            {
                                FilePath = file,
                                Name = MediaUtilities.GetMediaItemNameForMediaItemPath(file),
                                ImagePath = mediaItemImagePath
                            });
                        }
                        else    // Just needed to get one audio file so that we can get the artist & album names
                        {
                            break;
                        }
                    }
                }

                if (isContainsMediaItems)
                {
                    // E.g. \Podcasts\Russell Brand Podcasts\Episode 01.mp3
                    MediaItemCollectionDetails mediaItemCollectionDetails = new MediaItemCollectionDetails()
                    {
                        Artist = new Artist()
                        {
                            Name = MediaUtilities.GetArtistNameForMediaItemPath(firstMediaItemFile),                // Podcasts
                            Path = new DirectoryInfo(Path.GetDirectoryName(firstMediaItemFile)).Parent.FullName
                        },
                        MediaItemCollection = new MediaItemCollection()
                        {
                            Name = MediaUtilities.GetMediaItemCollectionNameForMediaItemPath(firstMediaItemFile),   // Russell Brand Podcasts                    
                            Path = Path.GetDirectoryName(firstMediaItemFile),
                            ImagePath = MediaUtilities.GetMediaItemCollectionImagePath(folder)
                        },
                        MediaItems = mediaItems
                    };
                    
                    // If no media item collection image path then set default
                    if (String.IsNullOrEmpty(mediaItemCollectionDetails.MediaItemCollection.ImagePath))
                    {
                        mediaItemCollectionDetails.MediaItemCollection.ImagePath = GetImagePathByMediaItemTypes();
                    }

                    return mediaItemCollectionDetails;
                }
            }

            return null;
        }

        public MediaItem? GetMediaItemByFile(string file)
        {            
                // Check that file is in this media location
                foreach (var source in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                {
                    if (file.StartsWith(source) && File.Exists(file))
                    {
                        MediaItem mediaItem = new MediaItem()
                        {
                            FilePath = file,
                            Name = MediaUtilities.GetMediaItemNameForMediaItemPath(file),
                            ImagePath = GetImagePathByMediaItemTypes()
                        };
                        return mediaItem;
                    }
                }        

            return null;
        }        
    }
}
