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

        public List<Artist> GetArtists()
        {
            var artists = new List<Artist>();
            artists.Add(new Artist() { Name = LocalizationResources.Instance["None"].ToString() });   // Dummy artists
            return artists;
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName)
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

            // Add New Playlist option
            // NOTE: If there are no playlists then the app will display the New Playlist page as soon as the current
            // page is population because it auto-selects the first media item collection.
            var newPlaylistItemCollection = new MediaItemCollection()
            {
                Name = LocalizationResources.Instance["NewPlaylistText"].ToString()
            };
            mediaItemCollections.Add(newPlaylistItemCollection);

            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string mediaItemCollectionName)
        {
            var mediaItems = new List<MediaItem>();

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
                    if (mediaItemCollectionName == itemCollection.Name)   // IPlaylist found
                    {
                        playlist.SetFile(file);
                        mediaItems.AddRange(playlist.GetAll());
                        playlist.SetFile("");
                        break;
                    }
                }
            }

            return mediaItems;
        }

        public List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem)
        {
            var items = new List<MediaItemAction>();

            // Check each file
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

                    // Create media item action
                    var item = new MediaItemAction()
                    {
                        MediaLocationName = _mediaLocation.Name,
                        Name = isFoundMediaItem ?
                                 String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.RemoveFromPlaylist)].ToString(),
                                        Path.GetFileNameWithoutExtension(file)) :
                                 String.Format(LocalizationResources.Instance[InternalUtilities.GetEnumResourceKey(MediaItemActions.AddToPlaylist)].ToString(),
                                        Path.GetFileNameWithoutExtension(file)),                                                              
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

            return items;
        }

        public void ExecuteMediaItemAction(MediaItem mediaItem, MediaItemAction mediaItemAction)
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
                    case MediaItemActions.RemoveFromPlaylist:
                        mediaItems.RemoveAll(mi => mi.FilePath == mediaItem.FilePath);
                        break;
                }

                playlist.SaveAll(mediaItems);

                playlist.SetFile("");
            }
        }

        public List<SearchResult> Search(SearchOptions searchOptions)
        {
            var searchResults = new List<SearchResult>();

            var mediaItemCollections = GetMediaItemCollectionsForArtist("");    // Playlists

            foreach(var mediaItemCollection in mediaItemCollections)
            {
                if (SearchUtilities.IsValidSearchResult(mediaItemCollection, searchOptions))
                {
                    searchResults.Add(new SearchResult()
                    {
                        EntityType = EntityTypes.MediaItemCollection,
                        Name = mediaItemCollection.Name,
                        Artist = new Artist() { Name = LocalizationResources.Instance["None"].ToString() },
                        MediaItemCollection = mediaItemCollection
                    });
                }

                var mediaItems = GetMediaItemsForMediaItemCollection("", mediaItemCollection.Name);

                searchResults.AddRange(mediaItems.Where(mi => SearchUtilities.IsValidSearchResult(mi, searchOptions))
                       .Select(mi => new SearchResult()
                       {
                           EntityType = EntityTypes.MediaItem,
                           Name = mi.Name,
                           Artist = new Artist() { Name = LocalizationResources.Instance["None"].ToString() },
                           MediaItemCollection = mediaItemCollection,
                           MediaItem = mi
                       }));
            }

            return searchResults;
        }
    }
}
