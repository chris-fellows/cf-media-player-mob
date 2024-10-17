using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

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
                // Add shuffle artist option           
                if (artists.Count > 1)
                {
                    artists.Insert(0, new Artist()
                    {
                        Name = LocalizationResources.Instance["ShuffleText"].ToString(),
                    });
                }

                // Add None if no artists
                if (!artists.Any())
                {
                    artists.Add(new Artist()
                    {
                        Name = LocalizationResources.Instance["NoneText"].ToString(),
                    });
                }
            }

            return artists.OrderBy(a => a.Name).ToList();
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            // If shuffle artist then we just display [Multiple]
            if (includeNonReal && MediaUtilities.IsShuffleArtist(artist))
            {
                mediaItemCollections.Insert(0, new MediaItemCollection()
                {
                    Name = LocalizationResources.Instance["MultipleText"].ToString(),
                });

                return mediaItemCollections;
            }          
            
            if (IsAvailable &&
                MediaUtilities.IsRealArtist(artist))
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
                // Add shuffle media item collection option
                if (!MediaUtilities.IsShuffleArtist(artist) &&
                    mediaItemCollections.Count > 1)
                {
                    mediaItemCollections.Insert(0, new MediaItemCollection()
                    {
                        Name = LocalizationResources.Instance["ShuffleText"].ToString(),
                    });
                }

                // Add None if no media item collections
                if (!mediaItemCollections.Any())
                {
                    mediaItemCollections.Add(new MediaItemCollection()
                    {
                        Name = LocalizationResources.Instance["NoneText"].ToString(),
                    });
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
                if (MediaUtilities.IsShuffleArtist(artist))
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

                    mediaItems.SortRandom();
                }
                else if (MediaUtilities.IsShuffleMediaItemCollection(mediaItemCollection))
                {
                    // All media items for artist
                    // Artist will be real artist
                    foreach (var albumFolder in Directory.GetDirectories(artist.Path))
                    {
                        mediaItems.AddRange(GetMediaItemsFromFolder(albumFolder));
                    }

                    mediaItems.SortRandom();
                }
                else if (MediaUtilities.IsRealMediaItemCollection(mediaItemCollection))
                {
                    mediaItems.AddRange(GetMediaItemsFromFolder(mediaItemCollection.Path));
                }

                //// Play media items for this media item collection
                //var path = Path.Combine(_mediaLocation.Source, artist.Name, mediaItemCollection.Name);
                //if (Directory.Exists(path))
                //{
                //    mediaItems.AddRange(GetMediaItemsFromFolder(path));       
                //}
            }

            if (includeNonReal)
            {
                // Add None if no media items
                if (!mediaItems.Any())
                {
                    mediaItems.Add(new MediaItem()
                    {
                        Name = LocalizationResources.Instance["NoneText"].ToString(),
                    });
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

            var artists = GetArtists(false);

            foreach(var artist in artists.Where(a => MediaUtilities.IsRealArtist(a)))
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
                var mediaItemCollections = GetMediaItemCollectionsForArtist(artist, false).Where(mic => MediaUtilities.IsRealMediaItemCollection(mic));

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
                    var mediaItems = GetMediaItemsForMediaItemCollection(artist, mediaItemCollection, false).Where(mi => MediaUtilities.IsRealMediaItem(mi));

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
