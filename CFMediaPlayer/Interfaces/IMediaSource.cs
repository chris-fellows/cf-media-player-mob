﻿using CFMediaPlayer.Enums;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Source for media items (Storage, playlists, HTTP etc)
    /// </summary>
    public interface IMediaSource
    {
        /// <summary>
        /// Sets source location (E.g. Root path)
        /// </summary>
        /// <param name="source"></param>
        void SetSource(string source);

        /// <summary>
        /// Whether media source is currently available. E.g. User may unmount SD card.
        /// </summary>
        bool IsAvailable { get; }

        /// <summary>
        /// Media source type
        /// </summary>
        MediaSourceTypes MediaSourceType { get; }

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
        /// <param name="isPlaylistMediaSourceSelected"></param>
        /// <param name="mediaItem"></param>
        /// <returns></returns>
        List<PlaylistAction> GetPlaylistActionsForMediaItem(bool isPlaylistMediaSourceSelected, MediaItem mediaItem);

        /// <summary>
        /// Executes action for adding/removing media item for playlist
        /// </summary>
        /// <param name="playlistFile"></param>
        /// <param name="mediaItem"></param>
        /// <param name="playlistAction"></param>
        void ExecutePlaylistAction(string playlistFile, MediaItem mediaItem, PlaylistActions playlistAction);        
    }
}
