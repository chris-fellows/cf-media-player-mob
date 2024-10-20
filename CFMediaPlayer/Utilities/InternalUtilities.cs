using System.ComponentModel.DataAnnotations;
using System.Reflection;

namespace CFMediaPlayer.Utilities
{
    public static class InternalUtilities
    {
        public static string DefaultImagePath = "cassette_player_audio_speaker_sound_icon.png";

        /// <summary>
        /// Gets resource key for enum. Enum value must have Display attribute with Description property set
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="item"></param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
        public static string GetEnumResourceKey<T>(T item) where T : Enum
        {            
            var displayAttribute = item.GetType().GetMember(item.ToString()).FirstOrDefault().GetCustomAttribute<DisplayAttribute>();
            if (displayAttribute != null && !String.IsNullOrEmpty(displayAttribute.Description))
            {
                return displayAttribute.Description;
            }

            throw new ArgumentException($"Enum {item} does not have resource key indicated");
        }
    }
}
