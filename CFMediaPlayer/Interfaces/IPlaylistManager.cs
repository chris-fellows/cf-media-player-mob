using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Interface for managing playlist
    /// </summary>
    public interface IPlaylistManager
    {
        /// <summary>
        /// Playlist file
        /// </summary>
        string FilePath { get; set; }

        /// <summary>
        /// Playlist name
        /// </summary>
        string Name { get; set; }

        /// <summary>
        /// Returns all playlist items
        /// </summary>
        /// <returns></returns>
        List<MediaItem> GetAll();

        /// <summary>
        /// Updates playlist
        /// </summary>
        /// <param name="mediaItems"></param>
        void SaveAll(List<MediaItem> mediaItems);

        ///// <summary>
        ///// Sets file to read/write
        ///// </summary>
        ///// <param name="file"></param>
        //void SetFile(string file);

        /// <summary>
        /// Whether file is supported
        /// </summary>
        /// <param name="file"></param>
        /// <returns></returns>
        bool SupportsFile(string file);
    }
}
