using CFMediaPlayer.Enums;
using System.Xml.Serialization;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Media item
    /// </summary>
    public class MediaItem : ICloneable
    {   
        /// <summary>
        /// Path to media item file
        /// </summary>
        public string FilePath { get; set; } = String.Empty;

        /// <summary>
        /// Display name
        /// </summary>
        public string Name { get; set; } = String.Empty;

        /// <summary>
        /// Path to image
        /// </summary>
        public string ImagePath { get; set; } = String.Empty;

        [XmlIgnore]
        public EntityCategory EntityCategory
        {
            get
            {
                if (String.IsNullOrEmpty(FilePath))
                {
                    if (Name == LocalizationResources.Instance["NoneText"].ToString())
                    {
                        return EntityCategory.None;
                    }
                    //else if (Name == LocalizationResources.Instance["MultipleText"].ToString())
                    //{
                    //    return EntityCategory.Multiple;
                    //}
                    else if (Name == LocalizationResources.Instance["AllText"].ToString())
                    {
                        return EntityCategory.All;
                    }
                }
                return EntityCategory.Real;
            }
        }

        public object Clone()
        {
            return new MediaItem()
            {
                FilePath = FilePath,
                Name = Name,
                ImagePath = ImagePath
            };
        }

        public static MediaItem InstanceNone => new MediaItem() { Name = LocalizationResources.Instance["NoneText"].ToString() };

        //public static MediaItem InstanceMultiple => new MediaItem() { Name = LocalizationResources.Instance["MultipleText"].ToString() };

        public static MediaItem InstanceAll => new MediaItem() { Name = LocalizationResources.Instance["AllText"].ToString() };

        /// <summary>
        /// Whether media item is streamed
        /// </summary>
        [XmlIgnore]
        public bool IsStreamed => FilePath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);
    }
}
