﻿using Android.Content;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Exceptions;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Services;
using CFMediaPlayer.Utilities;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using System.Windows.Input;

namespace CFMediaPlayer.ViewModels
{
    public class CurrentPageModel : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler? PropertyChanged;

        public LocalizationResources LocalizationResources => LocalizationResources.Instance;

        public void OnPropertyChanged([CallerMemberName] string name = "") =>
                     PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));

        public delegate void MediaPlayerError(MediaPlayerException mediaPlayerException);
        public event MediaPlayerError? OnMediaPlayerError;

        public delegate void DebugAction(string debug);
        public event DebugAction? OnDebugAction;

        private System.Timers.Timer _elapsedTimer;

        private readonly IAudioSettingsService _audioSettingsService;
        private ICurrentState _currentState;
        private IMediaPlayer _mediaPlayer;
        private readonly IMediaSourceService _mediaSourceService;
        private readonly IUIThemeService _uiThemeService;
        private readonly IUserSettingsService _userSettingsService;
        
        private List<MediaAction> _mediaActions = new List<MediaAction>();

        private UITheme _uiTheme;
        private AudioSettings _audioSettings;        
        private bool _isSearchBusy = false;

        public CurrentPageModel(IAudioSettingsService audioSettingsService,
                                ICurrentState currentState,
                                IMediaPlayer mediaPlayer,
                                IMediaSourceService mediaSourceService,
                                IUIThemeService uiThemeService,
                                IUserSettingsService userSettingsService)
        {
            _audioSettingsService = audioSettingsService;
            _currentState = currentState;
            _mediaPlayer = mediaPlayer;
            _mediaSourceService = mediaSourceService;
            _uiThemeService = uiThemeService;
            _userSettingsService = userSettingsService;
            
            _currentState.MediaPlayer = mediaPlayer;

            // Set handler for selected media item changed
            _currentState.RegisterSelectedMediaItemChanged(() => SelectedMediaItem = _currentState.SelectedMediaItem);

            // Set action when user settings updated
            _currentState.UserSettingsUpdatedAction += () =>
            {
                var oldPreset = _audioSettings.PresetName;

                LoadUserSettings();

                // Apply audio preset changes
                if (oldPreset != _audioSettings.PresetName)
                {                    
                    _mediaPlayer.AudioEqualizer.EqualizerPresetName = _audioSettings.PresetName;
                    _mediaPlayer.AudioEqualizer.ApplyPreset();
                }
            };

            // Handle media play status changes
            _mediaPlayer.SetStatusAction(OnMediaItemStatusChange);            

            // Set timer for elapsed play time
            _elapsedTimer = new System.Timers.Timer();
            _elapsedTimer.Elapsed += _timer_Elapsed;
            _elapsedTimer.Interval = 1000;
            _elapsedTimer.Enabled = false;

            // Set commands
            NextCommand = new Command(DoNext);
            PrevCommand = new Command(DoPrev);
            PlayOrNotCommand = new Command(DoPlayOrNot);
            //RestartCommand = new Command(DoRestart);
            StopCommand = new Command(DoStop);

            // Set defaults
            ShufflePlay = false;
            AutoPlayNext = false;

            // Load user settings
            LoadUserSettings();

            // Set equalizer preset
            _mediaPlayer.AudioEqualizer.EqualizerPresetName = _audioSettings.PresetName;
        }

        private void LoadUserSettings()
        {
            // Get user settings (Theme, audio settings)
            var userSettings = _userSettingsService.GetByUsername(Environment.UserName);
            _uiTheme = _uiThemeService.GetAll().First(t => t.Id == userSettings.UIThemeId);
            _audioSettings = _audioSettingsService.GetById(userSettings.AudioSettingsId)!;
        }

        //public bool IsShouldBeVisible => _currentState.SelectedMediaItem != null;

        public bool IsDebugMode => false;

        private Action<double> _elapsedAction;
        public void SetElapsedAction(Action<double> action)
        {
            _elapsedAction = action;
        }

        public string? MediaItemName => _selectedMediaItem != null ? _selectedMediaItem.Name : null;

        /// <summary>
        /// Whether component is busy. This is typically used for the ActivityIndicator which is used for any
        /// busy functions, not just search.
        /// </summary>
        public bool IsBusy => false;

        public List<MediaAction> MediaActions
        {
            get { return _mediaActions; }
            set
            {
                _mediaActions = value;

                OnPropertyChanged(nameof(MediaActions));
            }
        }
    
        /// <summary>
        /// Whether to shuffle media items
        /// </summary>
        public bool ShufflePlay
        {
            get { return _currentState.ShufflePlay; }
            set
            {
                _currentState.ShufflePlay = value;

                OnPropertyChanged(nameof(ShufflePlay));
            }
        }

        /// <summary>
        /// Whether to auto-play next media item when current media item completed
        /// </summary>
        public bool AutoPlayNext
        {
            get { return _currentState.AutoPlayNext; }

            set
            {
                _currentState.AutoPlayNext = value;

                OnPropertyChanged(nameof(AutoPlayNext));
            }
        }

        /// <summary>
        /// Command to play previous media item
        /// </summary>
        public ICommand PrevCommand { get; set; }

        /// <summary>
        /// Command to play next media item
        /// </summary>
        public ICommand NextCommand { get; set; }

        /// <summary>
        /// Command to restart current media item
        /// </summary>
        public ICommand RestartCommand { get; set; }

        /// <summary>
        /// Command to play or pause/stop current media item
        /// </summary>
        public ICommand PlayOrNotCommand { get; set; }

        /// <summary>
        /// Command to stop playing or pausing current media item
        /// </summary>
        public ICommand StopCommand { get; set; }

        //public ICommand ExecuteMediaItemActionCommand { get; set; }    
  
        /// <summary>
        /// Notifies property changes for media item play state.
        /// </summary>
        private void NotifyPropertiesChangedForPlayState()
        {
            OnPropertyChanged(nameof(IsCanSelectPosition));
            OnPropertyChanged(nameof(IsPlaying));
            OnPropertyChanged(nameof(IsPaused));
            OnPropertyChanged(nameof(IsNextEnabled));
            OnPropertyChanged(nameof(IsNextVisible));
            OnPropertyChanged(nameof(IsPrevEnabled));
            OnPropertyChanged(nameof(IsPrevVisible));
            OnPropertyChanged(nameof(IsAutoPlayNextVisible));
            OnPropertyChanged(nameof(IsShufflePlayVisible));
            OnPropertyChanged(nameof(PlayButtonImageSource));
            OnPropertyChanged(nameof(DurationMS));
            OnPropertyChanged(nameof(ElapsedTime));
            OnPropertyChanged(nameof(ElapsedMS));
            OnPropertyChanged(nameof(RemainingTime));
            OnPropertyChanged(nameof(RemainingMS));
        }
      
        private void ClearActionsForMediaItem()
        {
            MediaActions = new List<MediaAction>();
            //SelectedMediaItemAction = null;
        }
  
        /// <summary>
        /// Loads actions for media item
        /// </summary>
        /// <param name="mediaItem"></param>
        private void LoadMediaActionsForMediaItem(MediaItem mediaItem, int maxItems = 10)
        {
            // Clear
            MediaActions = new List<MediaAction>();

            // Load actions
            var mediaActions = new List<MediaAction>();
            if (mediaItem.EntityCategory == EntityCategory.Real)
            {
                mediaActions.AddRange(_currentState.SelectedMediaSource!.GetMediaActionsForMediaItem(_currentState.SelectedMediaLocation, mediaItem));
            }

            // Limit number of actions. If we have lots of playlists then there will be lots of actions for playlists.            
            while (mediaActions.Count > maxItems)
            {
                var playlistAction = mediaActions.FirstOrDefault(mia => mia.ActionType == Enums.MediaActionTypes.AddToPlaylist);
                if (playlistAction == null)
                {
                    break;
                }
                else
                {
                    mediaActions.Remove(playlistAction);
                }
            }
            while (mediaActions.Count > maxItems)
            {
                mediaActions.RemoveAt(0);
            }

            // Consistent order
            mediaActions = mediaActions.OrderBy(mia => mia.Name).ToList();

            MediaActions = mediaActions;
        }

        private MediaItem? _selectedMediaItem;

        /// <summary>
        /// Selected media item.
        /// 
        /// If media item is set then we load list of actions for media item. E.g. Add to playlist X, remove from playlist Y etc
        /// </summary>
        public MediaItem? SelectedMediaItem
        {
            get
            {
                return _selectedMediaItem;
            }

            set
            {
                _selectedMediaItem = value;

                // Notify properties on change of selected media item
                OnPropertyChanged(nameof(SelectedMediaItem));
                OnPropertyChanged(nameof(IsRealMediaItemSelected));
                OnPropertyChanged(nameof(MainLogoImage));   // If media item specific image
                OnPropertyChanged(nameof(MediaItemName));

                // Stop current media
                var isWasPlaying = IsPlaying;
                if (isWasPlaying || IsPaused)
                {
                    Stop();
                }

                if (_selectedMediaItem != null)
                {
                    // Set actions for media item (Add to playlist X etc)
                    LoadMediaActionsForMediaItem(_selectedMediaItem);

                    // Respect previous play state. Only play if was playing. Don't play if was paused or stopped.
                    if (IsRealMediaItemSelected && isWasPlaying)
                    {
                        PlayMediaItem(_selectedMediaItem);
                    }
                }

                // Notify play state changed. Can now play media item
                NotifyPropertiesChangedForPlayState();
            }
        }

        /// <summary>
        /// Main logo image to display   
        /// </summary>
        public string MainLogoImage
        {
            get
            {
                // Set media item level image. E.g. Radio stream, playlist item with image for album.
                // If storage then do nothing because we want to display the album image.
                if (_currentState.SelectedMediaLocation != null && _currentState.SelectedMediaLocation.MediaSourceType != MediaSourceTypes.Storage)
                {
                    if (_selectedMediaItem != null && !String.IsNullOrEmpty(_selectedMediaItem.ImagePath))
                    {
                        return _selectedMediaItem.ImagePath;
                    }
                }

                // Set media item collection level image. E.g. Album image
                if (_currentState.SelectedMediaItemCollection != null && !String.IsNullOrEmpty(_currentState.SelectedMediaItemCollection.ImagePath))
                {
                    return _currentState.SelectedMediaItemCollection.ImagePath;
                }

                // Set media source level image
                if (_currentState.SelectedMediaSource != null) return _currentState.SelectedMediaSource.ImagePath;

                return "cassette_player_audio_speaker_sound_icon.png";  // Default
            }
        }

        /// <summary>
        /// Restarts media item
        /// </summary>
        /// <param name="parameter"></param>
        private void DoRestart(object parameter)
        {
            if (_currentState.SelectMediaItemAction != null)
            {
                if (IsPlaying || IsPaused)    // Just move to start
                {
                    ElapsedMS = 0;
                }
                else   // Media item completed, play it again
                {
                    PlayMediaItem(SelectedMediaItem);
                }
            }
        }

        /// <summary>
        /// Plays next media item. If shuffle play then media items already sorted randomly
        /// </summary>
        /// <param name="parameter"></param>
        private void DoNext(object parameter)
        {                        
            if (_currentState.SelectMediaItemAction != null)
            {
                // Select next media item
                var index = _currentState.MediaItems.IndexOf(SelectedMediaItem!);

                if (_currentState.MediaItems.Count > 1)
                {                                        
                    // Select media item
                    _currentState.SelectMediaItemAction(_currentState.MediaItems[index + 1]);

                    // Play media item                
                    PlayMediaItem(SelectedMediaItem);
                }
            }
        }

        /// <summary>
        /// Plays previous media item
        /// </summary>
        /// <param name="parameter"></param>
        private void DoPrev(object parameter)
        {            
            if (_currentState.SelectMediaItemAction != null)
            {
                // Select prev media item
                var index = _currentState.MediaItems.IndexOf(SelectedMediaItem!);
                _currentState.SelectMediaItemAction(_currentState.MediaItems[index - 1]);

                // Play media item                
                PlayMediaItem(SelectedMediaItem);
            }
        }

        /// <summary>
        /// Stops playing current media item
        /// </summary>
        /// <param name="parameter"></param>
        private void DoStop(object parameter)
        {
            Stop();
        }

        /// <summary>
        /// Plays/pauses/stops current media item. Streamed media only allows play/stop and other media allows
        /// play/pause.
        /// </summary>
        /// <param name="parameter"></param>
        private void DoPlayOrNot(object parameter)
        {
            if (IsPlaying)   // Pause or stop
            {               
                if (_selectedMediaItem.IsStreamed)   // Play & stop
                {
                    Stop();
                }
                else    // Play & pause
                {
                    Pause();
                }                
            }
            else    // Play
            {                
                PlayMediaItem(SelectedMediaItem!);                                
            }
        }

        /// <summary>
        /// Plays or resumes media item
        /// </summary>
        /// <param name="mediaItem"></param>
        private void PlayMediaItem(MediaItem mediaItem)
        {
            // Stop current media item if different media item to current
            if (IsPlaying || IsPaused)
            {
                if (mediaItem.FilePath != _mediaPlayer.CurrentFilePath)
                {
                    Stop();
                }
            }

            // Play or resume
            _mediaPlayer.Play(mediaItem.FilePath, (exception) =>
            {
                // TODO: Send to UI
                System.Diagnostics.Debug.WriteLine($"Error playing audio: {exception.Message}");
                //StatusLabel.Text = exception.Message;
            });
        }
  
        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            System.Diagnostics.Debug.WriteLine($"_timer_Elapsed: ElapsedMS={ElapsedMS}");

            // Execute action for elapsed time
            if (_elapsedAction != null)
            {
                _elapsedAction(ElapsedMS);
            }

            // TODO: Consider disabling timer when app not visible

            // Notify property changes to update UI. Only need to be concerned about updating UI with progress
            // for current media item.            
            OnPropertyChanged(nameof(ElapsedTime));
            OnPropertyChanged(nameof(ElapsedMS));
            OnPropertyChanged(nameof(RemainingTime));
            OnPropertyChanged(nameof(RemainingMS));
        }        
  
        /// <summary>
        /// Elapsed time for media item (00:00:00)
        /// </summary>
        public string ElapsedTime
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return GetDurationString(_mediaPlayer.GetElapsedPlayTime());
                }
                return GetDurationString(TimeSpan.Zero);    // "00:00:00"
            }
        }

        /// <summary>
        /// Remaining time for media item (00:00:00)
        /// </summary>
        public string RemainingTime
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return GetDurationString(_mediaPlayer.GetTotalDuration() - _mediaPlayer.GetElapsedPlayTime());
                }
                return GetDurationString(TimeSpan.Zero);
            }
        }

        private static string GetDurationString(TimeSpan duration)
        {
            return string.Format("{0:00}", duration.Hours) + ":" +
                          string.Format("{0:00}", duration.Minutes) + ":" +
                          string.Format("{0:00}", duration.Seconds);
        }

        /// <summary>
        /// Remaining time for media item (MS)
        /// </summary>
        public System.Double RemainingMS
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return (int)(_mediaPlayer.GetTotalDuration().TotalMilliseconds - _mediaPlayer.GetElapsedPlayTime().TotalMilliseconds);
                }
                return 0;
            }
        }

        /// <summary>
        /// Duration of current media item (MS)
        /// </summary>
        public System.Double DurationMS
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return (int)_mediaPlayer.GetTotalDuration().TotalMilliseconds;
                }
                return 1000;    // Any non-zero value
            }
        }

        /// <summary>
        /// Elapsed time of current media item
        /// </summary>
        public System.Double ElapsedMS
        {
            get
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return (int)_mediaPlayer.GetElapsedPlayTime().TotalMilliseconds;
                }
                return 0;
            }

            set
            {
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    _mediaPlayer.SetElapsedPlayTime(TimeSpan.FromMilliseconds(value));
                }
            }
        }

        /// <summary>
        /// Whether media item is playing
        /// </summary>
        public bool IsPlaying
        {
            get { return _mediaPlayer.IsPlaying; }
        }

        /// <summary>
        /// Whether media item is paused
        /// </summary>
        public bool IsPaused
        {
            get { return _mediaPlayer.IsPaused; }
        }

        /// <summary>
        /// Pauses media item
        /// </summary>
        public void Pause()
        {
            _mediaPlayer.Pause();
            _elapsedTimer.Enabled = false;
        }

        /// <summary>
        /// Stops media item
        /// </summary>
        public void Stop()
        {
            _mediaPlayer.Stop();
            _elapsedTimer.Enabled = false;
        }

        /// <summary>
        /// Image source for the play button (Play/Pause/Stop)
        /// </summary>
        public string PlayButtonImageSource
        {
            get
            {                
                if (IsPlaying)
                {                   
                    if (_selectedMediaItem.IsStreamed)    // Streamed
                    {
                        return "audio_media_media_player_music_stop_icon.png";
                    }
                    else    // Pause
                    {
                        return "audio_media_media_player_music_pause_icon.png";
                    }                    
                }
                return "audio_media_media_player_music_play_icon.png";
            }
        }

        /// <summary>
        /// Handles media player status change
        /// </summary>
        /// <param name="status"></param>
        private void OnMediaItemStatusChange(MediaPlayerStatuses status, MediaPlayerException? exception)
        {
            System.Diagnostics.Debug.WriteLine($"OnMediaItemStatusChange: {status}");

            switch (status)
            {
                case MediaPlayerStatuses.Completed:
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = false;

                    // Play next if configured, otherwise just leave selected media item
                    if (_currentState.AutoPlayNext &&
                        SelectedMediaItem != _currentState.MediaItems.Last())
                    {                        
                        NextCommand.Execute(null);
                    }                  
                    break;
                case MediaPlayerStatuses.Paused:
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = false;
                    break;
                case MediaPlayerStatuses.Playing:
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = true;
                    break;
                case MediaPlayerStatuses.Stopped:
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = false;
                    break;
                case MediaPlayerStatuses.PlayError:
                    NotifyPropertiesChangedForPlayState();

                    _elapsedTimer.Enabled = false;

                    // Notify error
                    if (OnMediaPlayerError != null && exception != null)
                    {
                        OnMediaPlayerError(exception);
                    }
                    break;
            }

            if (OnDebugAction != null) OnDebugAction($"Status={status}");
        }    

        /// <summary>
        /// Whether "Previous media item" is enabled
        /// </summary>
        public bool IsPrevEnabled
        {
            get
            {
                if (IsRealMediaItemSelected)
                {
                    var index = _currentState.MediaItems.IndexOf(SelectedMediaItem);
                    return index > 0;
                }
                return false;
            }
        }

        /// <summary>
        /// Whether "Previous media item" is visible
        /// </summary>
        public bool IsPrevVisible
        {
            get
            {
                if (_selectedMediaItem != null) return !_selectedMediaItem.IsStreamed;  //   _selectedMediaItem.IsAllowPrev;
                return true;
            }
        }

        /// <summary>
        /// Whether "Next media item" is enabled
        /// </summary>
        public bool IsNextEnabled
        {
            get
            {
                if (IsRealMediaItemSelected)
                {
                    var index = _currentState.MediaItems.IndexOf(SelectedMediaItem);
                    return index < _currentState.MediaItems.Count - 1;
                }
                return false;
            }
        }

        /// <summary>
        /// Whether "Next media item" is visible
        /// </summary>
        public bool IsNextVisible
        {
            get
            {
                if (_selectedMediaItem != null) return !_selectedMediaItem.IsStreamed;  // _selectedMediaItem.IsAllowNext;
                return true;
            }
        }

        /// <summary>
        /// Whether user can select position in media item
        /// </summary>
        public bool IsCanSelectPosition
        {
            get
            {
                if (_selectedMediaItem != null) return !_selectedMediaItem.IsStreamed;  //  _ selectedMediaItem.IsCanSelectPosition;
                return true;
            }
        }

        /// <summary>
        /// Whether a real media item is selected rather than [None], [Multiple], [Shuffle] etc
        /// </summary>
        public bool IsRealMediaItemSelected
        {
            get
            {
                return SelectedMediaItem != null &&
                    SelectedMediaItem.EntityCategory == EntityCategory.Real;
            }
        }

        ///// <summary>
        ///// Selected playlist action for media item
        ///// </summary>
        //private MediaItemAction? _selectedMediaItemAction;
        //public MediaItemAction? SelectedMediaItemAction
        //{
        //    get { return _selectedMediaItemAction;  }
        //    set
        //    {
        //        _selectedMediaItemAction = value;         

        //        OnPropertyChanged(nameof(SelectedMediaItemAction));

        //        //if (_selectedMediaItemAction != null && !_selectedMediaItemAction.Name.Equals("Select an action..."))
        //        //{
        //        //    ExecuteMediaItemActionCommand.Execute(null);
        //        //}
        //    }
        //}

        /// <summary>
        /// Executes media item action
        /// </summary>
        /// <param name="parmediaActionameter"></param>
        public void ExecuteMediaAction(MediaAction mediaAction)
        {
            if (mediaAction != null)
            {
                // Get media source to execute action against. We may be displaying the storage source but need to execute
                // 'Add to playlist X' against the playlist source.
                var mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == mediaAction.MediaLocationName);

                // Execute action. E.g. Add to playlist X, remove from playlist Y etc
                mediaSource.ExecuteMediaAction(mediaAction);

                // Refresh media actions. E.g. If we just added media item to playlist X then we now have an action to remove it
                // from the playlist
                LoadMediaActionsForMediaItem(_selectedMediaItem);
            }
        }     
   
        public void ApplyEqualizerTest()
        {
            var url = "https://stream.rcs.revma.com/muddaykbyk3vv";
            _mediaPlayer.Play(url, (exception) =>
            {
                int xxx = 1000;
            });

            //_mediaPlayer.ApplyEqualizerTest();
            //int xxx = 1000;
        }

        ///// <summary>
        ///// Handles user settings updated
        ///// </summary>
        //public void HandleUserSettingsUpdated()
        //{
        //    var userSettings = _userSettingsService.GetByUsername(Environment.UserName)!;

        //    // Handle them change
        //    var isThemeChanged = _uiTheme.Id != userSettings.UIThemeId;
        //    if (isThemeChanged)
        //    {
        //        _uiTheme = _uiThemeService.GetAll().First(t => t.Id == userSettings.UIThemeId);
        //    }

        //    // Handle audio settings
        //    var isAudioSettingsChanged = _audioSettings.Id != userSettings.AudioSettingsId;
        //    if (isAudioSettingsChanged)
        //    {
        //        _audioSettings = _audioSettingsService.GetById(userSettings.AudioSettingsId)!;
        //        _mediaPlayer.EqualizerPresetName = _audioSettings.PresetName;
        //    }
        //}

        ///// <summary>
        ///// Handles playlists updated. Playlist changes may playlist lists displayed, media item actions for adding/
        ///// removing media items from playlists.
        ///// </summary>
        //public void HandlePlaylistsUpdated()
        //{
        //    //Reset(SelectedMediaLocation.Name,
        //    //            SelectedArtist!.Name,
        //    //            SelectedMediaItemCollection!.Name,
        //    //            SelectedMediaItem!.Name);
        //}

        ///// <summary>
        ///// Handles queue updated. E.g. Cleared.
        ///// </summary>
        //public void HandleQueueUpdated()
        //{
        //    //Reset(SelectedMediaLocation.Name,
        //    //            SelectedArtist!.Name,
        //    //            SelectedMediaItemCollection!.Name,
        //    //            SelectedMediaItem!.Name);
        //}

        ///// <summary>
        ///// Resets UI state. Selects requested items, if not available then selects default
        ///// </summary>
        ///// <param name="selectedMediaLocationName"></param>
        ///// <param name="selectedArtistName"></param>
        ///// <param name="selectedMediaItemCollectionName"></param>
        ///// <param name="selectedMediaItemName"></param>
        ///// <param name="selectedMediaItemActionName"></param>
        //private void Reset(string selectedMediaLocationName,
        //                    string selectedArtistName,
        //                    string selectedMediaItemCollectionName,
        //                    string selectedMediaItemName)
        //{
        //    // Clear selected media item location
        //    SelectedMediaLocation = null;
        //    LoadMediaLocationsToDisplayInUI();

        //    // Set media location. When the property is set then we load the child items and set a default selected child
        //    // item and we repeat this until the lowest level (Media item actions)
        //    SelectedMediaLocation = MediaLocations.First(ml => ml.Name == selectedMediaLocationName);

        //    // Set selected artist
        //    if (!String.IsNullOrEmpty(selectedArtistName)) ;
        //    {
        //        SelectedArtist = Artists.FirstOrDefault(a => a.Name == selectedArtistName);
        //    }
        //    if (SelectedArtist == null)
        //    {
        //        SelectedArtist = Artists.First();
        //    }

        //    // Set selected media item collection
        //    if (!String.IsNullOrEmpty(selectedMediaItemCollectionName))
        //    {
        //        SelectedMediaItemCollection = MediaItemCollections.FirstOrDefault(mic => mic.Name == selectedMediaItemCollectionName);
        //    }
        //    if (SelectedMediaItemCollection == null)
        //    {
        //        SelectedMediaItemCollection = MediaItemCollections.First();
        //    }

        //    // Set selected media item
        //    if (!String.IsNullOrEmpty(selectedMediaItemName))
        //    {
        //        SelectedMediaItem = MediaItems.FirstOrDefault(mi => mi.Name == selectedMediaItemName);
        //    }
        //    if (SelectedMediaItem == null)
        //    {
        //        SelectedMediaItem = MediaItems.First();
        //    }

        //    //// Selected selected media item action
        //    //if (!String.IsNullOrEmpty(selectedMediaItemActionName))
        //    //{
        //    //    SelectedMediaItemAction = MediaItemActions.FirstOrDefault(mia => mia.Name == selectedMediaItemActionName);
        //    //}
        //    //if (SelectedMediaItemAction == null)
        //    //{
        //    //    SelectedMediaItemAction = MediaItemActions.First();
        //    //}
        //}

        ///// <summary>
        ///// Clear search results
        ///// </summary>
        //public void ClearSearchResults()
        //{
        //    SearchResults = new List<SearchResult>();
        //}

        ////public string SearchBarPlaceholderText => SelectedMediaLocation == null ? "" :
        ////                                String.Format(LocalizationResources.Instance["SearchWithParam"].ToString(), SelectedMediaLocation.Name);
        //public string SearchBarPlaceholderText => LocalizationResources.Instance["Search"].ToString();

        /// <summary>
        /// Whether the Shuffle Play switch is visible
        /// </summary>
        public bool IsShufflePlayVisible
        {
            get
            {
                return _currentState.SelectedMediaSource.IsShufflePlayAllowed;
            }
        }

        /// <summary>
        /// Whether Auto-Play Next switch is visible
        /// </summary>
        public bool IsAutoPlayNextVisible
        {
            get
            {
                return _currentState.SelectedMediaSource.IsAutoPlayNextAllowed;

            }
        }
    }
}
