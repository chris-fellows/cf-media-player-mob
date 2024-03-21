using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

        public string Name => "Playlists";

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
            artists.Add(new Artist() { Path = "None" });   // Dummy artists
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
                        Path = file
                    };
                    mediaItemCollections.Add(itemCollection);
                }
            }

            return mediaItemCollections;
        }

        public List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string mediaItemCollectionName)
        {
            var mediaItems = new List<MediaItem>();

            // Check each file
            foreach (var file in Directory.GetFiles(_rootPath, "*"))
            {
                // Get playlist handler
                var playlist = _playlists.FirstOrDefault(pl => pl.SupportsFile(file));
                if (playlist != null)   // Playlist
                {
                    var itemCollection = new MediaItemCollection()
                    {
                        Path = file
                    };
                    if (mediaItemCollectionName == itemCollection.Name)   // IPlaylist found
                    {
                        mediaItems.AddRange(playlist.GetAll());
                        break;
                    }
                }
            }

            return mediaItems;
        }
    }
}
