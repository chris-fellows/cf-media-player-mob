using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using static Android.Provider.MediaStore.Audio;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from playlists    
    /// 
    /// Notes:
    /// - Currently we always display only "All" in the artist list because playlists are expected to be in the route of 
    ///   the source location. However we do support having an artist folder but those should be rare.
    /// - Media items can be in any of these folder structures:    
    ///      1) \[Source]\[Artist]          : Default for playlists.
    ///      2) \[Source]\[Artist]\[Album]  : Less common but we support it.
    /// </summary>
    public class PlaylistMediaSource : MediaSourceBase, IMediaSource
    {        
        private List<IPlaylistManager> _playlistManagers;           

        public PlaylistMediaSource(ICurrentState currentState, 
                            MediaLocation mediaLocation, 
                            IEnumerable<IPlaylistManager> playlistManagers) : base(currentState, mediaLocation)
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

        public bool IsShufflePlayAllowed => _mediaLocation != null && _mediaLocation.MediaSourceType == MediaSourceTypes.RadioStreams ? false : true;

        public bool IsAutoPlayNextAllowed => _mediaLocation != null && _mediaLocation.MediaSourceType == MediaSourceTypes.RadioStreams ? false : true;

        public List<Artist> GetArtists(bool includeNonReal)
        {
            var artists = new List<Artist>();

            //foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
            //{
            //    // Check files in this folder
            //    // D:\Data\Podcasts\Rock.m3u (Artist.Name=Podcasts)
            //    // D:\Data\Podcasts\Blues.m3u (Artist.Name=Podcasts)
            //    foreach (var file in Directory.GetFiles(mediaLocationSource))
            //    {
            //        // Get playlist if any
            //        var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForPlaylist(file, false);
            //        if (mediaItemCollectionDetails != null &&
            //            !artists.Any(a => a.Name == mediaItemCollectionDetails.Artist.Name && a.Path == mediaCollectionDetails.Artist.Path))
            //        {
            //            artists.Add(mediaItemCollectionDetails.Artist);
            //            break;
            //        }
            //    }

            //    // Check sub-folders
            //    // D:\Data\Podcasts\Group 1\Rock.m3u (Artist.Name=Group 1)
            //    // D:\Data\Podcasts\Group 2\Blues.m3u (Artist.Name=Group 2)
            //    foreach (var subFolder in Directory.GetDirectories(mediaLocationSource))
            //    {
            //        foreach(var file in Directory.GetFiles(subFolder))
            //        {
            //            // Get playlist if any
            //            var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForPlaylist(file, false);
            //            if (mediaItemCollectionDetails != null &&
            //                !artists.Any(a => a.Name == mediaItemCollectionDetails.Artist.Name && a.Path == mediaCollectionDetails.Artist.Path))
            //            {
            //                artists.Add(mediaItemCollectionDetails.Artist);
            //                break;
            //            }
            //        }
            //    }
            //}

            if (includeNonReal)
            {
                //// Add all artist option           
                //if (artists.Count > 1)
                //{
                //    artists.Insert(0, Artist.InstanceAll);
                //}

                artists.Add(Artist.InstanceAll);
            }

            return artists;
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            // Check each file
            // NOTE: We don't check Artist because it's always "All"
            if (IsAvailable)
            {
                // Set action to read media item collections from folder
                Action<string> processFolder = (currentFolder) =>
                {
                    foreach (var file in Directory.GetFiles(currentFolder))
                    {
                        // Get playlist if any
                        var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForPlaylist(file, false);
                        if (mediaItemCollectionDetails != null)
                        {
                            mediaItemCollections.Add(mediaItemCollectionDetails.MediaItemCollection);
                        }
                    }
                };

                // Formats:
                // Source=D:\Playlists
                // MediaItemCollection=D:\Playlist\Rock.m3u (MediaItemCollection.Name=Rock, Artist.Name=Playlist)

                // Source=D:\Playlists
                // MediaItemCollection=D:\Playlists\Group1\Rock.m3u (MediaItemCollection.Name=Rock, Artist.Name=Group 1)
                // MediaItemCollection=D:\Playlists\Group1\Blues.m3u
                // MediaItemCollection=D:\Playlists\Group2\Rap.m3u
                // MediaItemCollection=D:\Playlists\Group2\RnB.m3u
                foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                {
                    processFolder(mediaLocationSource);        

                    // Check sub-folders
                    foreach (var subFolder in Directory.GetDirectories(mediaLocationSource))
                    {
                        processFolder(subFolder);
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
            
            // NOTE: We don't check Artist because it's always "All"
            if (IsAvailable)                
            {                
                if (mediaItemCollection.EntityCategory == EntityCategory.All)   // Media items for all playlists
                {
                    // Set action to load media items from folder
                    Action<string> processFolder = (currentFolder) =>
                    {
                        foreach (var file in Directory.GetFiles(currentFolder))
                        {
                            // Get media item collection for playlist
                            var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForPlaylist(file, true);
                            if (mediaItemCollectionDetails != null)
                            {
                                foreach (var mediaItem in mediaItemCollectionDetails.MediaItems)
                                {
                                    if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))   // Exclude duplicates
                                    {
                                        // Set image path to be album image if not set
                                        if (String.IsNullOrEmpty(mediaItem.ImagePath))
                                        {
                                            mediaItem.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                                        }
                                        mediaItems.Add(mediaItem);
                                    }
                                }
                            }
                        }
                    };

                    // Check each playlist file           
                    foreach (var mediaLocationSource in _mediaLocation.Sources.Where(f => Directory.Exists(f)))
                    {
                        processFolder(mediaLocationSource);

                        //foreach (var file in Directory.GetFiles(mediaLocationSource))
                        //{
                        //    // Get media item collection for playlist
                        //    var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForPlaylist(file, true);
                        //    if (mediaItemCollectionDetails != null)
                        //    {
                        //        foreach (var mediaItem in mediaItemCollectionDetails.MediaItems)
                        //        {
                        //            if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))   // Exclude duplicates
                        //            {
                        //                // Set image path to be album image if not set
                        //                if (String.IsNullOrEmpty(mediaItem.ImagePath))
                        //                {
                        //                    mediaItem.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                        //                }
                        //                mediaItems.Add(mediaItem);
                        //            }
                        //        }
                        //    }

                        //    //// Get playlist handler
                        //    //var playlistManager = _playlistManagers.FirstOrDefault(pl => pl.SupportsFile(playlistFile));
                        //    //if (playlistManager != null)   // Playlist
                        //    //{
                        //    //    playlistManager.FilePath = playlistFile;
                        //    //    var playlistMediaItems = playlistManager.GetAll();
                        //    //    foreach (var mediaItem in playlistMediaItems)
                        //    //    {
                        //    //        if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))
                        //    //        {
                        //    //            // Set image path to be album image if not set
                        //    //            if (String.IsNullOrEmpty(mediaItem.ImagePath))
                        //    //            {
                        //    //                mediaItem.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                        //    //            }
                        //    //            mediaItems.Add(mediaItem);
                        //    //        }
                        //    //    }
                        //    //    playlistManager.FilePath = "";  // Clean up
                        //    //}
                        //}

                        // Check sub-folders
                        foreach (var subFolder in Directory.GetDirectories(mediaLocationSource))
                        {
                            processFolder(subFolder);

                            //foreach (var file in Directory.GetFiles(subFolder))
                            //{
                            //    var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForPlaylist(file, false);
                            //    if (mediaItemCollectionDetails != null)
                            //    {
                            //        foreach(var mediaItem in mediaItemCollectionDetails.MediaItems)
                            //        {
                            //            if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))   // Exclude duplicates
                            //            {
                            //                // Set image path to be album image if not set
                            //                if (String.IsNullOrEmpty(mediaItem.ImagePath))
                            //                {
                            //                    mediaItem.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                            //                }
                            //                mediaItems.Add(mediaItem);
                            //            }
                            //        }                                    
                            //    }
                            //}
                        }
                    }
                }
                else if (mediaItemCollection.EntityCategory == EntityCategory.Real)    // Media items for specific playlist
                {
                    var playlistFile = mediaItemCollection.Path;
                    var mediaItemCollectionDetails = GetMediaItemCollectionDetailsForPlaylist(playlistFile, true);
                    if (mediaItemCollectionDetails != null)
                    {
                        // Set image path to be album image if not set
                        foreach (var mediaItem in mediaItemCollectionDetails.MediaItems.Where(mi => String.IsNullOrEmpty(mi.ImagePath)))
                        {
                            mediaItem.ImagePath = GetMediaItemCollectionImagePath(mediaItem);
                        }
                        mediaItems.AddRange(mediaItemCollectionDetails.MediaItems);
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
   
        public List<MediaAction> GetMediaActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var mediaActions = new List<MediaAction>();

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
                            var mediaAction = new MediaAction()
                            {
                                ActionType = MediaActionTypes.OpenMediaItemCollection,
                                MediaLocationName = mediaSource.MediaLocation.Name,                                
                                MediaItemFile = mediaItem.FilePath,            
                                //ImagePath = ancestors.Item2.ImagePath,  // Album image "picture.png"
                                ImagePath = "picture.png",  // No need to display album image, it's the main logo image
                                Name = String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.OpenMediaItemCollection)].ToString(),
                                        ancestors.Item2.Name)
                            };
                            mediaActions.Add(mediaAction);
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
                            var item = new MediaAction()
                            {
                                MediaLocationName = _mediaLocation.Name,
                                Name = isFoundMediaItem ?
                                         String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.RemoveFromPlaylist)].ToString(),
                                                playlistManager.Name) :
                                         String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaActionTypes.AddToPlaylist)].ToString(),
                                                playlistManager.Name),
                                MediaItemFile = mediaItem.FilePath,
                                PlaylistFile = playlistFile,
                                ImagePath = isFoundMediaItem ?
                                        "cross.png" :
                                        "plus.png",
                                ActionType = isFoundMediaItem ?
                                        MediaActionTypes.RemoveFromPlaylist :
                                        MediaActionTypes.AddToPlaylist
                            };
                            playlistManager.FilePath = "";  // Clean up

                            // If user currently has playlist selected then we only allow them to remove the media item from any playlist that 
                            // the media item is added to
                            if (currentMediaLocation.MediaSourceType == MediaSourceTypes.Playlist)
                            {
                                if (item.ActionType == MediaActionTypes.RemoveFromPlaylist)
                                {
                                    mediaActions.Add(item);
                                }
                            }
                            else
                            {
                                mediaActions.Add(item);
                            }                            
                        }
                    }
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
            if (IsAvailable)
            {
                var playlistManager = _playlistManagers.FirstOrDefault(pl => pl.SupportsFile(mediaAction.PlaylistFile));
                if (playlistManager != null)
                {                               
                    playlistManager.FilePath = mediaAction.PlaylistFile;
                    var mediaItems = playlistManager.GetAll();

                    // Load media item
                    MediaItem? mediaItem = String.IsNullOrEmpty(mediaAction.MediaItemFile) ? null :
                                            GetMediaItemByFileFromOriginalSource(mediaAction.MediaItemFile);

                    // Add or remove playlist item
                    var isSavePlaylist = true;
                    switch (mediaAction.ActionType)
                    {
                        case MediaActionTypes.AddToPlaylist:                            
                            if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))  // Not in playlist already
                            {
                                mediaItems.Add(mediaItem);
                            }                            
                            break;
                        case MediaActionTypes.ClearPlaylist:
                            mediaItems.Clear();
                            break;
                        case MediaActionTypes.DeletePlaylist:                            
                            File.Delete(mediaAction.PlaylistFile);
                            isSavePlaylist = false;
                            break;
                        case MediaActionTypes.RemoveFromPlaylist:
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
                                Name = playlistManager.Name,
                                ImagePath = MediaUtilities.GetMediaItemCollectionImagePath(Path.GetDirectoryName(playlistFile))
                            };                            
                            var mediaItemFound = mediaItems.FirstOrDefault(mi => mi.FilePath == mediaItem.FilePath);
                            if (mediaItemFound != null)
                            {
                                ancestors.Add(new Tuple<Artist, MediaItemCollection>(Artist.InstanceAll, mediaItemCollection));
                            }
                            playlistManager.FilePath = "";  // Clean up
                        }
                    }
                }
            }

            return ancestors;
        }

        /// <summary>
        /// Gets MediaItemCollectionDetails for file if it's a playlist file else null
        /// </summary>
        /// <param name="file">File to check</param>
        /// <param name="getMediaItems"></param>
        /// <returns></returns>
        private MediaItemCollectionDetails? GetMediaItemCollectionDetailsForPlaylist(string file, bool getMediaItems)
        {
            // File=D:\Data\Podcasts\Favourites.m3u
            if (File.Exists(file))
            {
                // Get IPlaylistManager (if any) that supports file
                var playlistManager = _playlistManagers.FirstOrDefault(pm => pm.SupportsFile(file));
                if (playlistManager != null)
                {
                    playlistManager.FilePath = file;
                    var mediaItems = playlistManager.GetAll();  // Needed in order to set IPlaylistManager.Name
                    MediaItemCollectionDetails mediaItemCollectionDetails = new MediaItemCollectionDetails()
                    {
                        Artist = new Artist()
                        {
                            Name = new DirectoryInfo(Path.GetDirectoryName(file)).Name, // Podcasts
                            Path = Path.GetDirectoryName(file)  // D:\Data\Podcasts
                        },
                        MediaItemCollection = new MediaItemCollection()
                        {
                            Name = playlistManager.Name,
                            Path = file,
                            ImagePath = MediaUtilities.GetMediaItemCollectionImagePath(Path.GetDirectoryName(file))
                        },
                        MediaItems = getMediaItems ? mediaItems : new()
                    };
                    playlistManager.FilePath = "";  // Clean up
                    return mediaItemCollectionDetails;
                }
            }

            return null;
        }

        public MediaItem? GetMediaItemByFile(string filePath)
        {
            return null;
        }
    }
}
