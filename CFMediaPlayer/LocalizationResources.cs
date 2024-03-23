using CFMediaPlayer.Resources.Languages;
using System.ComponentModel;
using System.Globalization;

namespace CFMediaPlayer
{
    /// <summary>
    /// Localization resources    
    /// </summary>
    public class LocalizationResources : INotifyPropertyChanged
    {
        private LocalizationResources()
        {
            AppResources.Culture = CultureInfo.CurrentCulture;
        }

        public static LocalizationResources Instance { get; } = new();

        public object this[string resourceKey]
            => AppResources.ResourceManager.GetObject(resourceKey, AppResources.Culture) ?? Array.Empty<byte>();

        public event PropertyChangedEventHandler? PropertyChanged;

        public void SetCulture(CultureInfo culture)
        {
            AppResources.Culture = culture;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(null));
        }
    }
}
