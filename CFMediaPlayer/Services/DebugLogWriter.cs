using AndroidX.AppCompat.View.Menu;
using CFMediaPlayer.Interfaces;

namespace CFMediaPlayer.Services
{
    internal class DebugLogWriter : ILogWriter
    {
        public void Log(string message)
        {
            System.Diagnostics.Debug.WriteLine($"{DateTimeOffset.UtcNow.ToString()} : {message}");

            //try
            //{
            //    var logFolder = Path.Combine(Android.OS.Environment.ExternalStorageDirectory.Path, Android.OS.Environment.DirectoryMusic);
            //    var logFile = Path.Combine(logFolder, "CFMediaPlayer.txt");

            //    using (var stream = new StreamWriter(logFile, true))
            //    {
            //        stream.WriteLine($"{DateTimeOffset.UtcNow.ToString()}\t{message}");
            //        stream.Flush();
            //    }
            //}
            //catch { };      // Ignore errors
        }
    }
}
