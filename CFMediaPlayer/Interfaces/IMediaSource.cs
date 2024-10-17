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
        List<Artist> GetArtists(bool includeNonReal);

        /// <summary>
        /// Gets media item collections for artist
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="includeNonReal"></param>
        /// <returns></returns>
        List<MediaItemCollection> GetMediaItemCollectionsForArtist(Artist artist, bool includeNonReal);
      
        /// <summary>
        /// Gets media items for media item collection
        /// </summary>
        /// <param name="artist"></param>
        /// <param name="mediaItemCollection"></param>
        /// <param name="includeNonReal"></param>
        /// <returns></returns>
        List<MediaItem> GetMediaItemsForMediaItemCollection(Artist artist, MediaItemCollection mediaItemCollection, bool includeNonReal);

        /// <summary>
        /// Gets actions for media item. E.g. Add to playlist X, remove from playist Y, add to queue etc
        /// </summary>        
        /// <param name="mediaItem"></param>
        /// <returns></returns>
        List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem,
                                                 List<IMediaSource> allMediaSources);

        /// <summary>
        /// Executes action for media item. E.g. Add to playlist X, add to queue etc
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
