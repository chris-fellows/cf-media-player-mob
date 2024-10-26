namespace CFMediaPlayer.Models
{
    public class CustomAudioSettings : AudioSettings
    {
        /// <summary>
        /// Audio bands
        /// </summary>
        public List<short> AudioBands { get; set; } = new List<short>();
    }
}
