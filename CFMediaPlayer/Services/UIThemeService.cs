using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class UIThemeService : IUIThemeService
    {
        public List<UITheme> GetAll()
        {
            var themes = new List<UITheme>();

            themes.Add(new UITheme()
            {
                Id = "1",
                Name = "Light"
            });

            themes.Add(new UITheme()
            {
                Id = "2",
                Name = "Dark"
            });

            return themes;                
        }
    }
}
