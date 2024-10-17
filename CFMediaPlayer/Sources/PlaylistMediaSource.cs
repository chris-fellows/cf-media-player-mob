using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from playlists    
    /// </summary>
    public class PlaylistMediaSource : IMediaSource
    {
        private readonly MediaLocation _mediaLocation;
        private List<IPlaylist> _playlists;        

        public PlaylistMediaSource(MediaLocation mediaLocation, IEnumerable<IPlaylist> playlists)
        {
            _mediaLocation = mediaLocation;
            _playlists = playlists.ToList();
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
            if (includeNonReal)
            {
                return new List<Artist>()
                {
                    new Artist() { Name = LocalizationResources.Instance["MultipleText"].ToString() }
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
                foreach (var file in Directory.GetFiles(_mediaLocation.Source))
                {
                    // Get playlist handler
                    var playlist = _playlists.FirstOrDefault(pl => pl.SupportsFile(file));
                    if (playlist != null)   // Playlist
                    {
                        var itemCollection = new MediaItemCollection()
                        {
                            Path = file,
                            Name = Path.GetFileNameWithoutExtension(file)
                        };
                        mediaItemCollections.Add(itemCollection);
                    }
                }
            }

            if (includeNonReal)
            {
                // Add shuffle media item collection option
                if (mediaItemCollections.Count > 1)
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

        public List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, bool includeNonReal)
        {
            var mediaItems = new List<MediaItem>();
            
            if (IsAvailable)                
            {
                if (MediaUtilities.IsShuffleMediaItemCollection(mediaItemCollection))
                {
                    // Get media items for all playlists
                    // Check each file            
                    foreach (var file in Directory.GetFiles(_mediaLocation.Source))
                    {
                        // Get playlist handler
                        var playlist = _playlists.FirstOrDefault(pl => pl.SupportsFile(file));
                        if (playlist != null)   // Playlist
                        {                            
                            playlist.SetFile(file);
                            var playlistMediaItems = playlist.GetAll();
                            foreach(var mediaItem in playlistMediaItems)
                            {
                                if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))
                                {
                                    mediaItems.Add(mediaItem);                                       
                                }
                            }                    
                            playlist.SetFile("");                                                           
                        }
                    }
                }
                else if (MediaUtilities.IsRealMediaItemCollection(mediaItemCollection))
                {
                    // Get media items for specific playlist
                    // Check each file            
                    foreach (var file in Directory.GetFiles(_mediaLocation.Source))
                    {
                        // Get playlist handler
                        var playlist = _playlists.FirstOrDefault(pl => pl.SupportsFile(file));
                        if (playlist != null)   // Playlist
                        {
                            var itemCollection = new MediaItemCollection()
                            {
                                Path = file,
                                Name = Path.GetFileNameWithoutExtension(file)
                            };
                            if (mediaItemCollection.Name == itemCollection.Name)   // IPlaylist found
                            {
                                playlist.SetFile(file);
                                mediaItems.AddRange(playlist.GetAll());
                                playlist.SetFile("");
                                break;
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
            var items = new List<MediaItemAction>();

            // Check each file
            if (IsAvailable)
            {
                foreach (var file in Directory.GetFiles(_mediaLocation.Source))
                {
                    // Get playlist handler
                    var playlist = _playlists.FirstOrDefault(pl => pl.SupportsFile(file));
                    if (playlist != null)   // Playlist
                    {
                        // Check if media item in playlist
                        playlist.SetFile(file);
                        var mediaItems = playlist.GetAll();
                        var isFoundMediaItem = mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath);
                        var playlistName = Path.GetFileNameWithoutExtension(file);

                        // Create media item action
                        var item = new MediaItemAction()
                        {
                            MediaLocationName = _mediaLocation.Name,
                            Name = isFoundMediaItem ?
                                     String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.RemoveFromPlaylist)].ToString(),
                                            playlistName) :
                                     String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.AddToPlaylist)].ToString(),
                                            playlistName),
                            File = file,
                            ActionToExecute = isFoundMediaItem ?
                                    MediaItemActions.RemoveFromPlaylist :
                                    MediaItemActions.AddToPlaylist
                        };

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

                        playlist.SetFile("");
                    }
                }
            }

            return items;
        }

        public void ExecuteMediaItemAction(MediaItem mediaItem, MediaItemAction mediaItemAction)
        {
            if (IsAvailable)
            {
                var playlist = _playlists.FirstOrDefault(pl => pl.SupportsFile(mediaItemAction.File));
                if (playlist != null)
                {
                    playlist.SetFile(mediaItemAction.File);
                    var mediaItems = playlist.GetAll();

                    // Add or remove playlist item
                    switch (mediaItemAction.ActionToExecute)
                    {
                        case MediaItemActions.AddToPlaylist:
                            if (!mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath))  // Not in playlist already
                            {
                                mediaItems.Add(mediaItem);
                            }
                            break;
                        case MediaItemActions.ClearPlaylist:
                            playlist.SaveAll(new());
                            break;
                        case MediaItemActions.DeletePlaylist:                            
                            File.Delete(mediaItemAction.File);                            
                            break;
                        case MediaItemActions.RemoveFromPlaylist:
                            mediaItems.RemoveAll(mi => mi.FilePath == mediaItem.FilePath);
                            break;
                    }

                    playlist.SaveAll(mediaItems);
                    playlist.SetFile("");
                }
            }
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();

            var mediaItemCollections = GetMediaItemCollectionsForArtist(null, false);    // Playlists

            foreach(var mediaItemCollection in mediaItemCollections)
            {
                if (SearchUtilities.IsValidSearchResult(mediaItemCollection, searchOptions))
                {
                    searchResults.Add(new SearchResult()
                    {
                        EntityType = EntityTypes.MediaItemCollection,
                        Name = mediaItemCollection.Name,
                        Artist = new Artist() { Name = LocalizationResources.Instance["NoneText"].ToString() },
                        MediaItemCollection = mediaItemCollection
                    });
                }

                var mediaItems = GetMediaItemsForMediaItemCollection(null, mediaItemCollection, false);

                searchResults.AddRange(mediaItems.Where(mi => SearchUtilities.IsValidSearchResult(mi, searchOptions))
                       .Select(mi => new SearchResult()
                       {
                           EntityType = EntityTypes.MediaItem,
                           Name = mi.Name,
                           Artist = new Artist() { Name = LocalizationResources.Instance["NoneText"].ToString() },
                           MediaItemCollection = mediaItemCollection,
                           MediaItem = mi
                       }));
            }

            return searchResults;
        }
    }
}
