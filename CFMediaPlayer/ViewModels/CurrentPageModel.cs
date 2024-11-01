using Android.Content;
using Android.Media;
using CFMediaPlayer.Constants;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Exceptions;
using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Services;
using CFMediaPlayer.Utilities;
using Kotlin.Reflect;
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
    /// <summary>
    /// Notes:
    /// - If media item isn't playable (E.g. No internet connection to play stream) then user can next/previous
    ///   but won't be able to play.
    /// - Busy indicator is displayed when changing media location because it can be slow.
    /// - Busy indicator is displayed when starting media item.
    /// </summary>
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
        private bool _isBusy;

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

            // Set function to return media item player status for media item
            _currentState.GetMediaItemPlayStatusFunction = (mediaItem) =>
            {                
                if (SelectedMediaItem != null && 
                    SelectedMediaItem.FilePath == mediaItem.FilePath)   // Current media item is requested media item
                {                                        
                    if (IsPaused) return MediaPlayerStatuses.Paused;
                    if (IsPlaying || IsStarting) return MediaPlayerStatuses.Playing;
                    if (IsCompleted) return MediaPlayerStatuses.Completed;
                }
                return null;
            };

            ConfigureEvents();       

            // Set timer for elapsed play time
            _elapsedTimer = new System.Timers.Timer();
            _elapsedTimer.Elapsed += _timer_Elapsed;
            _elapsedTimer.Interval = 1000;
            _elapsedTimer.Enabled = false;

            // Set commands
            NextCommand = new Command(DoNext);
            PrevCommand = new Command(DoPrev);
            PlayToggleCommand = new Command(DoPlayToggle);
            //RestartCommand = new Command(DoRestart);
            StopCommand = new Command(DoStop);

            // Set defaults
            ShufflePlay = false;
            AutoPlayNext = false;

            // Set equalizer preset
            ApplyEqualizerSettings(_userSettingsService.GetByUsername(Environment.UserName));            
        }

        public bool IsBusy
        {
            get { return _isBusy; }
            set
            {
                _isBusy = value;

                OnPropertyChanged(nameof(IsBusy));
            }
        }
       
        private void ConfigureEvents()
        {
            // Set event handler to play media item
            _currentState.Events.OnPlayMediaItem += (mediaItem) =>
            {
                SelectAndPlayMediaItem(mediaItem);                
            };

            // Set event handler to toggle play state 
            _currentState.Events.OnTogglePlayMediaItem += (mediaItem) =>
            {
                PlayToggleCommand.Execute(null);
            };

            /*
            // Set event handler for selected media item changed         .
            _currentState.Events.OnSelectedMediaItemChanged += (mediaItem) =>
            {
                SelectedMediaItem = mediaItem;
            };
            */

            // Set event handler for user settings updated
            _currentState.Events.OnUserSettingsUpdated += (userSettings) =>
            {
                var userSettings2 = _userSettingsService.GetByUsername(Environment.UserName);

                ApplyLanguageSettings(userSettings2);
                ApplyEqualizerSettings(userSettings2);
            };

            // Set event handler for playlist updated. Only need to be concerned about updates that affect current media
            // item (E.g. Add to playlist X, remove from playlist Y etc). If playlist is cleared then it will be handled
            // by LibraryPage.
            _currentState.Events.OnPlaylistUpdated += (systemEventType, mediaItemCollection, mediaItem) =>
            {
                System.Diagnostics.Debug.WriteLine($"OnPlaylistUpdated in CurrentPageModel {systemEventType}");

                if (SelectedMediaItem != null)
                {

                    // If playlist update relates to specific media item (E.g. Add/remove from playlist) or playlist deleted then
                    // refresh media actions.                
                    switch (systemEventType)
                    {
                        case SystemEventTypes.PlaylistItemAdded:
                        case SystemEventTypes.PlaylistItemRemoved:
                            if (mediaItem != null &&
                                mediaItem.FilePath == SelectedMediaItem.FilePath)
                            {
                                LoadMediaActionsForMediaItem(SelectedMediaItem);
                            }
                            break;
                        case SystemEventTypes.PlaylistDeleted:
                            LoadMediaActionsForMediaItem(SelectedMediaItem);
                            break;
                    }
                }
            };

            // Set event handler for queue updated. Only needs to be concerned about updates that affect current media item.
            // If queue is cleared then it will be handled by LibraryPage.
            _currentState.Events.OnQueueUpdated += (systemEventType, mediaItem) =>
            {
                System.Diagnostics.Debug.WriteLine($"[in] OnQueueUpdated in CurrentPageModel {systemEventType}");

                if (SelectedMediaItem != null)
                {
                    switch (systemEventType)
                    {
                        case SystemEventTypes.QueueItemAdded:
                        case SystemEventTypes.QueueItemRemoved:
                            if (mediaItem != null &&
                                mediaItem.FilePath == SelectedMediaItem.FilePath)
                            {
                                LoadMediaActionsForMediaItem(SelectedMediaItem);
                            }
                            break;
                    }
                }

                /*
                if (mediaItem != null &&
                    mediaItem.FilePath == SelectedMediaItem.FilePath)
                {
                    LoadMediaActionsForMediaItem(SelectedMediaItem);
                }
                */

                System.Diagnostics.Debug.WriteLine("[out] OnQueueUpdated in CurrentPageModel");
            };

            // Set handler for media player events
            _mediaPlayer.Events.OnStatusChange += delegate (MediaPlayerStatuses status, MediaPlayerException? mediaPlayerException)
            {
                HandleMediaPlayerStatus(status, mediaPlayerException);
            };

            // Set handler for media player debug events
            _mediaPlayer.Events.OnDebug += delegate (string message)
            {
                if (OnDebugAction != null) OnDebugAction(message);
            };
        }

        /// <summary>
        /// Select and play media item from start
        /// </summary>
        /// <param name="mediaItem"></param>
        private void SelectAndPlayMediaItem(MediaItem mediaItem)
        {   
            // Stop any current media item
            Stop();

            // Set selected media item
            SelectedMediaItem = mediaItem;
            
            // Play
            PlayToggleCommand.Execute(null);
        }

        /// <summary>
        /// Apply language change
        /// </summary>
        /// <param name="userSettings"></param>
        private void ApplyLanguageSettings(UserSettings userSettings)
        {
            var cultureInfo = System.Globalization.CultureInfo.GetCultureInfo(userSettings.CultureName);
            LocalizationResources.SetCulture(cultureInfo);
        }

        /// <summary>
        /// Applies equalizer defaults
        /// </summary>
        private void ApplyEqualizerSettings(UserSettings userSettings)
        {
            // Clear defaults
            _mediaPlayer.AudioEqualizer.DefaultPresetName = "";
            _mediaPlayer.AudioEqualizer.DefaultCustomBandLevels = new List<short>();
           
            // Set defaults, doesn't apply them
            var customAudioSettings = userSettings.CustomAudioSettingsList.FirstOrDefault(s => s.Id == userSettings.AudioSettingsId);
            if (customAudioSettings == null)   // Preset
            {
                var audioSettings = _audioSettingsService.GetById(userSettings.AudioSettingsId);
                _mediaPlayer.AudioEqualizer.DefaultPresetName = audioSettings.Name;
            }
            else       // Custom
            {
                _mediaPlayer.AudioEqualizer.DefaultCustomBandLevels = customAudioSettings.AudioBands;
            }

            // Apply preset
            _mediaPlayer.AudioEqualizer.ApplyDefault();
        }

        public bool IsDebugMode => false;

        private string _errorMessage;
        public bool IsErrorMessage => !String.IsNullOrEmpty(_errorMessage);

        public string ErrorMessage
        {
            get { return _errorMessage; }
            set
            {
                _errorMessage = value;

                OnPropertyChanged(nameof(ErrorMessage));
                OnPropertyChanged(nameof(IsErrorMessage));
            }
        }

        private Action<double> _elapsedAction;
        public void SetElapsedAction(Action<double> action)
        {
            _elapsedAction = action;
        }

        public string? MediaItemName => _selectedMediaItem != null ? _selectedMediaItem.Name : null;

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

        ///// <summary>
        ///// Command to restart current media item
        ///// </summary>
        //public ICommand RestartCommand { get; set; }

        /// <summary>
        /// Command to play or pause/stop current media item
        /// </summary>
        public ICommand PlayToggleCommand { get; set; }

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
            OnPropertyChanged(nameof(IsPlayToggleEnabled));
            OnPropertyChanged(nameof(IsPlayToggleVisible));
            OnPropertyChanged(nameof(IsSelectPositionEnabled));
            OnPropertyChanged(nameof(IsSelectPositionVisible));
            OnPropertyChanged(nameof(IsStarting));  // Not typically bound
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
            OnPropertyChanged(nameof(ElapsedTimeString));
            OnPropertyChanged(nameof(ElapsedMS));
            OnPropertyChanged(nameof(RemainingTimeString));
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
                _currentState.CurrentMediaItem = value;

                // Notify properties on change of selected media item
                OnPropertyChanged(nameof(SelectedMediaItem));
                OnPropertyChanged(nameof(IsRealMediaItemSelected));
                OnPropertyChanged(nameof(IsRealPlayableMediaItemSelected));
                OnPropertyChanged(nameof(MainLogoImage));   // If media item specific image
                OnPropertyChanged(nameof(MediaItemName));

                //// Stop current media
                //var isWasPlaying = IsPlaying;
                //if (isWasPlaying || IsPaused)
                //{
                //    Stop();
                //}

                if (_selectedMediaItem != null)
                {
                    // Set actions for media item (Add to playlist X etc)
                    LoadMediaActionsForMediaItem(_selectedMediaItem);

                    //// Respect previous play state. Only play if was playing. Don't play if was paused or stopped.
                    //if (IsRealMediaItemSelected && isWasPlaying)
                    //{
                    //    StartPlayMediaItem(_selectedMediaItem);
                    //}
                }

                // Notify play state changed. Can now play media item
                NotifyPropertiesChangedForPlayState();
            }
        }

        /// <summary>
        /// Main logo image to display   
        /// 
        /// Media item				        Image
        /// ----------                      -----
        /// Music item (Real album)			MediaItemCollection.ImagePath
        /// Music item (All albums)			MediaItem.ImagePath
        /// Podcast (Real album)			MediaItemCollection.ImagePath
        /// Podcast (All albums)			MediaItem.ImagePath
        /// Queue item				        MediaItemCollection.ImagePath
        /// Radio stream				    MediaItem.ImagePath
        /// </summary>
        public string MainLogoImage
        {
            get
            {
                // If [All abums] selected then show media item image which defaults to album image
                if (_currentState.SelectedMediaLocation != null &&
                    _currentState.SelectedMediaItemCollection != null &&
                    _currentState.SelectedMediaItemCollection.EntityCategory == EntityCategory.All &&
                    _selectedMediaItem != null)
                {
                    return _selectedMediaItem.ImagePath;
                }

                // Set media item level image. E.g. Radio stream, playlist item with image for album.
                // If storage then do nothing because we want to display the album image.
                if (_currentState.SelectedMediaLocation != null && 
                    _currentState.SelectedMediaLocation.MediaSourceType != MediaSourceTypes.Storage)
                {
                    if (_selectedMediaItem != null && !String.IsNullOrEmpty(_selectedMediaItem.ImagePath))
                    {
                        return _selectedMediaItem.ImagePath;
                    }
                }

                // Set media item collection level image. E.g. Album image
                if (_currentState.SelectedMediaItemCollection != null && 
                    !String.IsNullOrEmpty(_currentState.SelectedMediaItemCollection.ImagePath))
                {
                    return _currentState.SelectedMediaItemCollection.ImagePath;
                }

                // Set media source level image
                if (_currentState.SelectedMediaSource != null) return _currentState.SelectedMediaSource.ImagePath;

                return "cassette_player_audio_speaker_sound_icon.png";  // Default
            }
        }

        ///// <summary>
        ///// Restarts media item
        ///// </summary>
        ///// <param name="parameter"></param>
        //private void DoRestart(object parameter)
        //{
        //    if (_currentState.SelectMediaItemAction != null)
        //    {
        //        if (IsPlaying || IsPaused)    // Just move to start
        //        {
        //            ElapsedMS = 0;
        //        }
        //        else   // Media item completed, play it again
        //        {
        //            StartPlayMediaItem(SelectedMediaItem);
        //        }
        //    }
        //}

        /// <summary>
        /// Determines next media item (if any) to play. Returns null if user has selected a media item collection
        /// that doesn't contain current media item.
        /// </summary>
        /// <returns></returns>
        private MediaItem? GetNextMediaItem()
        {
            // Get index of selected media item
            var index = _currentState.MediaItems.IndexOf(SelectedMediaItem!);

            if (index != -1) // User still has current media item collection selected
            {
                if (index < _currentState.MediaItems.Count - 1)
                {
                    return _currentState.MediaItems[index + 1];
                }
            }

            return null;
        }

        /// <summary>
        /// Determines previous media item (if any) to play.  Returns null if user has selected a media item collection
        /// that doesn't contain current media item.
        /// </summary>
        /// <returns></returns>
        private MediaItem? GetPrevMediaItem()
        {
            // Get index of selected media item
            var index = _currentState.MediaItems.IndexOf(SelectedMediaItem!);

            if (index != -1) // User still has current media item collection selected
            {
                if (index > 0)
                {
                    return _currentState.MediaItems[index - 1];
                }
            }

            return null;
        }

        /// <summary>
        /// Plays next media item. If shuffle play then media items already sorted randomly
        /// </summary>
        /// <param name="parameter"></param>
        private void DoNext(object parameter)
        {
            var mediaItem = GetNextMediaItem();
            if (mediaItem != null)
            {
                SelectAndPlayMediaItem(mediaItem);
            }
        }

        /// <summary>
        /// Plays previous media item. If shuffle play then media items already sorted randomly
        /// </summary>
        /// <param name="parameter"></param>
        private void DoPrev(object parameter)
        {            
            var mediaItem = GetPrevMediaItem();
            if (mediaItem != null)
            {
                SelectAndPlayMediaItem(mediaItem);
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
        private void DoPlayToggle(object parameter)
        {
            if (IsStarting || IsPlaying)   // Pause or stop
            {
                if (_selectedMediaItem.IsPausable)
                {
                    Pause();
                }
                else
                {
                    Stop();
                }                     
            }
            else    // Play
            {             
                /*
                var task = Task.Factory.StartNew(() =>
                {                    
                    // Run in main thread.
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        StartPlayMediaItem(SelectedMediaItem!);                        
                    });
                });
                */

                StartPlayMediaItem(SelectedMediaItem!);

                // Refresh play toggle button while media item is started
                OnPropertyChanged(nameof(PlayButtonImageSource));
            }
        }

        /// <summary>
        /// Starts playing media item.
        /// 
        /// Some media items (E.g. Streaming) may take many seconds to start. During this time then we show the
        /// busy indicator and the pause/stop buttons.
        /// </summary>
        /// <param name="mediaItem"></param>
        private void StartPlayMediaItem(MediaItem mediaItem)
        {
            // Set busy, may take time to start. IsBusy = false when Starting event is handled
            IsBusy = true;

            // Stop current media item if different media item to current
            if (IsStarting || IsPlaying || IsPaused)
            {
                if (mediaItem.FilePath != _mediaPlayer.CurrentFilePath)
                {
                    Stop();
                }
            }

            // Play or resume
            _mediaPlayer.Play(mediaItem.FilePath, (exception) =>
            {
                if (OnMediaPlayerError != null)
                {
                    OnMediaPlayerError(new MediaPlayerException("Error playing media", exception));
                }
                System.Diagnostics.Debug.WriteLine($"Error playing audio: {exception.Message}");
            });            
        }
  
        private void _timer_Elapsed(object? sender, ElapsedEventArgs e)
        {
            //System.Diagnostics.Debug.WriteLine($"_timer_Elapsed: ElapsedMS={ElapsedMS}");

            // Execute action for elapsed time
            if (_elapsedAction != null)
            {
                _elapsedAction(ElapsedMS);
            }

            // TODO: Consider disabling timer when app not visible

            // Notify property changes to update UI. Only need to be concerned about updating UI with progress
            // for current media item.            
            OnPropertyChanged(nameof(ElapsedTimeString));
            OnPropertyChanged(nameof(ElapsedMS));
            OnPropertyChanged(nameof(RemainingTimeString));
            OnPropertyChanged(nameof(RemainingMS));
        }        
  
        /// <summary>
        /// Elapsed time for media item (00:00:00)
        /// </summary>
        public string ElapsedTimeString
        {
            get
            {
                return GetDurationString(_mediaPlayer.ElapsedTime);                

                /*
                if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                {
                    return GetDurationString(_mediaPlayer.GetElapsedPlayTime());
                }
                return GetDurationString(TimeSpan.Zero);    // "00:00:00"
                */
            }
        }

        /// <summary>
        /// Remaining time for media item (00:00:00)
        /// </summary>
        public string RemainingTimeString
        {
            get
            {
                return GetDurationString(_mediaPlayer.RemainingTime);

                //if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
                //{
                //    return GetDurationString(_mediaPlayer.GetTotalDuration() - _mediaPlayer.GetElapsedPlayTime());
                //}
                //return GetDurationString(TimeSpan.Zero);
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
                return _mediaPlayer.RemainingTime.TotalMilliseconds;

                //if (_mediaPlayer.IsPlaying || _mediaPlayer.IsPaused)
               // {
               //     return (int)(_mediaPlayer.GetTotalDuration().TotalMilliseconds - _mediaPlayer.GetElapsedPlayTime().TotalMilliseconds);
                //}
                //return 0;
            }
        }

        /// <summary>
        /// Duration of current media item (MS)
        /// </summary>
        public System.Double DurationMS
        {
            get
            {
                return _mediaPlayer.DurationTime.TotalMilliseconds;
            }
        }

        /// <summary>
        /// Elapsed time of current media item
        /// </summary>
        public System.Double ElapsedMS
        {
            get
            {
                return _mediaPlayer.ElapsedTime.TotalMilliseconds;
            }

            set
            {
                _mediaPlayer.ElapsedTime = TimeSpan.FromMilliseconds(value);
            }
        }
        
        public void SetElapsedMS(double value)
        {
            ElapsedMS = value;

            // Update UI. If completed media item then play toggle was previously disabled and so we need to enable
            // it so that user can press play.
            NotifyPropertiesChangedForPlayState();
        }

        /// <summary>
        /// Whether media item is starting. May take many seconds (E.g. For streaming)
        /// </summary>
        public bool IsStarting
        {
            get { return _mediaPlayer.IsStarting; }
        }

        /// <summary>
        /// Whether media item is completed
        /// </summary>
        public bool IsCompleted
        {
            get { return _mediaPlayer.IsCompleted; }
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
            System.Diagnostics.Debug.WriteLine("Entered CurrentPageModel.Stop");
            _mediaPlayer.Stop();
            _elapsedTimer.Enabled = false;
            System.Diagnostics.Debug.WriteLine("Leaving CurrentPageModel.Stop");
        }

        /// <summary>
        /// Image source for the play button (Play/Pause/Stop)
        /// </summary>
        public string PlayButtonImageSource
        {
            get
            {                
                if (IsStarting || IsPlaying)
                {                   
                    if (_selectedMediaItem.IsPausable)
                    {
                        return ImageConstants.PauseMediaItemImage;
                    }
                    else
                    {
                        return ImageConstants.StopMediaItemImage;
                    }                    
                }
                return ImageConstants.PlayMediaItemImage;
            }
        }

        /// <summary>
        /// Handles media player status change
        /// </summary>
        /// <param name="status"></param>
        private void HandleMediaPlayerStatus(MediaPlayerStatuses status, MediaPlayerException? exception)
        {
            System.Diagnostics.Debug.WriteLine($"HandleMediaPlayerStatus: {status}");

            if (_selectedMediaItem != null)     // Sanity check
            {                
                // Reset IsBusy status. When user presses Play then busy indicator is displayed. Normally will subsequently                
                // receive Started status. For items slow to play (E.g. Streaming) then user can press Stop.
                if (IsBusy &&
                    (status == MediaPlayerStatuses.Playing ||      
                    status == MediaPlayerStatuses.Started ||                    
                    status == MediaPlayerStatuses.Stopped ||
                    status == MediaPlayerStatuses.PlayError))
                {
                    IsBusy = false;
                }

                NotifyPropertiesChangedForPlayState();
                
                switch (status)
                {
                    case MediaPlayerStatuses.Completed:                        
                        _elapsedTimer.Enabled = false;

                        //var isPlayEnabled = this.IsPlayToggleEnabled;
                        //var isPlayVisible = this.IsPlayToggleVisible;
                        //var elapsed = _mediaPlayer.ElapsedTime;
                        //var duration = _mediaPlayer.DurationTime;
                        //var diff = duration - elapsed;
                        //var diffMS = diff.TotalMilliseconds;

                        _currentState.Events.RaiseOnCurrentMediaItemStatusChanged(_selectedMediaItem, IsPlaying, IsPaused);

                        // Play next if configured, otherwise just leave selected media item
                        if (_currentState.AutoPlayNext &&
                            SelectedMediaItem != _currentState.MediaItems.Last())
                        {
                            NextCommand.Execute(null);
                        }
                        break;
                    case MediaPlayerStatuses.Paused:                        
                        _elapsedTimer.Enabled = false;

                        _currentState.Events.RaiseOnCurrentMediaItemStatusChanged(_selectedMediaItem, IsPlaying, IsPaused);
                        break;
                    case MediaPlayerStatuses.Playing:                        
                        _elapsedTimer.Enabled = true;

                        _currentState.Events.RaiseOnCurrentMediaItemStatusChanged(_selectedMediaItem, IsPlaying, IsPaused);
                        break;
                    case MediaPlayerStatuses.Started:                        
                        _elapsedTimer.Enabled = true;

                        _currentState.Events.RaiseOnCurrentMediaItemStatusChanged(_selectedMediaItem, IsPlaying, IsPaused);
                        break;
                    case MediaPlayerStatuses.Stopped:                        
                        _elapsedTimer.Enabled = false;

                        _currentState.Events.RaiseOnCurrentMediaItemStatusChanged(_selectedMediaItem, IsPlaying, IsPaused);
                        break;
                    case MediaPlayerStatuses.PlayError:                        
                        _elapsedTimer.Enabled = false;
                        
                        // Notify error
                        if (OnMediaPlayerError != null && exception != null)
                        {
                            OnMediaPlayerError(exception);
                        }
                        break;
                }
            }

            if (OnDebugAction != null) OnDebugAction($"Status={status}");
        }    

        /// <summary>
        /// Whether option to play/pause/stop is visible        
        /// </summary>
        public bool IsPlayToggleEnabled
        {
            get
            {                
                if (IsRealMediaItemSelected)
                {                    
                    // If end of media item then user shouldn't be able to press play/pause/stop
                    // Elapsed & duration seem to be may be 30 ms different at the end
                    if (_mediaPlayer.ElapsedTime > TimeSpan.Zero &&
                         Math.Abs(_mediaPlayer.ElapsedTime.TotalMilliseconds - _mediaPlayer.DurationTime.TotalMilliseconds) <= 200)
                    {
                        return false;
                    }

                    return true;
                }

                return false;
            }
        }

        /// <summary>
        /// Whether option to play/pause/stop is visible
        /// </summary>
        public bool IsPlayToggleVisible
        {
            get
            {
                return IsRealMediaItemSelected;
            }
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
                    return GetPrevMediaItem() != null;
                    //var index = _currentState.MediaItems.IndexOf(SelectedMediaItem);
                    //return index > 0;
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
                if (_selectedMediaItem != null) return !_selectedMediaItem.IsStreamed;
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
                    return GetNextMediaItem() != null;
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
                if (_selectedMediaItem != null) return !_selectedMediaItem.IsStreamed;
                return true;
            }
        }

        /// <summary>
        /// Whether select position control is visible
        /// </summary>
        public bool IsSelectPositionVisible
        {
            get
            {
                if (_selectedMediaItem != null) return !_selectedMediaItem.IsStreamed;
             
                return false;
            }
        }

        /// <summary>
        /// Whether user can select position
        /// </summary>
        public bool IsSelectPositionEnabled
        {
            get
            {
                if (_selectedMediaItem != null)
                {
                    return IsSelectPositionVisible &&
                        (IsPlaying || IsPaused || IsCompleted);
                }
                return false;
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

        /// <summary>
        /// Whether a real and playable media item is selected rather than [None], [Multiple], [Shuffle] etc
        /// </summary>
        public bool IsRealPlayableMediaItemSelected
        {
            get
            {
                return IsRealMediaItemSelected && SelectedMediaItem!.IsPlayable;
            }
        }

        /// <summary>
        /// Executes media action. May trigger event from ICurrentState.Events which does more processing.
        /// </summary>
        /// <param name="mediaAction"></param>
        public void ExecuteMediaAction(MediaAction mediaAction)
        {
            if (mediaAction != null)
            {
                // Get media source to execute action against. We may be displaying the storage source but need to execute
                // 'Add to playlist X' against the playlist source.
                var mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == mediaAction.MediaLocationName);

                // Execute action. E.g. Add to playlist X, remove from playlist Y etc.
                // If action relates to playlists then ICurrentState.Events.OnPlaylistUpdated handler will refresh the list of media
                // actions.
                mediaSource.ExecuteMediaAction(mediaAction);
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
