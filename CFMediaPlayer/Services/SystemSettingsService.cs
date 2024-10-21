using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class SystemSettingsService : XmlEntityWithIdStoreService<SystemSettings, string>, ISystemSettingsService
    {        
        public SystemSettingsService(string folder) : base(folder, "SystemSettings.*.xml",
                                            (systemSettings) => $"SystemSettings.{systemSettings.Id}.xml",
                                            (id) => $"SystemSettings.{id}.xml")
        {

        } 
    }
}
