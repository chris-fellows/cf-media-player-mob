﻿using CFMediaPlayer.Enums;
using System.Xml.Serialization;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Collection of media items (Album, playlist etc)
    /// </summary>
    public class MediaItemCollection
    {
        /// <summary>
        /// Folder where media item collection stored
        /// </summary>
        public string Path { get; set; } = String.Empty;        

        /// <summary>
        /// Name
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Path to image (Album artwork)
        /// </summary>
        public string ImagePath { get; set; } = String.Empty;

        [XmlIgnore]
        public EntityCategory EntityCategory
        {
            get
            {
                if (String.IsNullOrEmpty(Path))
                {
                    if (Name == LocalizationResources.Instance["NoneText"].ToString())
                    {
                        return EntityCategory.None;
                    }
                    //else if (Name == LocalizationResources.Instance["MultipleText"].ToString())
                    //{
                    //    return EntityCategory.Multiple;
                    //}
                    else if (Name == LocalizationResources.Instance["AllMediaItemCollectionsText"].ToString())
                    {
                        return EntityCategory.All;
                    }
                }
                return EntityCategory.Real;
            }
        }

        public static MediaItemCollection InstanceNone => new MediaItemCollection() { Name = LocalizationResources.Instance["NoneText"].ToString() };

        //public static MediaItemCollection InstanceMultiple => new MediaItemCollection() { Name = LocalizationResources.Instance["MultipleText"].ToString() };

        public static MediaItemCollection InstanceAll => new MediaItemCollection() { Name = LocalizationResources.Instance["AllMediaItemCollectionsText"].ToString() };
    }
}
