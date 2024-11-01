using AndroidX.Core.Util;
using CFMediaPlayer.Enums;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Xml.Serialization;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Media item
    /// </summary>
    public class MediaItem : ICloneable //, INotifyPropertyChanged
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

        /// <summary>
        /// Status image. E.g. None, playing, paused etc.
        /// </summary>
        [XmlIgnore]
        public string StatusImage { get; set; } = String.Empty;

        /// <summary>
        /// Whether the status image (.gif) should be animating
        /// </summary>
        [XmlIgnore]
        public bool IsStatusImageAnimating { get; set; }

        /// <summary>
        /// Image for play/pause/stop toggle
        /// </summary>
        [XmlIgnore]
        public string PlayToggleImage { get; set; } = String.Empty;

        //public event PropertyChangedEventHandler? PropertyChanged;
        
        //public void OnPropertyChanged([CallerMemberName] string name = "") =>
        //             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));    

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
                    else if (Name == LocalizationResources.Instance["AllMediaItemsText"].ToString())
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
                ImagePath = ImagePath,
                StatusImage = StatusImage
            };
        }

        public static MediaItem InstanceNone => new MediaItem() { Name = LocalizationResources.Instance["NoneText"].ToString() };

        //public static MediaItem InstanceMultiple => new MediaItem() { Name = LocalizationResources.Instance["MultipleText"].ToString() };

        public static MediaItem InstanceAll => new MediaItem() { Name = LocalizationResources.Instance["AllMediaItemsText"].ToString() };

        /// <summary>
        /// Whether media item is streamed
        /// </summary>
        [XmlIgnore]
        public bool IsStreamed => FilePath.StartsWith("http", StringComparison.InvariantCultureIgnoreCase);

        /// <summary>
        /// Whether media item can be paused. If not then can only be stopped.
        /// </summary>
        [XmlIgnore]
        public bool IsPausable => !IsStreamed;

        /// <summary>
        /// Whether media can be played
        /// </summary>
        [XmlIgnore]
        public bool IsPlayable
        {
            get
            {
                if (IsStreamed)    // Assume that all streamed media needs internet
                {
                    NetworkAccess accessType = Connectivity.Current.NetworkAccess;
                    return Array.IndexOf(new[] { NetworkAccess.Internet, NetworkAccess.ConstrainedInternet }, accessType) != -1;                                        
                }

                return !String.IsNullOrEmpty(FilePath) && File.Exists(FilePath);
            }
        }
    }
}
