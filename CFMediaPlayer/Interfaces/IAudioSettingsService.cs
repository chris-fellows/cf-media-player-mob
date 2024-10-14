using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    public interface IAudioSettingsService : IEntityWithIdService<AudioSettings, string>
    {
        void AddDefaults();
    }
}
