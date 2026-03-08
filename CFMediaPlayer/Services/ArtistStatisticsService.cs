//using CFMediaPlayer.Interfaces;
//using CFMediaPlayer.Models;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace CFMediaPlayer.Services
//{
//    public class ArtistStatisticsService : XmlEntityWithIdStoreService<ArtistStatistics, string>, IArtistStatisticsService
//    {
//        public ArtistStatisticsService(string folder) : base(folder, "ArtistStatistics.*.xml",
//                                            (artistStatistics) => $"ArtistStatistics.{artistStatistics.ArtistId}.xml",
//                                            (id) => $"ArtistStatistics.{id}.xml")
//        {

//        }
//    }
//}
