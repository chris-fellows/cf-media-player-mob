using System.ComponentModel.DataAnnotations;

namespace CFMediaPlayer.Enums
{
    /// <summary>
    /// Actions to execute against a media item or related entities
    /// </summary>
    public enum MediaItemActions
    {
        /// <summary>
        /// Add media item to playlist X
        /// </summary>
        [Display(Description = "MediaItemActionsAddToPlaylist")]
        AddToPlaylist,

        /// <summary>
        /// Add media item to end of queue
        /// </summary>
        [Display(Description = "MediaItemActionsAddToQueueEnd")]
        AddToQueueEnd,

        /// <summary>
        /// Add media item to start of queue
        /// </summary>
        [Display(Description = "MediaItemActionsAddToQueueNext")]
        AddToQueueNext,

        /// <summary>
        /// Clear playlist X
        /// </summary>
        [Display(Description = "MediaItemActionsClearPlaylist")]
        ClearPlaylist,

        /// <summary>
        /// Clear queue
        /// </summary>
        [Display(Description = "MediaItemActionsClearQueue")]
        ClearQueue,

        /// <summary>
        /// Delete playlist X
        /// </summary>
        [Display(Description = "MediaItemActionsDeletePlaylist")]
        DeletePlaylist,

        /// <summary>
        /// Open media item collection
        /// </summary>
        [Display(Description = "MediaItemActionsOpenMediaItemCollection")]
        OpenMediaItemCollection,

        /// <summary>
        /// Remove media item from playlist
        /// </summary>
        [Display(Description = "MediaItemActionRemoveFromPlaylist")]
        RemoveFromPlaylist,

        /// <summary>
        /// Remove media item from queue
        /// </summary>
        [Display(Description = "MediaItemActionRemoveFromQueue")]
        RemoveFromQueue
    }
}
