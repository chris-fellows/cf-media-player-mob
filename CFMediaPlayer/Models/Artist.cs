using CFMediaPlayer.Enums;
using System.Xml.Serialization;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Artist details
    /// </summary>
    public class Artist
    {
        /// <summary>
        /// Folder containing media item collections for artist
        /// </summary>
        public string Path { get; set; } = String.Empty;
     
        /// <summary>
        /// Artist name
        /// </summary>
        public string Name { get; set; } = String.Empty;

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
                    else if (Name == LocalizationResources.Instance["AllArtistsText"].ToString())
                    {
                        return EntityCategory.All;
                    }                    
                }
                return EntityCategory.Real;
            }
        }

        public static Artist InstanceNone => new Artist() { Name = LocalizationResources.Instance["NoneText"].ToString() };

        //public static Artist InstanceMultiple => new Artist() { Name = LocalizationResources.Instance["MultipleText"].ToString() };

        public static Artist InstanceAll => new Artist() { Name = LocalizationResources.Instance["AllArtistsText"].ToString() };
    }
}
