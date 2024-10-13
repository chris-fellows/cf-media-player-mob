using CFMediaPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.ViewModels
{
    public class UserSettingsPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly IUserSettingsService _userSettingsService;

        public UserSettingsPageModel(IUserSettingsService userSettingsService)
        {
            _userSettingsService = userSettingsService;
        }
    }
}
