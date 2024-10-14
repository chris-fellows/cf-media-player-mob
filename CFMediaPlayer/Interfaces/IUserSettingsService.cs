using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Service for UserSettings
    /// </summary>
    public interface IUserSettingsService : IEntityWithIdService<UserSettings, string>
    {
        UserSettings? GetByUsername(string username);        
    }
}
