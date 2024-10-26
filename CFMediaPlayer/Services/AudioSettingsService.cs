using CFMediaPlayer.Constants;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class AudioSettingsService : XmlEntityWithIdStoreService<AudioSettings, string>, IAudioSettingsService
    {
        private readonly IAudioEqualizer _audioEqualizer;

        public AudioSettingsService(IAudioEqualizer audioEqualizer,
                                            string folder) : base(folder, "AudioSettings.*.xml",
                                            (userSettings) => $"AudioSettings.{userSettings.Id}.xml",
                                            (id) => $"AudioSettings.{id}.xml")
        {
            _audioEqualizer = audioEqualizer;
        }

        public void AddDefaults()
        {
            //var presets = new string[]
            //{
            //    "Custom",       // Special handling
            //    ,
            //    "Classical",
            //    "Dance",
            //    "Flat",
            //    "Folk",
            //    "Heavy Metal",
            //    "Hip Hop",
            //    "Jazz",
            //    "Pop",
            //    "Rock"
            //};
            
            var presetNames = _audioEqualizer.PresetNames;
            //presetNames.Insert(0, GeneralConstants.CustomPresetName);
            foreach(var presetName in presetNames)
            {
                Update(new AudioSettings()
                {
                    Id = Guid.NewGuid().ToString(),
                    Name = presetName
                    //PresetName = presetName
                });
            }
        }
    }
}
