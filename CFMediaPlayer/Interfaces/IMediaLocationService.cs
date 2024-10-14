using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Service for MediaLocation instances
    /// </summary>
    public interface IMediaLocationService
    {
        List<MediaLocation> GetAll();
    }
}
