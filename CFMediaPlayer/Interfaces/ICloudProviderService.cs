using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Service for CloudProvider instances
    /// </summary>
    public interface ICloudProviderService
    {
        List<CloudProvider> GetAll();
    }
}
