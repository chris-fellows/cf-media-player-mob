using CFMediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Media search
    /// </summary>
    public interface IMediaSearchService
    {
        Task<List<SearchResult>> Search(SearchOptions searchOptions);
    }
}
