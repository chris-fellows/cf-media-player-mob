using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    // TODO: Consider storing in XML files and populating on startup if empty
    public class CloudProviderService : ICloudProviderService
    {
        public List<CloudProvider> GetAll()
        {            
            var cloudProviders = new List<CloudProvider>()
            {
                new CloudProvider()
                {
                    Id = "1",
                    NameResource = "MediaSourceOneDriveText",
                    Url = ""
                },
                new CloudProvider()
                {
                    Id = "2",
                    NameResource = "MediaSourceGoogleText",
                    Url = ""
                }
            };
            return cloudProviders;
        }
    }
}
