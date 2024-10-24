using Android.Media.Audiofx;
using Android.Media;
using CFMediaPlayer.Enums;
using CFMediaPlayer.Exceptions;
using CFMediaPlayer.Interfaces;
using Android.Content;
using System.Diagnostics.Contracts;
using CFMediaPlayer.Models;

namespace CFMediaPlayer
{
    /// <summary>
    /// Android media player
    /// </summary>
    internal class AndroidMediaPlayer : IMediaPlayer, IDisposable
    {
        private Android.Media.MediaPlayer _mediaPlayer = null;      
        private int _currentPosition = 0;
        private string? _currentFilePath;     
        private bool _isPrepared;
        private AndroidAudioEqualizer _audioEqualizer = new AndroidAudioEqualizer();  
        private Action<string>? _debugAction;        
        private Action<MediaPlayerStatuses, MediaPlayerException?>? _statusAction;
       
        public void Dispose()
        {
            // Stop if necessary
            Stop();

            // Clean up equalizer
            if (_audioEqualizer != null && _audioEqualizer.Equalizer != null)
            {
                _audioEqualizer.Equalizer.Release();
                _audioEqualizer.Equalizer = null;
                _audioEqualizer = null;
            }

            // Clean up media player
            if (_mediaPlayer != null)
            {                
                _mediaPlayer.Release();
                _mediaPlayer = null;
            }
        }

        public string CurrentFilePath => _currentFilePath;

        public void SetStatusAction(Action<MediaPlayerStatuses, MediaPlayerException?> action)
        {
            _statusAction = action;
        }

        public void SetDebugAction(Action<string> action)
        {
            _debugAction = action;
        }

        /// <summary>
        /// Initialises media player
        /// </summary>
        /// <param name="errorAction"></param>
        private void InitialiseMediaPlayer(Action<System.Exception> errorAction)
        {
            _mediaPlayer = new Android.Media.MediaPlayer();

            // If you play an invalid file and the error handler is defined then it fires but the Completion event doesn't.
            // If you play an invalid file and the error handler isn't defined then it fires the Completion event.
            _mediaPlayer.Error += (object? sender, MediaPlayer.ErrorEventArgs e) =>
            {                
                var mediaPlayerException = new MediaPlayerException("Error playing media") { MediaError = e.What };
                errorAction(mediaPlayerException);
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.PlayError, mediaPlayerException);
            };            
            
            _mediaPlayer.Prepared += (sender, args) =>
            {
                // Allow data to buffer
                Thread.Sleep(2000);

                _isPrepared = true;
                if (_debugAction != null) _debugAction("Playing");
                _mediaPlayer.Start();
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.Playing, null);
            };

            _mediaPlayer.Completion += (sender, args) =>
            {
                _currentFilePath = "";
                if (_debugAction != null) _debugAction($"Completed");
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.Completed, null);
            };

            _audioEqualizer.Equalizer = new Equalizer(0, _mediaPlayer.AudioSessionId);
            _audioEqualizer.Equalizer.SetEnabled(true);
            if (!String.IsNullOrEmpty(_audioEqualizer.EqualizerPresetName))
            {
                _audioEqualizer.ApplyPreset();
            }
        }

        public void Play(string filePath,                         
                              Action<System.Exception> errorAction)
        {        
            // Initialise media player if first time
            if (_mediaPlayer == null)
            {
                InitialiseMediaPlayer(errorAction);
            }

            // Clean up if playing different file
            if (filePath != _currentFilePath && (IsPlaying || IsPaused))
            {                
                Stop();                
            }                       
            
            if (!_mediaPlayer.IsPlaying && _mediaPlayer.CurrentPosition > 0)        // Paused, resume it
            {
                if (_debugAction != null) _debugAction($"Starting from {_currentPosition}");
                _mediaPlayer.SeekTo(_currentPosition);
                _currentPosition = 0;
                _mediaPlayer.Start();
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.Playing, null);
            }
            else     // Not playing
            {                
                try
                {                    
                    _currentPosition = 0;
                    _isPrepared = false;                  
                    if (_debugAction != null) _debugAction("Setting media: " + filePath);
                    _mediaPlayer.SetDataSource(filePath);                    
                    _currentFilePath = filePath;                    
                    _mediaPlayer.SetAudioStreamType(Android.Media.Stream.Music);
                    if (_debugAction != null) _debugAction("Preparing " + filePath);

                    //// If you play an invalid file and the error handler is defined then it fires but the Completion event doesn't.
                    //// If you play an invalid file and the error handler isn't defined then it fires the Completion event.
                    //_mediaPlayer.Error += (object? sender, MediaPlayer.ErrorEventArgs e) =>
                    //{
                    //    var mediaPlayerException = new MediaPlayerException("Error playing media") { MediaError = e.What };
                    //    errorAction(mediaPlayerException);                        
                    //    if (_statusAction != null) _statusAction(MediaPlayerStatuses.PlayError, mediaPlayerException);                        
                    //};

                    _mediaPlayer.PrepareAsync();                  
                }
                catch (Exception e)
                {
                    errorAction(e);
                    _mediaPlayer = null;
                    if (_statusAction != null) _statusAction(MediaPlayerStatuses.PlayError, new MediaPlayerException("Error playing media", e));
                }
            }

            //// Apply equalizer preset
            //_audioEqualizer.Equalizer = new Equalizer(0, _mediaPlayer.AudioSessionId);
            //_audioEqualizer.Equalizer.SetEnabled(true);
            //if (!String.IsNullOrEmpty(_audioEqualizer.EqualizerPresetName))
            //{
            //    _audioEqualizer.ApplyPreset();                
            //}            
        }

        //private void _mediaPlayer_Error(object? sender, MediaPlayer.ErrorEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        public void Pause()
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {                
                _mediaPlayer.Pause();
                _currentPosition = _mediaPlayer.CurrentPosition;

                if (_debugAction != null) _debugAction("Paused");
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.Paused, null);
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
                _audioEqualizer.Equalizer.Release();
                _isPrepared = false;
                _mediaPlayer = null;
                //_equalizer = null;
                _audioEqualizer.Equalizer = null;
                _audioEqualizer = null;
                _currentPosition = 0;
                _currentFilePath = "";
                if (_debugAction != null) _debugAction("Stopped");
                if (_statusAction != null) _statusAction(MediaPlayerStatuses.Stopped, null);                
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
            //var equalizer = new Equalizer(0, _mediaPlayer.AudioSessionId);            

            //for (short band = 0; band < equalizer.NumberOfBands; band++)
            //{
            //    var freqRange = equalizer.GetBandFreqRange(band);
            //    var bandLevel = equalizer.GetBandLevel(band);
            //    var centerFreq = equalizer.GetCenterFreq(band);

            //    System.Diagnostics.Debug.WriteLine($"Band={band}; FreqRange={freqRange[0]}-{freqRange[1]}; BandLevel={bandLevel}; CenterFreq={centerFreq}");

            //    int zzz = 1000;                
            //}

            //for (short index =0; index < equalizer.NumberOfPresets; index++)
            //{
            //    var preset = equalizer.GetPresetName(index);                        

            //    int xxxx = 1000;
            //}

            int xxx = 1000;            
        }

        public IAudioEqualizer AudioEqualizer => _audioEqualizer;     
    }
}