namespace CFMediaPlayer.Models
{
    /// <summary>
    /// Credentials for cloud storage
    /// </summary>
    public class CloudCredentials
    {
        /// <summary>
        /// Cloud provider Id (CloudProvider.Id)
        /// </summary>
        public string CloudProviderId { get; set; } = String.Empty;

        /// <summary>
        /// User name
        /// </summary>
        public string Username { get; set; } = String.Empty;

        /// <summary>
        /// Password
        /// </summary>
        public string Password { get; set; } = String.Empty;
    }
}
