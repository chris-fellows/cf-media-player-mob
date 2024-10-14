using System.Xml.Serialization;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Media item
    /// </summary>
    public class MediaItem
    {        
        public string FilePath { get; set; } = String.Empty;

        [XmlIgnore] // Don't serialize
        public string Name => System.IO.Path.GetFileName(FilePath);
    }
}
