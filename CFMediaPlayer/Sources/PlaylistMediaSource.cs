using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from playlists    
    /// </summary>
    public class PlaylistMediaSource : MediaSourceBase, IMediaSource
    {        
        private List<IPlaylistManager> _playlistManagers;           

        public PlaylistMediaSource(MediaLocation mediaLocation, IEnumerable<IPlaylistManager> playlistManagers) : base(mediaLocation)
        {            
            _playlistManagers = playlistManagers.ToList();
        }

        public string ImagePath => _mediaLocation != null && _mediaLocation.MediaSourceType == MediaSourceTypes.RadioStreams ?
                            "radio.png" :
                            InternalUtilities.DefaultImagePath;

        public bool IsAvailable
        {
            get
            {
                return _mediaLocation.Sources.Any(folder => Directory.Exists(folder));
            }
        }

        public bool IsDisplayInUI => IsAvailable;

        public List<Artist> GetArtists(bool includeNonReal)
        {
            if (includeNonReal)
            {
                return new List<Artist>()
                {
                    Artist.InstanceMultiple
                };
            }
            return new List<Artist>();
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            // Check each file
            if (IsAvailable)
            {
                foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                {
                    foreach (var playlistFile in Directory.GetFiles(mediaLocationSource))
                    {
                        // Get playlist handler
                        var playlistManager = _playlistManagers.FirstOrDefault(pl => pl.SupportsFile(playlistFile));
                        if (playlistManager != null)   // Playlist
                        {
                            playlistManager.FilePath = playlistFile;
                            var mediaItems = playlistManager.GetAll();
                            var itemCollection = new MediaItemCollection()
                            {
                                Path = playlistFile,                                
                                Name = playlistManager.Name                                                               
                            };
                            mediaItemCollections.Add(itemCollection);
                            playlistManager.FilePath = "";  // Clean up
                        }
                    }                    
                }
            }

            if (includeNonReal)
            {
                // Add all media item collection option
                if (mediaItemCollections.Count > 1)
                {
                    mediaItemCollections.Insert(0, MediaItemCollection.InstanceAll);
                }

                // Add None if no media item collections
                if (!mediaItemCollections.Any())
                {
                    mediaItemCollections.Add(MediaItemCollection.InstanceNone);                    
                }
            }

            //// Add New Playlist option
            //// NOTE: If there are no playlists then the app will display the New Playlist page as soon as the current
            //// page is population because it auto-selects the first media item collection.
            //var newPlaylistItemCollection = new MediaItemCollection()
            //{
            //    Name = LocalizationResources.Instance["NewPlaylistText"].ToString()
            //};
            //mediaItemCollections.Add(newPlaylistItemCollection);

            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, 
                                                bool includeNonReal)
        {
            var mediaItems = new List<MediaItem>();          
            
            if (IsAvailable)                
            {                
                if (mediaItemCollection.EntityCategory == EntityCategory.All)
                {
                    // Get media items for all playlists
                    // Check each playlist file           
                    foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                    {
                        foreach (var playlistFile in Directory.GetFiles(mediaLocationSource))
                        {
                            // Get playlist handler
                            var playlistManager = _playlistManagers.FirstOrDefault(pl => pl.SupportsFile(playlistFile));
                            if (playlistManager != null)   // Playlist
                            {
                                playlistManager.FilePath = playlistFile;
                                var playlistMediaItems = playlistManager.GetAll();
                                foreach (var mediaItem in playlistMediaItems)
                                {
                                    if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))
                                    {
                                        // Set image path to be album image if not set
                                        if (String.IsNullOrEmpty(mediaItem.ImagePath))
                                        {
                                            mediaItem.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                                        }
                                        mediaItems.Add(mediaItem);
                                    }
                                }
                                playlistManager.FilePath = "";  // Clean up
                            }
                        }                            
                    }
                }
                else if (mediaItemCollection.EntityCategory == EntityCategory.Real)
                {
                    // Get media items for specific playlist
                    // Check each playlist file            
                    foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                    {
                        foreach (var playlistFile in Directory.GetFiles(mediaLocationSource))
                        {
                            // Get playlist handler
                            var playlistManager = _playlistManagers.FirstOrDefault(pl => pl.SupportsFile(playlistFile));
                            if (playlistManager != null)   // Playlist
                            {
                                playlistManager.FilePath = playlistFile;
                                var playlistMediaItems = playlistManager.GetAll();  // Necessary to get playlist name
                                var itemCollection = new MediaItemCollection()
                                {
                                    Path = playlistFile,
                                    Name = playlistManager.Name                                    
                                };

                                if (mediaItemCollection.Name == itemCollection.Name)   // IPlaylist found
                                {                                    
                                    // Set image path to be album image if not set
                                    foreach (var mediaItem in playlistMediaItems.Where(mi => String.IsNullOrEmpty(mi.ImagePath)))
                                    {
                                        mediaItem.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                                    }
                                    mediaItems.AddRange(playlistMediaItems);
                                    playlistManager.FilePath= "";   // Clean up
                                    break;
                                }
                                playlistManager.FilePath = "";  // Clean up
                            }
                        }
                    }
                }
            }

            // Add None if no media items
            if (includeNonReal)
            {
                if (!mediaItems.Any())
                {
                    mediaItems.Add(MediaItem.InstanceNone);
                }
            }

            return mediaItems;
        }
   
        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var items = new List<MediaItemAction>();

            // Check each file
            if (IsAvailable &&
                _mediaLocation.MediaSourceType == MediaSourceTypes.Playlist)    // Only for playlists, not radio streams
            {
                // If playlists currently selected then add action to open album
                if (currentMediaLocation.MediaSourceType == MediaSourceTypes.Playlist)                    
                {
                    foreach(IMediaSource mediaSource in _allMediaSources.Where(ms => ms.MediaLocation.MediaSourceType == MediaSourceTypes.Storage && ms.IsAvailable))
                    {
                        var ancestors = mediaSource.GetAncestorsForMediaItem(mediaItem).FirstOrDefault();
                        if (ancestors != null)
                        {
                            var item = new MediaItemAction()
                            {
                                ActionToExecute = MediaItemActions.OpenMediaItemCollection,
                                MediaLocationName = mediaSource.MediaLocation.Name,                                
                                MediaItemFile = mediaItem.FilePath,            
                                ImagePath = ancestors.Item2.ImagePath,  // Album image "picture.png"
                                Name = String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.OpenMediaItemCollection)].ToString(),
                                        ancestors.Item2.Name)
                            };
                            items.Add(item);
                            break;
                        }
                    }      
                }

                // Add actions to add/remove from playlist
                foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                {
                    foreach (var playlistFile in Directory.GetFiles(mediaLocationSource))
                    {
                        // Get playlist handler
                        var playlistManager = _playlistManagers.FirstOrDefault(pl => pl.SupportsFile(playlistFile));
                        if (playlistManager != null)   // Playlist
                        {
                            // Check if media item in playlist
                            playlistManager.FilePath = playlistFile;
                            var mediaItems = playlistManager.GetAll();
                            var isFoundMediaItem = mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath);                            

                            // Create media item action
                            var item = new MediaItemAction()
                            {
                                MediaLocationName = _mediaLocation.Name,
                                Name = isFoundMediaItem ?
                                         String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.RemoveFromPlaylist)].ToString(),
                                                playlistManager.Name) :
                                         String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.AddToPlaylist)].ToString(),
                                                playlistManager.Name),
                                MediaItemFile = mediaItem.FilePath,
                                PlaylistFile = playlistFile,
                                ImagePath = isFoundMediaItem ?
                                        "cross.png" :
                                        "plus.png",
                                ActionToExecute = isFoundMediaItem ?
                                        MediaItemActions.RemoveFromPlaylist :
                                        MediaItemActions.AddToPlaylist
                            };
                            playlistManager.FilePath = "";  // Clean up

                            // If user currently has playlist selected then we only allow them to remove the media item from any playlist that 
                            // the media item is added to
                            if (currentMediaLocation.MediaSourceType == MediaSourceTypes.Playlist)
                            {
                                if (item.ActionToExecute == MediaItemActions.RemoveFromPlaylist)
                                {
                                    items.Add(item);
                                }
                            }
                            else
                            {
                                items.Add(item);
                            }

                            
                        }
                    }
                }
            }

            return items;
        }

        public void ExecuteMediaItemAction(MediaItem mediaItem, MediaItemAction mediaItemAction)
        {
            if (IsAvailable)
            {
                var playlistManager = _playlistManagers.FirstOrDefault(pl => pl.SupportsFile(mediaItemAction.PlaylistFile));
                if (playlistManager != null)
                {
                    playlistManager.FilePath = mediaItemAction.PlaylistFile;
                    var mediaItems = playlistManager.GetAll();

                    // Add or remove playlist item
                    var isSavePlaylist = true;
                    switch (mediaItemAction.ActionToExecute)
                    {
                        case MediaItemActions.AddToPlaylist:
                            if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))  // Not in playlist already
                            {
                                mediaItems.Add(mediaItem);
                            }                            
                            break;
                        case MediaItemActions.ClearPlaylist:
                            mediaItems.Clear();
                            break;
                        case MediaItemActions.DeletePlaylist:                            
                            File.Delete(mediaItemAction.PlaylistFile);
                            isSavePlaylist = false;
                            break;
                        case MediaItemActions.RemoveFromPlaylist:
                            mediaItems.RemoveAll(mi => mi.FilePath == mediaItem.FilePath);                            
                            break;
                    }                   
                    if (isSavePlaylist)
                    {
                        playlistManager.SaveAll(mediaItems);
                    }
                    playlistManager.FilePath = "";  // Clean up
                }
            }
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();

            if (IsAvailable)
            {
                var mediaItemCollections = GetMediaItemCollectionsForArtist(null, false);    // Playlists

                foreach (var mediaItemCollection in mediaItemCollections)
                {
                    if (SearchUtilities.IsValidSearchResult(mediaItemCollection, searchOptions))
                    {
                        searchResults.Add(new SearchResult()
                        {
                            EntityType = EntityTypes.MediaItemCollection,
                            Name = mediaItemCollection.Name,
                            Artist = Artist.InstanceNone,
                            MediaItemCollection = mediaItemCollection,
                            MediaLocationName = MediaLocation.Name,
                            ImagePath = _mediaLocation.MediaSourceType == MediaSourceTypes.RadioStreams ? ImagePath : mediaItemCollection.ImagePath
                        });
                    }

                    var mediaItems = GetMediaItemsForMediaItemCollection(null, mediaItemCollection, false);

                    searchResults.AddRange(mediaItems.Where(mi => SearchUtilities.IsValidSearchResult(mi, searchOptions))
                           .Select(mi => new SearchResult()
                           {
                               EntityType = EntityTypes.MediaItem,
                               Name = mi.Name,
                               Artist = Artist.InstanceNone,
                               MediaItemCollection = mediaItemCollection,
                               MediaItem = mi,
                               MediaLocationName = MediaLocation.Name,
                               ImagePath = !String.IsNullOrEmpty(mi.ImagePath) ? mi.ImagePath :   // Radio stream image
                                                    GetMediaItemCollectionImagePath(mi)                                                                   
                           }));
                }
            }

            searchResults = searchResults.OrderBy(s => s.Name).ToList();

            return searchResults;
        }

        public List<Tuple<Artist, MediaItemCollection>> GetAncestorsForMediaItem(MediaItem mediaItem)
        {            
            var ancestors = new List<Tuple<Artist, MediaItemCollection>>();

            if (IsAvailable)
            {
                foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                { 
                    foreach (var playlistFile in Directory.GetFiles(mediaLocationSource))
                    {
                        // Get playlist handler
                        var playlistManager = _playlistManagers.FirstOrDefault(pl => pl.SupportsFile(playlistFile));
                        if (playlistManager != null)   // Playlist
                        {
                            playlistManager.FilePath = playlistFile;
                            var mediaItems = playlistManager.GetAll();

                            var mediaItemCollection = new MediaItemCollection()
                            {
                                Path = playlistFile,
                                Name = playlistManager.Name                                
                            };                            
                            var mediaItemFound = mediaItems.FirstOrDefault(mi => mi.FilePath == mediaItem.FilePath);
                            if (mediaItemFound != null)
                            {
                                ancestors.Add(new Tuple<Artist, MediaItemCollection>(Artist.InstanceMultiple, mediaItemCollection));
                            }
                            playlistManager.FilePath = "";  // Clean up
                        }
                    }
                }
            }

            return ancestors;
        }
    }
}
