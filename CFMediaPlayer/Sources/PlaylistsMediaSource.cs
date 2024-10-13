using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Sources
{
    /// <summary>
    /// Media source from playlists    
    /// </summary>
    public class PlaylistsMediaSource : IMediaSource
    {
        private List<IPlaylist> _playlists;
        private string _rootPath;

        public PlaylistsMediaSource(IEnumerable<IPlaylist> playlists)
        {
            _playlists = playlists.ToList();
        }

        public MediaSourceTypes MediaSourceType => MediaSourceTypes.Playlist;

        public bool IsAvailable
        {
            get
            {
                return !String.IsNullOrEmpty(_rootPath) &&
                    Directory.Exists(_rootPath);
            }
        }

        public void SetSource(string source)
        {
            _rootPath = source;
        }


        public List<Artist> GetArtists()
        {
            var artists = new List<Artist>();
            artists.Add(new Artist() { Path = "None", Name = "None" });   // Dummy artists
            return artists;
        }

        public List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName)
        {
            var mediaItemCollections = new List<MediaItemCollection>();

            // Check each file
            foreach (var file in Directory.GetFiles(_rootPath))
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

            // Add New Playlist option
            var newPlaylistItemCollection = new MediaItemCollection()
            {
                Name = "[New Playlist]"
            };
            mediaItemCollections.Add(newPlaylistItemCollection);

            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string mediaItemCollectionName)
        {
            var mediaItems = new List<MediaItem>();

            // Check each file            
            foreach (var file in Directory.GetFiles(_rootPath))
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

        public List<MediaItemAction> GetActionsForMediaItem(MediaItem mediaItem)
        {
            var items = new List<MediaItemAction>();

            // Check each file
            foreach (var file in Directory.GetFiles(_rootPath))
            {
                // Get playlist handler
                var playlist = _playlists.FirstOrDefault(pl => pl.SupportsFile(file));
                if (playlist != null)   // Playlist
                {
                    // Check if media item in playlist
                    playlist.SetFile(file);
                    var mediaItems = playlist.GetAll();
                    var isFoundMediaItem = mediaItems.Any(mi => mi.FilePath == mediaItem.FilePath);

                    // TODO: Set language resources
                    var item = new MediaItemAction()
                    {
                        Name = isFoundMediaItem ?
                                $"Remove from playlist {Path.GetFileNameWithoutExtension(file)}" :
                                $"Add to playlist {Path.GetFileNameWithoutExtension(file)}",
                        File = file,
                        SelectedAction = isFoundMediaItem ?
                                MediaItemActions.RemoveFromPlaylist :
                                MediaItemActions.AddToPlaylist
                    };
                    items.Add(item);

                    playlist.SetFile("");
                }
            }

            // Add header
            if (!items.Any())
            {
                var itemNone = new MediaItemAction()
                {
                    Name = "Playlist actions..."
                };
                items.Add(itemNone);
            }

            return items;
        }

        public void ExecuteMediaItemAction(string playlistFile, MediaItem mediaItem, MediaItemActions mediaItemAction)
        {
            var playlist = _playlists.FirstOrDefault(pl => pl.SupportsFile(playlistFile));
            if (playlist != null)
            {
                playlist.SetFile(playlistFile);
                var mediaItems = playlist.GetAll();

                // Add or remove playlist item
                switch (mediaItemAction)
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
                        Artist = new Artist() { Name = "None" },
                        MediaItemCollection = mediaItemCollection
                    });
                }

                var mediaItems = GetMediaItemsForMediaItemCollection("", mediaItemCollection.Name);

                searchResults.AddRange(mediaItems.Where(mi => SearchUtilities.IsValidSearchResult(mi, searchOptions))
                       .Select(mi => new SearchResult()
                       {
                           EntityType = EntityTypes.MediaItem,
                           Name = mi.Name,
                           Artist = new Artist() { Name = "None" },
                           MediaItemCollection = mediaItemCollection,
                           MediaItem = mi
                       }));
            }

            return searchResults;
        }
    }
}
