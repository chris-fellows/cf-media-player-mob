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
        private readonly ICurrentState _currentState;
        private readonly IUIThemeService _uiThemeService;
        private readonly IUserSettingsService _userSettingsService;

        private readonly UserSettings _userSettings;
        private List<UITheme> _uiThemes = new List<UITheme>();

        private List<AudioSettings> _audioSettingsList = new List<AudioSettings>();

        public UserSettingsPageModel(IAudioSettingsService audioSettingsService,
                                ICurrentState currentState,
                                IUIThemeService uiThemeService,
                                IUserSettingsService userSettingsService)
        {
            _audioSettingsService = audioSettingsService;
            _currentState = currentState;
            _uiThemeService = uiThemeService;
            _userSettingsService = userSettingsService;            

            // Set commands
            SaveCommand = new Command(Save);
            CancelCommand = new Command(Cancel);
            RefreshCommand = new Command(Refresh);

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

        public ICommand RefreshCommand { get; set; }

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
            //Shell.Current.GoToAsync($"//{nameof(MainPage)}?EventData=UserSettingsUpdated");

            if (_currentState.UserSettingsUpdatedAction != null)
            {
                _currentState.UserSettingsUpdatedAction();
            }
        }

        private void Cancel(object parameter)
        {            
            //Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }

        private void Refresh(object parameter)
        {
            //Shell.Current.GoToAsync($"//{nameof(MainPage)}");

            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMax0));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMax1));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMax2));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMax3));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMax4));

            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMin0));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMin1));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMin2));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMin3));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeMin4));

            OnPropertyChanged(nameof(EqualizerBandLevelRangeMin));
            OnPropertyChanged(nameof(EqualizerBandLevelRangeMax));

            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeText0));            
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeText1));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeText2));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeText3));
            OnPropertyChanged(nameof(EqualizerBandFrequencyRangeText4));
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

        public short EqualizerBandLevelRangeMin
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevelRange();
                return value == null ? (short)0 : value[0];
            }
        }

        public short EqualizerBandLevelRangeMax
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevelRange();
                return value == null ? (short)0 : value[1];
            }
        }

        public short EqualizerBandLevel0
        {
            get
            {                
                return _currentState == null ||  _currentState.MediaPlayer == null ? (short)0 :
                      _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevel(0);
            }
            set
            {
                if (_currentState != null && _currentState.MediaPlayer != null)
                {
                    _currentState.MediaPlayer.AudioEqualizer.SetBandLevel(0, value);
                }
            }
        }

        public short EqualizerBandLevel1
        {
            get
            {
                return _currentState == null ||  _currentState.MediaPlayer == null ? (short)0 :
                      _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevel(1);
            }
            set
            {
                if (_currentState != null && _currentState.MediaPlayer != null)
                {
                    _currentState.MediaPlayer.AudioEqualizer.SetBandLevel(1, value);
                }
            }
        }

        public short EqualizerBandLevel2
        {
            get
            {
                return _currentState == null || _currentState.MediaPlayer == null ? (short)0 :
                      _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevel(2);
            }
            set
            {
                if (_currentState != null && _currentState.MediaPlayer != null)
                {
                    _currentState.MediaPlayer.AudioEqualizer.SetBandLevel(2, value);
                }
            }
        }

        public short EqualizerBandLevel3
        {
            get
            {
                return _currentState == null || _currentState.MediaPlayer == null ? (short)0 :
                      _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevel(3);
            }
            set
            {
                if (_currentState != null && _currentState.MediaPlayer != null)
                {
                    _currentState.MediaPlayer.AudioEqualizer.SetBandLevel(3, value);
                }
            }
        }

        public short EqualizerBandLevel4
        {
            get
            {
                return _currentState == null || _currentState.MediaPlayer == null ? (short)0 :
                      _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevel(4);
            }
            set
            {
                if (_currentState != null && _currentState.MediaPlayer != null)
                {
                    _currentState.MediaPlayer.AudioEqualizer.SetBandLevel(4, value);
                }
            }
        }

        public string EqualizerBandFrequencyRangeText0
        {
            get
            {
                return $"{EqualizerBandFrequencyRangeMin0} to {EqualizerBandFrequencyRangeMax0}";
            }
        }

        public string EqualizerBandFrequencyRangeText1
        {
            get
            {
                return $"{EqualizerBandFrequencyRangeMin1} to {EqualizerBandFrequencyRangeMax1}";
            }
        }

        public string EqualizerBandFrequencyRangeText2
        {
            get
            {
                return $"{EqualizerBandFrequencyRangeMin2} to {EqualizerBandFrequencyRangeMax2}";
            }
        }

        public string EqualizerBandFrequencyRangeText3
        {
            get
            {
                return $"{EqualizerBandFrequencyRangeMin3} to {EqualizerBandFrequencyRangeMax3}";
            }
        }

        public string EqualizerBandFrequencyRangeText4
        {
            get
            {
                return $"{EqualizerBandFrequencyRangeMin4} to {EqualizerBandFrequencyRangeMax4}";
            }
        }

        public int EqualizerBandFrequencyRangeMin0
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(0);
                return value == null ? 0 : value[0];
            }

        }
        public int EqualizerBandFrequencyRangeMax0
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(0);
                return value == null ? 0 : value[0];
            }
        }

        public int EqualizerBandFrequencyRangeMin1
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(1);
                return value == null ? 0 : value[0];
            }

        }
        public int EqualizerBandFrequencyRangeMax1
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(1);
                return value == null ? 0 : value[1];
            }
        }

        public int EqualizerBandFrequencyRangeMin2
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(2);
                return value == null ? 0 : value[0];
            }

        }
        public int EqualizerBandFrequencyRangeMax2
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(2);
                return value == null ? 0 : value[1];
            }
        }

        public int EqualizerBandFrequencyRangeMin3
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(3);
                return value == null ? 0 : value[0];
            }

        }
        public int EqualizerBandFrequencyRangeMax3
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(3);
                return value == null ? 0 : value[1];
            }
        }

        public int EqualizerBandFrequencyRangeMin4
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(4);
                return value == null ? 0 : value[0];
            }

        }
        public int EqualizerBandFrequencyRangeMax4
        {
            get
            {
                if (_currentState == null || _currentState.MediaPlayer == null) return 0;
                var value = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(4);
                return value == null ? 0 : value[1];
            }
        }
    }
}
