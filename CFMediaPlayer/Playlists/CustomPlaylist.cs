using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

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
            throw new NotImplementedException();
            return new List<MediaItem>();
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
            throw new NotImplementedException();
        }
    }
}
