using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Interface for media playlist
    /// </summary>
    public interface IPlaylist
    {        
        List<MediaItem> GetAll();

        void SaveAll(List<MediaItem> mediaItems);

        void SetFile(string file);

        bool SupportsFile(string file);
    }
}
