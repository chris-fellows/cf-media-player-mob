using CFMediaPlayer.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Services
{
    public class MediaSourceService : IMediaSourceService
    {
        private readonly IEnumerable<IMediaSource> _mediaSources;
        public MediaSourceService(IEnumerable<IMediaSource> mediaSources)
        {
            _mediaSources = mediaSources;
        }

        public List<IMediaSource> GetAll()
        {
            return _mediaSources.ToList();
        }
    }
}
