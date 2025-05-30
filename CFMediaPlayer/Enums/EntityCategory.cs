﻿namespace CFMediaPlayer.Enums
{
    /// <summary>
    /// Entity category for Artist, MediaItemCollection and MediaItem in the UI.
    /// </summary>
    public enum EntityCategory
    {
        /// <summary>
        /// All entities. E.g. All media item collections for artist so that user can play media items from
        /// any album in shuffle play order.
        /// </summary>
        All,

        /// <summary>
        /// No item selected. Typically when no real items in the list.
        /// </summary>
        None,

        /// <summary>
        /// Real entity. Refers to an artist, media item collection or media item
        /// </summary>
        Real
    }
}
