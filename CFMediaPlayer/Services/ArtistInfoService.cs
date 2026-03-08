//using CFMediaPlayer.Interfaces;
//using CFMediaPlayer.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CFMediaPlayer.Services
//{
//    public class ArtistInfoService : XmlEntityWithIdStoreService<ArtistInfo, string>, IArtistInfoService
//    {
//        public ArtistInfoService(string folder) : base(folder, "ArtistInfo.*.xml",
//                                            (artistInfo) => $"ArtistInfo.{artistInfo.Id}.xml",
//                                            (id) => $"ArtistInfo.{id}.xml")
//        {

//        }
//    }
//}
