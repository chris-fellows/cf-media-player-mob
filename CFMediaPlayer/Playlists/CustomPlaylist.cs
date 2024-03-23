using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System.IO;

namespace CFMediaPlayer.Playlists
{
    /// <summary>
    /// Custom playlist format
    /// </summary>
    public class CustomPlaylist : IPlaylist
    {
        private string? _file;

        public List<MediaItem> GetAll()
        {
            var playlist = XmlUtilities.DeserializeFromString<Playlist>(File.ReadAllText(_file, System.Text.Encoding.UTF8));

            return playlist.Items.Select(item =>

                new MediaItem()
                {
                    FilePath = item.FilePath
                }
            ).ToList();            
        }

        public void SetFile(string file)
        {
            _file = file;
        }

        public bool SupportsFile(string file)
        {
            return Path.GetExtension(file).ToLower().Equals(".playlist");
        }

        public void SaveAll(List<MediaItem> mediaItems)
        {
            var playlist = new Playlist()
            {
                Name = Path.GetFileNameWithoutExtension(_file),
                Items = mediaItems.Select(item =>
                    new PlaylistItem() { FilePath = item.FilePath }
                ).ToList()                                
            };

            File.WriteAllText(_file, XmlUtilities.SerializeToString(playlist));
        }
    }
}
