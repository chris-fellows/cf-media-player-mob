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

        string ImagePath { get; }
        
        /// <summary>
        /// Whether media source is currently available. E.g. User may unmount SD card.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Whether to display media source in UI.
        /// </summary>
        bool IsDisplayInUI { get; }

        ///// <summary>
        ///// Whether there are media items available
        ///// </summary>
        //bool HasMediaItems { get; }

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
        List<MediaItemAction> GetActionsForMediaItem(MediaLocation currentMediaLocation, MediaItem mediaItem);

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

        /// <summary>
        /// Gets ancestors (Album, MediaItemCollection) for media item. For playlists then a media item can be 
        /// member of multiple playlists. For storage then it's only one.       
        /// </summary>
        /// <param name="mediaItem"></param>
        /// <returns></returns>
        /// <remarks>This method is typically used for finding the album image for a media item</remarks>
        List<Tuple<Artist, MediaItemCollection>> GetAncestorsForMediaItem(MediaItem mediaItem);

        /// <summary>
        /// Sets all media sources
        /// </summary>
        /// <param name="allMediaSources"></param>
        void SetAllMediaSources(List<IMediaSource> allMediaSources);
    }
}
