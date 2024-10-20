namespace CFMediaPlayer.Enums
{
    /// <summary>
    /// Media source types
    /// </summary>
    public enum MediaSourceTypes
    {
        /// <summary>
        /// Cloud media (OneDrive, AWS etc)
        /// </summary>
        Cloud,

        /// <summary>
        /// Playlists
        /// </summary>
        Playlist,

        /// <summary>
        /// Queue (In memory)
        /// </summary>
        Queue,

        /// <summary>
        /// Radio streams. These are just playlists that are located in a different location to normal
        /// playlists.
        /// </summary>
        RadioStreams,

        /// <summary>
        /// Internal or external phone storage
        /// </summary>
        /// <remarks>No real need to split between external or internal</remarks>
        Storage
    }
}
