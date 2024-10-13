using CFMediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Interfaces
{
    public interface IUserSettingsService
    {
        UserSettings Get();

        void Update(UserSettings settings);
    }
}
