namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Cloud provider details
    /// </summary>
    public class CloudProvider
    {
        /// <summary>
        /// Unique Id
        /// </summary>
        public string Id { get; set; } = String.Empty;

        /// <summary>
        /// Resource key for Name
        /// </summary>
        public string NameResource { get; set; } = String.Empty;

        /// <summary>
        /// URL
        /// </summary>
        public string Url { get; set; } = String.Empty;
    }
}
