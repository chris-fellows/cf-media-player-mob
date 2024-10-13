using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly string _folder;

        public UserSettingsService(string folder)
        {
            _folder = folder;
        }

        public UserSettings Get()
        {
            var file = Path.Combine(_folder, "UserSettings.xml");

            UserSettings userSettings = null;
            if (File.Exists(file))
            {
                userSettings = XmlUtilities.DeserializeFromString<UserSettings>(file);
            }
            else
            {
                userSettings = new UserSettings();
            }
            return userSettings;
        }

        public void Update(UserSettings settings)
        {
            var file = Path.Combine(_folder, "UserSettings.xml");
            File.WriteAllText(file, XmlUtilities.SerializeToString(settings));
        }
    }
}
