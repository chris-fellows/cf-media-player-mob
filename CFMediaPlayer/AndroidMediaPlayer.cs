using Android.Media;
using CFMediaPlayer.Interfaces;
using System.Diagnostics.CodeAnalysis;

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
        private Action<string>? _debugAction;        
        private Action<string>? _statusAction;
       
        public void Dispose()
        {
            Stop();
        }

        public void SetStatusAction(Action<string> action)
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
                _debugAction($"Starting from {_currentPosition}");
                _mediaPlayer.SeekTo(_currentPosition);
                _currentPosition = 0;
                _mediaPlayer.Start();
                _statusAction("Playing");
            }
            else if (_mediaPlayer == null || !_mediaPlayer.IsPlaying)   // Not started
            {                
                try
                {
                    //_mediaPlayer.SetOnBufferingUpdateListener()

                    //_isCompleted = false;
                    _currentPosition = 0;
                    _isPrepared = false;
                    _mediaPlayer = new Android.Media.MediaPlayer();
                    _debugAction("Setting media: " + filePath);
                    _mediaPlayer.SetDataSource(filePath);
                    _currentFilePath = filePath;
                    _mediaPlayer.SetAudioStreamType(Android.Media.Stream.Music);                    
                    _debugAction("Preparing " + filePath);                    
                    _mediaPlayer.PrepareAsync();
                    _mediaPlayer.Prepared += (sender, args) =>
                    {
                        System.Diagnostics.Debug.WriteLine("Prepared");                        

                        // Bit of a kludge. Allow data to buffer. Apparently there's currently no way to set the
                        // buffer size.
                        Thread.Sleep(5000);

                        _isPrepared = true;
                        _debugAction("Playing");
                        _mediaPlayer.Start();
                        _statusAction("Playing");
                    };
                    _mediaPlayer.Completion += (sender, args) =>
                    {
                        _debugAction($"Completed");                        
                        _statusAction("Completed");
                    };
                }
                catch (Exception e)
                {
                    errorAction(e);
                    _mediaPlayer = null;
                    _statusAction("StartError");
                }
            }
            else
            {
                int xxx = 1000;
            }
        }

        public void Pause()
        {
            if (_mediaPlayer != null && _mediaPlayer.IsPlaying)
            {
                _mediaPlayer.Pause();
                _currentPosition = _mediaPlayer.CurrentPosition;

                _debugAction("Paused");
                _statusAction("Paused");
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

                _isPrepared = false;
                _mediaPlayer = null;
                _currentPosition = 0;
                _currentFilePath = "";
                _debugAction("Stopped");
                _statusAction("Stopped");                
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
    }
}