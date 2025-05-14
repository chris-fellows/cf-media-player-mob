using CFMediaPlayer.Resources.Languages;
using System.ComponentModel;
using System.Globalization;

namespace CFMediaPlayer
{
    /// <summary>
    /// Localization resources. 
    /// 
    /// Class should be 
    /// </summary>
    public class LocalizationResources : INotifyPropertyChanged
    {
        private LocalizationResources()
        {
            AppResources.Culture = CultureInfo.CurrentCulture;
        }

        public static LocalizationResources Instance { get; } = new();

        /// <summary>
        /// Returns resource string for resource key. E.g. LocationResources["MyKeyX"]
        /// </summary>
        /// <param name="resourceKey"></param>
        /// <returns></returns>
        public object this[string resourceKey]
            => AppResources.ResourceManager.GetObject(resourceKey, AppResources.Culture) ?? Array.Empty<byte>();

        public event PropertyChangedEventHandler? PropertyChanged;

        /// <summary>
        /// Sets culture
        /// </summary>
        /// <param name="culture"></param>
        public void SetCulture(CultureInfo culture)
        {
            AppResources.Culture = culture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
