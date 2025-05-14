using __XamlGeneratedCode__;
using Android.Content;
using Android.Text;
using CFMediaPlayer.Constants;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace CFMediaPlayer.ViewModels
{
    /// <summary>
    /// View model for User Settings page.
    /// </summary>
    public class UserSettingsPageModel : PageModelBase, INotifyPropertyChanged
    {
        //public event PropertyChangedEventHandler? PropertyChanged;

        //public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        //public void OnPropertyChanged([CallerMemberName] string name = "") =>
        //             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        private readonly IAudioEqualizer _audioEqualizer;
        private readonly IAudioSettingsService _audioSettingsService;
        private readonly ICurrentState _currentState;
        private readonly ILogWriter _logWriter;
        private readonly IUIThemeService _uiThemeService;
        private readonly ISystemSettingsService _systemSettingsService;
        private readonly IUserSettingsService _userSettingsService;

        private List<AudioBand> _audioBands = new List<AudioBand>();
        private UserSettings _userSettings;
        private List<UITheme> _uiThemes = new List<UITheme>(); 
        private List<NameValuePair<string>> _audioSettingsList = new List<NameValuePair<string>>();

        public UserSettingsPageModel(IAudioEqualizer audioEqualizer,
                                IAudioSettingsService audioSettingsService,                
                                ICurrentState currentState,
                                ILogWriter logWriter,
                                ISystemSettingsService systemSettingsService,
                                IUIThemeService uiThemeService,
                                IUserSettingsService userSettingsService)
        {
            InternalUtilities.Log("Entered UserSettingsPageModel constructor");

            _audioEqualizer = audioEqualizer;
            _audioSettingsService = audioSettingsService;
            _currentState = currentState;
            _logWriter = logWriter;
            _systemSettingsService = systemSettingsService;
            _uiThemeService = uiThemeService;
            _userSettingsService = userSettingsService;            

            // Set commands
            SaveCommand = new Command(Save);
            CancelCommand = new Command(Cancel);
            CopyPresetToCustomCommand = new Command(CopyPresetToCustom);
            //RefreshCommand = new Command(Refresh);
            //ResetAudioDefaultsCommand = new Command(ResetAudioDefaults);
            //TestAudioSettingsCommand = new Command(TestAudioSettings);           

            LoadAllSettings();

            InternalUtilities.Log("Leaving UserSettingsPageModel constructor");
        }

        private List<NameValuePair<string>> _languages = new List<NameValuePair<string>>();

        public List<NameValuePair<string>> Languages
        {
            get { return _languages; }
            set
            {
                _languages = value;

                OnPropertyChanged(nameof(Languages));
            }
        }

        private NameValuePair<string> _selectedLanguage;
        public NameValuePair<string> SelectedLanguage
        {
            get { return _selectedLanguage; }
            set
            {
                _selectedLanguage = value;

                OnPropertyChanged(nameof(SelectedLanguage));
            }
        }

        //public bool IsDirty
        //{
        //    get
        //    {
        //        var userSettings = _userSettingsService.GetByUsername(Environment.UserName);
        //        return XmlUtilities.SerializeToString(_userSettingsService) !=
        //                XmlUtilities.SerializeToString(userSettings);
        //    }
        //}

        /// <summary>
        /// Loads all settings
        /// </summary>
        private void LoadAllSettings()
        {
            // Load languages
            var languages = new List<NameValuePair<string>>()
            {
                new NameValuePair<string>() { Name = "English", Value = "en" },
                new NameValuePair<string>() { Name = "French", Value = "fr" }                
            };
            Languages = languages;

            // Set current language
            SelectedLanguage = Languages.First(l => l.Value == "en");

            // Get current user settings
            _userSettings = _userSettingsService.GetByUsername(Environment.UserName)!;

            // Set current settings
            UIThemes = _uiThemeService.GetAll();
            SelectedUITheme = UIThemes.First(t => t.Id == _userSettings.UIThemeId);

            // Load audio settings            
            LoadAudioSettings();

            // Load custom audio settings
            CustomAudioSettingsList = _userSettings.CustomAudioSettingsList;

            // Set current audio settings
            SelectedAudioSettings = AudioSettingsList.First(s => s.Value == _userSettings.AudioSettingsId);

            // Set custom audio settings to edit. If using custom audio settings then select that one
            SelectedCustomAudioSettings = IsCustomAudioSettingsSelected ?
                            CustomAudioSettingsList.First(s => s.Id == SelectedAudioSettings.Value) :
                            CustomAudioSettingsList.First();
        }

        /// <summary>
        /// Loads audio settings, both presets and custom settings
        /// </summary>
        private void LoadAudioSettings()
        {
            var audioSettingsList = new List<NameValuePair<string>>();

            // Add standard audio settings
            audioSettingsList.AddRange(_audioSettingsService.GetAll()
                        .Select(s => new NameValuePair<string>() { Name = s.Name, Value = s.Id }));

            // Add custom audio settings
            audioSettingsList.AddRange(_userSettings.CustomAudioSettingsList
                        .Select(s => new NameValuePair<string>() { Name = s.Name, Value = s.Id }));

            // Sort alphabetic order
            audioSettingsList = audioSettingsList.OrderBy(s => s.Name).ToList();

            AudioSettingsList = audioSettingsList;
        }                

        /// <summary>
        /// Audio settings list. Presets and custom.
        /// </summary>
        public List<NameValuePair<string>> AudioSettingsList
        {
            get { return _audioSettingsList; }
            set
            {
                _audioSettingsList = value;

                OnPropertyChanged(nameof(AudioSettingsList));
            }
        }

        /// <summary>
        /// Selected audio settings. Either preset or custom settings
        /// </summary>
        private NameValuePair<string>? _selectedAudioSettings;
        public NameValuePair<string>? SelectedAudioSettings
        {
            get { return _selectedAudioSettings; }
            set
            {
                _selectedAudioSettings = value;

                OnPropertyChanged(nameof(SelectedAudioSettings));
                OnPropertyChanged(nameof(IsCustomAudioSettingsSelected));
                OnPropertyChanged(nameof(IsNotCustomAudioSettingsSelected));

                if (_selectedAudioSettings != null)
                {
                    _userSettings.AudioSettingsId = _selectedAudioSettings.Value;
                }                
            }
        }

        /// <summary>
        /// Custom audio settings list
        /// </summary>
        private List<CustomAudioSettings> _customAudioSettingsList = new List<CustomAudioSettings>();
        public List<CustomAudioSettings> CustomAudioSettingsList
        {
            get { return _customAudioSettingsList; }
            set
            {
                _customAudioSettingsList = value;

                OnPropertyChanged(nameof(CustomAudioSettingsList));
            }
        }

        /// <summary>
        /// Selected custom audio settings
        /// </summary>
        private CustomAudioSettings? _selectedCustomAudioSettings;
        public CustomAudioSettings? SelectedCustomAudioSettings
        {
            get { return _selectedCustomAudioSettings; }
            set
            {
                _selectedCustomAudioSettings = value;

                OnPropertyChanged(nameof(SelectedCustomAudioSettings));                

                // Display audio bands
                if (_selectedCustomAudioSettings == null)
                {
                    CustomAudioBands = new List<AudioBand>();
                }
                else
                {
                    LoadCustomAudioBands(SelectedCustomAudioSettings);
                }
            }
        }

        /// <summary>
        /// Custom audio bands. Empty list if preset selected
        /// </summary>
        private List<AudioBand> _customAudioBands = new List<AudioBand>();
        public List<AudioBand> CustomAudioBands
        {
            get { return _customAudioBands; }
            set
            {
                _customAudioBands = value;

                OnPropertyChanged(nameof(CustomAudioBands));
            }
        }

        /// <summary>
        /// Loads custom audio bands. It sets a delegate to apply any updated level back to CustomAdio
        /// </summary>
        /// <param name="customAudioSettings"></param>
        private void LoadCustomAudioBands(CustomAudioSettings customAudioSettings)
        {
            // Get band level ranges
            var bandLevelRange = _audioEqualizer.GetEqualizerBandLevelRange();

            // Get band frequency ranges
            var bandFrequencyRanges = _audioEqualizer.GetEqualizerBandFrequencyRanges();

            // Refresh custom audio bands
            var customAudioBands = new List<AudioBand>();
            for (short band = 0; band < customAudioSettings.AudioBands.Count; band++)
            {
                var audioBand = new AudioBand()
                {
                    Description = $"Band {band + 1} ({bandFrequencyRanges[band][0]} to {bandFrequencyRanges[band][1]})",
                    Index = band,
                    Level = customAudioSettings.AudioBands[band],
                    LevelRangeMin = bandLevelRange[0],
                    LevelRangeMax = bandLevelRange[1],                    
                    SetLevelAction = (myBand, level) =>
                    {                        
                        // Copy level to model
                        customAudioSettings.AudioBands[myBand] = level;                        
                    }
                };
                customAudioBands.Add(audioBand);
            }

            CustomAudioBands = customAudioBands;
        }

        /// <summary>
        /// Command to save changes and notify other components
        /// </summary>
        public ICommand SaveCommand { get; set; }

        /// <summary>
        /// Command to cancel unsaved changes and revert back
        /// </summary>
        public ICommand CancelCommand { get; set; }

        //public ICommand RefreshCommand { get; set; }
        
        //public ICommand ResetAudioDefaultsCommand { get; set; }

        /// <summary>
        /// Command to copy selected preset settings (Band levels) to custom preset
        /// </summary>
        public ICommand CopyPresetToCustomCommand { get; set; }

        //public ICommand TestAudioSettingsCommand { get; set; }

        //private void ResetAudioDefaults(object parameter)
        //{
        //    var systemSettings = _systemSettingsService.GetAll().First();

        //    // Set audio preset to default
        //    SelectedAudioPresetName = systemSettings.DefaultAudioPresetName;
            
        //    // Set custom audio bands to default
        //    _userSettings.CustomAudioBands = systemSettings.DefaultCustomAudioBands;

        //    // Display custom audio bands after reset to default
        //    LoadCustomAudioBands();
        //}

        private void CopyPresetToCustom(object parameter)
        {
            if (IsNotCustomAudioSettingsSelected)   // Sanity check
            {
                // Get band levels for preset
                var bandLevels = _audioEqualizer.GetBandLevelsForPreset(SelectedAudioSettings.Name);

                // Copy band levels to custom audio bands
                for (short band = 0; band < bandLevels.Count; band++)
                {
                    CustomAudioBands.First(b => b.Index == band).Level = bandLevels[band];
                }

                // Refresh CustomAudioBands
                var customBans = CustomAudioBands;
                CustomAudioBands = new();
                CustomAudioBands = customBans;                
            }
        }

        /// <summary>
        /// Saves user settings. Settings are automatically applied to _userSettings via UI.
        /// </summary>
        /// <param name="parameter"></param>
        private void Save(object parameter)
        {            
            // Save user settings
            _userSettingsService.Update(_userSettings);

            // Notify user settings changed
            _currentState.Events.RaiseOnUserSettingsUpdated(_userSettings);
        }      

        private void Cancel(object parameter)
        {
            LoadAllSettings();

            //Shell.Current.GoToAsync($"//{nameof(MainPage)}");
        }
   
        /// <summary>
        /// Whether custom audio settings are selected
        /// </summary>
        public bool IsCustomAudioSettingsSelected
        {
            get { return _selectedAudioSettings != null && 
                    _userSettings.CustomAudioSettingsList.Any(s => s.Id == _selectedAudioSettings.Value); }
        }

        /// <summary>
        /// Whether the custom preset is not selected
        /// </summary>
        public bool IsNotCustomAudioSettingsSelected
        {
            get { return !IsCustomAudioSettingsSelected; }            
        }

        //private void TestAudioSettings(object parameter)
        //{
        //    _currentState.MediaPlayer.AudioEqualizer.DefaultPresetName = "Jazz";
        //    _currentState.MediaPlayer.AudioEqualizer.ApplyDefaultPreset();

        //    // Display audio bands
        //    ListAudioBands();

        //    // Change audio bands
        //    _currentState.MediaPlayer.AudioEqualizer.Equalizer.SetBandLevel(2, 505);

        //    // Returns 500. Jazz was -200 and so it seems to have rounded from 505 to 500
        //    var bandLevel = _currentState.MediaPlayer.AudioEqualizer.Equalizer.GetBandLevel(2);

        //    // Returns -1
        //    short preset = _currentState.MediaPlayer.AudioEqualizer.Equalizer.CurrentPreset;

        //    ListAudioBands();

        //    _currentState.MediaPlayer.AudioEqualizer.DefaultPresetName = "Classical";
        //    _currentState.MediaPlayer.AudioEqualizer.ApplyDefaultPreset();

        //    ListAudioBands();

        //    int xxx = 1000;
        //}

        //private void ListAudioBands()
        //{
        //    System.Diagnostics.Debug.WriteLine($"Preset={_currentState.MediaPlayer.AudioEqualizer.Equalizer.GetPresetName(_currentState.MediaPlayer.AudioEqualizer.Equalizer.CurrentPreset)}");

        //    for (short band = 0; band < _currentState.MediaPlayer.AudioEqualizer.EqualizerBands; band++)
        //    {
        //        var frequency = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandFrequencyRange(band);
        //        var audioBand = new AudioBand()
        //        {
        //            Index = band,
        //            Description = $"Band {band + 1} ({frequency[0]} to {frequency[1]})",    // TODO: Language resource
        //            LevelRangeMin = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevelRange()[0],
        //            LevelRangeMax = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevelRange()[1],
        //            Level = _currentState.MediaPlayer.AudioEqualizer.GetEqualizerBandLevel(band)
        //        };

        //        System.Diagnostics.Debug.WriteLine($"Band={band}, Level={audioBand.Level}, Min={audioBand.LevelRangeMin}, Max={audioBand.LevelRangeMax}");                
        //    }
        //}

        /// <summary>
        /// UI themese
        /// </summary>
        public List<UITheme> UIThemes
        {            
            get { return _uiThemes; }
            set
            {
                _uiThemes = value;

                OnPropertyChanged(nameof(UIThemes));
            }
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

                if (_selectedUITheme != null)
                {
                    _userSettings.UIThemeId = _selectedUITheme.Id;
                }
            }
        }
    }
}
