using CFMediaPlayer.Enums;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Source for media items (Storage, playlists, HTTP etc)
    /// </summary>
    public interface IMediaSource
    {
        /// <summary>
        /// Media location
        /// </summary>
        MediaLocation MediaLocation { get; }
        
        /// <summary>
        /// Whether media source is currently available. E.g. User may unmount SD card.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Gets all artists
        /// </summary>
        /// <returns></returns>
        List<Artist> GetArtists();
        
        /// <summary>
        /// Gets media item collections for artist
        /// </summary>
        /// <param name="artistName"></param>
        /// <returns></returns>
        List<MediaItemCollection> GetMediaItemCollectionsForArtist(string artistName);

        /// <summary>
        /// Gets media items for artist and album
        /// </summary>
        /// <param name="artistName"></param>
        /// <param name="albumName"></param>
        /// <returns></returns>
        List<MediaItem> GetMediaItemsForMediaItemCollection(string artistName, string albumName);

        /// <summary>
        /// Gets playlists that media item can be added to
        /// </summary>        
        /// <param name="mediaItem"></param>
        /// <returns></returns>
        List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem);

        /// <summary>
        /// Executes action for media item. E.g. Add to playlist, add to queue etc
        /// </summary>        
        /// <param name="mediaItem"></param>
        /// <param name="mediaItemAction"></param>
        void ExecuteMediaItemAction(MediaItem mediaItem, MediaItemAction mediaItemAction);

        /// <summary>
        /// Search media source
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <returns></returns>
        List<SearchResult> Search(SearchOptions searchOptions);
    }
}
