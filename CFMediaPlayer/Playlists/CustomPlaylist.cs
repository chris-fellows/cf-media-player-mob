using Android.Telephony;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System.IO;

namespace CFMediaPlayer.Playlists
{
    /// <summary>
    /// Custom playlist format
    /// </summary>
    public class CustomPlaylist : IPlaylistManager
    {
        private string? _file;

        public string Name { get; set; } = String.Empty;

        public string FilePath
        {
            get { return _file; }
            set
            {
                _file = value;
                Name = "";
            }
        }

        public List<MediaItem> GetAll()
        {            
            var playlist = XmlUtilities.DeserializeFromString<Playlist>(File.ReadAllText(_file, System.Text.Encoding.UTF8));

            Name = playlist.Name;
            return playlist.Items.Select(item =>

                new MediaItem()
                {
                    FilePath = item.FilePath,
                    Name = item.Name
                }
            ).ToList();            
        }    

        //public void SetFile(string file)
        //{
        //    _file = file;
        //    Name = "";
        //}

        public bool SupportsFile(string file)
        {
            return Path.GetExtension(file).ToLower().Equals(".playlist");
        }

        public void SaveAll(List<MediaItem> mediaItems)
        {
            if (File.Exists(_file))
            {
                File.Delete(_file);
            }

            var playlist = new Playlist()
            {
                Name = Name,
                Items = mediaItems.Select(item =>
                    new PlaylistItem() { Name = item.Name, FilePath = item.FilePath }
                ).ToList()                                
            };

            File.WriteAllText(_file, XmlUtilities.SerializeToString(playlist), System.Text.Encoding.UTF8);
        }
    }
}
