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
        /// Playlists. Won't include radio streams that are in the RadioStreams folder.
        /// </summary>
        Playlist,

        /// <summary>
        /// Queue (in memory) of media items to play. User can manually add media items to the queue.
        /// </summary>
        Queue,

        /// <summary>
        /// Radio streams. These are just playlists that are located in a different location to normal
        /// playlists.
        /// </summary>
        RadioStreams,

        /// <summary>
        /// Internal or external phone storage for music, podcasts and audiobooks
        /// </summary>
        /// <remarks>No real need to split between external or internal</remarks>
        Storage
    }
}
