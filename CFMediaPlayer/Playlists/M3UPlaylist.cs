using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Playlists
{
    /// <summary>
    /// M3U playlist
    /// </summary>
    public class M3UPlaylist : IPlaylist
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
            return Path.GetExtension(file).ToLower().Equals(".m3u");
        }

        public void SaveAll(List<MediaItem> mediaItems)
        {
            throw new NotImplementedException();
        }
    }
}
