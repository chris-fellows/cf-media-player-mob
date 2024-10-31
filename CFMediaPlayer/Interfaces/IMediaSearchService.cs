using CFMediaPlayer.Models;

namespace CFMediaPlayer.Interfaces
{
    /// <summary>
    /// Media search
    /// </summary>
    public interface IMediaSearchService
    {
        /// <summary>
        /// Searches asynchronously
        /// </summary>
        /// <param name="searchOptions"></param>
        /// <returns></returns>
        Task<List<SearchResult>> SearchAsync(SearchOptions searchOptions, CancellationToken cancellationToken);
    }
}
