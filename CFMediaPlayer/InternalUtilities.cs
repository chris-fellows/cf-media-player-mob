using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
//using Android.Hardware.Camera2;
using CFMediaPlayer.Models;

namespace CFMediaPlayer
{
    internal class InternalUtilities
    {    
        /// <summary>
        /// Whether folder contains audio files in root
        /// </summary>
        /// <param name="path"></param>
        /// <returns></returns>
        public static bool IsFolderHasAudioFiles(string path)
        {            
            foreach (var extension in InternalUtilities.AudioFileExtensions)
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
