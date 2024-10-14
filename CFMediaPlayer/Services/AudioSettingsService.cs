using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class AudioSettingsService : XmlEntityWithIdStoreService<AudioSettings, string>, IAudioSettingsService
    {
        public AudioSettingsService(string folder) : base(folder, "AudioSettings.*.xml",
                                            (userSettings) => $"AudioSettings.{userSettings.Id}.xml",
                                            (id) => $"AudioSettings.{id}.xml")
        {

        }

        public void AddDefaults()
        {
            var presets = new string[]
            {
                "Normal",
                "Classical",
                "Dance",
                "Flat",
                "Folk",
                "Heavy Metal",
                "Hip Hop",
                "Jazz",
                "Pop",
                "Rock"
            };

            for (int i = 0; i < presets.Length; i++)
            {
                Update(new AudioSettings()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = presets[i],
                    PresetName = presets[i]
                });
            }
        }
    }
}
