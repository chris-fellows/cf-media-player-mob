namespace CFMediaPlayer.Enums
{
    /// <summary>
    /// Media play modes
    /// </summary>
    public enum MediaPlayModes
    {        
        /// <summary>
        /// Plays media item and stops
        /// </summary>
        SingleMediaItem,

        /// <summary>
        /// Plays each media item in the collection in sequence
        /// </summary>
        Sequential,

        /// <summary>
        /// Randomly plays media items in the collection
        /// </summary>
        ShuffleMediaItemCollection,

        /// <summary>
        /// Randomy plays media items in any collection for the artist
        /// </summary>
        ShuffleArtist,             

        /// <summary>
        /// Random plays media items in any collection for any artist
        /// </summary>
        ShuffleStorage             
    }
}
