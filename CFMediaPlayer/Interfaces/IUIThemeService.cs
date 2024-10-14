using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// UI theme service
    /// </summary>
    // TODO: Consider storing in XML files and populating on startup if empty
    public interface IUIThemeService
    {
        List<UITheme> GetAll();
    }
}
