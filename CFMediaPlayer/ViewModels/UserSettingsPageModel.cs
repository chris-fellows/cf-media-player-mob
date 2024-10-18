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
using static Java.Util.Jar.Attributes;

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

        private readonly IAudioSettingsService _audioSettingsService;
        private readonly IUIThemeService _uiThemeService;
        private readonly IUserSettingsService _userSettingsService;

        private readonly UserSettings _userSettings;
        private List<UITheme> _uiThemes = new List<UITheme>();

        private List<AudioSettings> _audioSettingsList = new List<AudioSettings>();

        public UserSettingsPageModel(IAudioSettingsService audioSettingsService,
                                IUIThemeService uiThemeService,
                                IUserSettingsService userSettingsService)
        {
            _audioSettingsService = audioSettingsService;
            _uiThemeService = uiThemeService;
            _userSettingsService = userSettingsService;            

            // Set commands
            SaveCommand = new Command(Save);
            CancelCommand = new Command(Cancel);

            // Get current user settings
            _userSettings = _userSettingsService.GetByUsername(Environment.UserName)!;
          
            _uiThemes = _uiThemeService.GetAll();
            _audioSettingsList = _audioSettingsService.GetAll();

            // Set current settings
            SelectedUITheme = _uiThemes.First(t => t.Id == _userSettings.UIThemeId);
            SelectedAudioSettings = _audioSettingsList.First(a => a.Id == _userSettings.AudioSettingsId);
        }

        public ICommand SaveCommand { get; set; }

        public ICommand CancelCommand { get; set; }

        /// <summary>
        /// Saves user settings
        /// </summary>
        /// <param name="parameter"></param>
        private void Save(object parameter)
        {
            _userSettings.UIThemeId = _selectedUITheme.Id;
            _userSettings.AudioSettingsId = _selectedAudioSettings.Id;
            _userSettingsService.Update(_userSettings);
                     
            //Shell.Current.GoToAsync($"//{nameof(MainPage)}?UserSettingsUpdated={_userSettings.Id}");
            Shell.Current.GoToAsync($"//{nameof(MainPage)}?EventData=UserSettingsUpdated");
        }

        private void Cancel(object parameter)
        {            
            Shell.Current.GoToAsync($"//{nameof(MainPage)}");
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

                OnPropertyChanged(nameof(SelectedUITheme));
            }
        }

        public IList<AudioSettings> AudioSettingsList
        {
            get { return _audioSettingsList; }
        }

        /// <summary>
        /// Selected audio settings
        /// </summary>
        private AudioSettings _selectedAudioSettings;
        public AudioSettings SelectedAudioSettings
        {
            get { return _selectedAudioSettings; }
            set
            {
                _selectedAudioSettings = value;

                OnPropertyChanged(nameof(SelectedAudioSettings));
            }
        }
    }
}
