using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class MediaSearchService : IMediaSearchService
    {        
        private readonly IEnumerable<IMediaSource> _mediaSources;

        public MediaSearchService(IEnumerable<IMediaSource> mediaSources)
        {     
            _mediaSources = mediaSources;
        }

        public Task<List<SearchResult>> Search(SearchOptions searchOptions)
        {
            // TODO: Make this async
            var searchResults = new List<SearchResult>();
                       
            foreach(var mediaLocation in searchOptions.MediaLocations)
            {
                //var task = Task.Factory.StartNew(() =>
                //{

                //});

                // Get media source to process this media location
                var mediaSource = _mediaSources.First(ms => ms.MediaLocation.Name == mediaLocation.Name);

                //mediaSource.SetSource(mediaLocation.Source);

                var searchResultsML = mediaSource.Search(searchOptions);

                searchResultsML.ForEach(sr => sr.MediaLocationName = mediaLocation.Name);   // Set media location
                searchResults.AddRange(searchResultsML);
            }

            // Sort alphabetical order
            searchResults = searchResults.OrderBy(sr => sr.Name).ToList();
            
            return Task.FromResult(searchResults);
        }
    }
}
