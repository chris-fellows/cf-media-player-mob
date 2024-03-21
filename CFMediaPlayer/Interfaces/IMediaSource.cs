using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Source for media items (Storage, playlists, HTTP etc)
    /// </summary>
    public interface IMediaSource
    {
        /// <summary>
        /// Sets source location (E.g. Root path)
        /// </summary>
        /// <param name="source"></param>
        void SetSource(string source);

        /// <summary>
        /// Whether media source is currently available. E.g. User may unmount SD card.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Name
        /// </summary>
        string Name { get; }

        List<Artist> GetArtists();
        
        List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName);

        List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string albumName);
    }
}
