namespace CFMediaPlayer.Utilities
{
    internal class MediaUtilities
    {
        /// <summary>
        /// Whether folder contains audio files in root
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFolderHasAudioFiles(string path)
        {
            foreach (var extension in AudioFileExtensions)
            {
                if (Directory.GetFiles(path, $"*{extension}").Any())
                {
                    return true;
                }
            }

            return false;
        }

        public static string[] AudioFileExtensions
        {
            get { return new[] { ".mp3", ".ogg", ".wma", ".wav" }; }
        }

        //public static bool IsAudioFile(string file)
        //{
        //    return Array.IndexOf(new[] { ".mp3", ".wma", ".wav" }, Path.GetExtension(file).ToLower()) != -1;
        //}
    }
}
