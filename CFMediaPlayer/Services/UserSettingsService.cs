using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class UserSettingsService : XmlEntityWithIdStoreService<UserSettings, string>, IUserSettingsService
    {        
        public UserSettingsService(string folder) : base(folder, "UserSettings.*.xml",
                                            (userSettings) => $"UserSettings.{userSettings.Id}.xml",
                                            (id) => $"UserSettings.{id}.xml")
        {
     
        }

        public UserSettings? GetByUsername(string username)
        {
            return GetAll().FirstOrDefault(us => us.Username == username);
        }
    }
}
