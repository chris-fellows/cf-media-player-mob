using Android.Media.Audiofx;
using Android.Media;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Interfaces;

namespace CFMediaPlayer
{
    /// <summary>
    /// Android media player
    /// </summary>
    internal class AndroidMediaPlayer : IMediaPlayer, IDisposable
    {
        private Android.Media.MediaPlayer _mediaPlayer = null;
        private Equalizer _equalizer = null;
        private int _currentPosition = 0;
        private string? _currentFilePath;
        private bool _isPrepared;
        private string _equalizerPresetName = String.Empty;
        private Action<string>? _debugAction;        
        private Action<MediaPlayerStatuses>? _statusAction;
       
        public void Dispose()
        {
            Stop();
        }

        public void SetStatusAction(Action<MediaPlayerStatuses> action)
        {
            _statusAction = action;
        }

        public void SetDebugAction(Action<string> action)
        {
            _debugAction = action;
        }

        public void PlayAudio(string filePath,                         
                              Action<System.Exception> errorAction)
        {    
            // Clean up if playing different file
            if (filePath != _currentFilePath)
            {                
                Stop();                
            }            

            if (_mediaPlayer != null && !_mediaPlayer.IsPlaying)        // Paused, resume it
            {
                if (_debugAction != null) _debugAction($"Starting from {_currentPosition}");
                _mediaPlayer.SeekTo(_currentPosition);
                _currentPosition = 0;
                _mediaPlayer.Start();
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.Playing);
            }
            else if (_mediaPlayer == null || !_mediaPlayer.IsPlaying)   // Not started
            {                
                try
                {                    
                    _currentPosition = 0;
                    _isPrepared = false;
                    _mediaPlayer = new Android.Media.MediaPlayer();
                    if (_debugAction != null) _debugAction("Setting media: " + filePath);
                    _mediaPlayer.SetDataSource(filePath);
                    _currentFilePath = filePath;
                    _mediaPlayer.SetAudioStreamType(Android.Media.Stream.Music);
                    if (_debugAction != null) _debugAction("Preparing " + filePath);                    
                    _mediaPlayer.PrepareAsync();
                    _mediaPlayer.Prepared += (sender, args) =>
                    {                        
                        // Allow data to buffer
                        Thread.Sleep(1000);

                        _isPrepared = true;
                        if (_debugAction != null) _debugAction("Playing");
                        _mediaPlayer.Start();
                        if (_statusAction != null) _statusAction(MediaPlayerStatuses.Playing);
                    };
                    _mediaPlayer.Completion += (sender, args) =>
                    {
                        if (_debugAction != null) _debugAction($"Completed");
                        if (_statusAction != null) _statusAction(MediaPlayerStatuses.Completed);
                    };
                }
                catch (Exception e)
                {
                    errorAction(e);
                    _mediaPlayer = null;
                    if (_statusAction != null) _statusAction(MediaPlayerStatuses.StartError);
                }
            }

            // Apply equalizer preset
            _equalizer = new Equalizer(0, _mediaPlayer.AudioSessionId);
            if (!String.IsNullOrEmpty(_equalizerPresetName))
            {
                ApplyEqualizerPreset(_equalizerPresetName);
            }            
        }

        public void Pause()
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
                _currentPosition = _mediaPlayer.CurrentPosition;

                if (_debugAction != null) _debugAction("Paused");
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.Paused);
            }
        }
        public void Stop()
        {
            if (_mediaPlayer != null)
            {
                if (_isPrepared)
                {
                    _mediaPlayer.Stop();                                                   
                }
                _mediaPlayer.Release();
                _equalizer.Release();

                _isPrepared = false;
                _mediaPlayer = null;
                _equalizer = null;
                _currentPosition = 0;
                _currentFilePath = "";
                if (_debugAction != null) _debugAction("Stopped");
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.Stopped);                
            }
        }
        public TimeSpan GetElapsedPlayTime()
        {                        
            return _mediaPlayer == null ? TimeSpan.Zero : 
                        TimeSpan.FromMilliseconds(_mediaPlayer.CurrentPosition);            
        }

        public void SetElapsedPlayTime(TimeSpan elapsedPlayTime)
        {
            if (_mediaPlayer != null)
            {
                _mediaPlayer.SeekTo((int)elapsedPlayTime.TotalMilliseconds);
                _currentPosition = _mediaPlayer.CurrentPosition;
            }
        }

        public TimeSpan GetTotalDuration()
        {
            return _mediaPlayer == null ? TimeSpan.Zero :
                        TimeSpan.FromMilliseconds(_mediaPlayer.Duration);
        }

        public bool IsPlaying
        {
            get { return _mediaPlayer != null && _mediaPlayer.IsPlaying; }
        }

        public bool IsPaused
        {
            get { return _mediaPlayer != null &&
                    !_mediaPlayer.IsPlaying && 
                    _mediaPlayer.CurrentPosition > 0;  }
        }

        public void ApplyEqualizerTest()
        {
            var equalizer = new Equalizer(0, _mediaPlayer.AudioSessionId);            

            for (short index =0; index < equalizer.NumberOfPresets; index++)
            {
                var preset = equalizer.GetPresetName(index);                        

                int xxxx = 1000;
            }

            int xxx = 1000;            
        }

        public string EqualizerPresetName
        {
            get { return _equalizerPresetName; }
            set
            {
                if (_equalizerPresetName != value)
                {
                    _equalizerPresetName = value;
                                                      
                    ApplyEqualizerPreset(_equalizerPresetName);                    
                }
            }
        }

        /// <summary>
        /// Applies equalizer preset
        /// </summary>
        /// <param name="presetName"></param>
        private void ApplyEqualizerPreset(string presetName)
        {
            if (!String.IsNullOrEmpty(_equalizerPresetName) && _equalizer != null)
            {
                for (short index = 0; index < _equalizer.NumberOfPresets; index++)
                {
                    var currentPresetName = _equalizer.GetPresetName(index);
                    if (currentPresetName.Equals(presetName, StringComparison.InvariantCultureIgnoreCase))
                    {
                        _equalizer.UsePreset(index);
                        break;
                    }
                }
            }
        }
    }
}