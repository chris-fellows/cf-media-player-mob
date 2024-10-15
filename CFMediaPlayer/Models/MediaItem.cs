using System.Xml.Serialization;

namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Media item
    /// </summary>
    public class MediaItem
    {        
        public string FilePath { get; set; } = String.Empty;

        //public string Name => System.IO.Path.GetFileName(FilePath);
        public string Name { get; set; } = String.Empty;
    }
}
