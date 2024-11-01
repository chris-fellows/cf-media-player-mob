using System.ComponentModel.DataAnnotations;

namespace CFMediaPlayer.Enums
{
    /// <summary>
    /// Action types for media. Typically refers to media item but can refer to other entities
    /// </summary>
    public enum MediaActionTypes
    {
        /// <summary>
        /// Add random media items to queue
        /// </summary>
        [Display(Description = "MediaItemActionsAddRandomItemsToQueue")]            
        AddRandomItemsToQueue,

        /// <summary>
        /// Add media item to playlist X
        /// </summary>
        [Display(Description = "MediaItemActionsAddToPlaylist")]
        AddToPlaylist,          // Level=MediaItem

        /// <summary>
        /// Add media item to end of queue
        /// </summary>
        [Display(Description = "MediaItemActionsAddToQueueEnd")]
        AddToQueueEnd,          // Level=MediaItem

        ///// <summary>
        ///// Add media item to start of queue
        ///// </summary>
        //[Display(Description = "MediaItemActionsAddToQueueNext")]
        //AddToQueueNext,         // Level=MediaItem

        /// <summary>
        /// Clear playlist X
        /// </summary>
        [Display(Description = "MediaItemActionsClearPlaylist")]
        ClearPlaylist,          // Level=MediaLocation

        /// <summary>
        /// Clear queue
        /// </summary>
        [Display(Description = "MediaItemActionsClearQueue")]
        ClearQueue,             // Level=MediaLocation

        /// <summary>
        /// Delete playlist X
        /// </summary>
        [Display(Description = "MediaItemActionsDeletePlaylist")]
        DeletePlaylist,        // Level=MediaItem

        /// <summary>
        /// Open media item collection
        /// </summary>
        [Display(Description = "MediaItemActionsOpenMediaItemCollection")]
        OpenMediaItemCollection,    // Level=MediaItem

        /// <summary>
        /// Remove media item from playlist
        /// </summary>
        [Display(Description = "MediaItemActionRemoveFromPlaylist")]
        RemoveFromPlaylist,     // Level=MediaItem

        /// <summary>
        /// Remove media item from queue
        /// </summary>
        [Display(Description = "MediaItemActionRemoveFromQueue")]
        RemoveFromQueue         // Level=MediaItem
    }
}
