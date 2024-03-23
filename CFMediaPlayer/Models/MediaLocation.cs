﻿using CFMediaPlayer.Enums;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Location of media (Local store, playlist etc)
    /// </summary>
    public class MediaLocation
    {
        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Indicates media source name (IMediaSource.Name) for accesing media items
        /// </summary>
        public MediaSourceTypes MediaSourceType { get; set; }

        public string RootFolderPath { get; set; } = String.Empty;
    }
}
