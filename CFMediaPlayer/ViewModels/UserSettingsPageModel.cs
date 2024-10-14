using Android.Content;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using static Android.Media.Browse.MediaBrowser;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// View model for user settings
    /// </summary>
    public class UserSettingsPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly IUIThemeService _uiThemeService;
        private readonly IUserSettingsService _userSettingsService;

        private readonly UserSettings _userSettings;
        private List<UITheme> _uiThemes = new List<UITheme>();

        public UserSettingsPageModel(IUIThemeService uiThemeService,
                                IUserSettingsService userSettingsService)
        {
            _uiThemeService = uiThemeService;
            _userSettingsService = userSettingsService;            

            // Set commands
            SaveCommand = new Command(Save);

            // Get current user settings
            _userSettings = _userSettingsService.GetByUsername(Environment.UserName)!;
          
            _uiThemes = _uiThemeService.GetAll();
            SelectedUITheme = _uiThemes.First(t => t.Id == _userSettings.UIThemeId);
        }

        public ICommand SaveCommand { get; set; }

        /// <summary>
        /// Saves user settings
        /// </summary>
        /// <param name="parameter"></param>
        private void Save(object parameter)
        {
            _userSettings.UIThemeId = _selectedUITheme.Id;
        }

        /// <summary>
        /// UI themese
        /// </summary>
        public IList<UITheme> UIThemes
        {
            get { return _uiThemes; }
        }

        /// <summary>
        /// Selected UI theme
        /// </summary>
        private UITheme _selectedUITheme;
        public UITheme SelectedUITheme
        {
            get { return _selectedUITheme; }
            set
            {
                _selectedUITheme = value;
            }
        }
    }
}
