using System.ComponentModel.DataAnnotations;

namespace CFMediaPlayer.Enums
{
    /// <summary>
    /// Actions to execute against a media item
    /// </summary>
    public enum MediaItemActions
    {
        [Display(Description = "MediaItemActionsAddToPlaylist")]
        AddToPlaylist,

        [Display(Description = "MediaItemActionsAddToQueueEnd")]
        AddToQueueEnd,

        [Display(Description = "MediaItemActionsAddToQueueNext")]
        AddToQueueNext,

        [Display(Description = "MediaItemActionsClearPlaylist")]
        ClearPlaylist,

        [Display(Description = "MediaItemActionsClearQueue")]
        ClearQueue,

        [Display(Description = "MediaItemActionsDeletePlaylist")]
        DeletePlaylist,

        [Display(Description = "MediaItemActionRemoveFromPlaylist")]
        RemoveFromPlaylist,

        [Display(Description = "MediaItemActionRemoveFromQueue")]
        RemoveFromQueue
    }
}
