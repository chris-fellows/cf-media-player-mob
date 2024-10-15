using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;

namespace CFMediaPlayer.Services
{
    public class MediaSearchService : IMediaSearchService
    {
        private readonly IMediaSourceService _mediaSourceService;

        public MediaSearchService(IMediaSourceService mediaSourceService)
        {
            _mediaSourceService = mediaSourceService;
        }

        public Task<List<SearchResult>> SearchAsync(SearchOptions searchOptions)
        {
            return Task.Factory.StartNew(() =>
            {               
                // Start task to search each media location
                var tasks = new List<Task<List<SearchResult>>>();
                foreach (var mediaLocation in searchOptions.MediaLocations)
                {
                    var task = Task.Factory.StartNew(() =>
                    {
                        // Get media source to process this media location                
                        var mediaSource = _mediaSourceService.GetAll().First(ms => ms.MediaLocation.Name == mediaLocation.Name);

                        var searchResultsML = mediaSource.Search(searchOptions);
                        searchResultsML.ForEach(sr => sr.MediaLocationName = mediaLocation.Name);   // Set media location
                        return searchResultsML;
                    });
                    tasks.Add(task);
                }

                // Wait for tasks to complete
                Task.WaitAll(tasks.ToArray());

                // Collect results
                var searchResults = new List<SearchResult>();
                foreach(var task in tasks)
                {
                    if (task.IsFaulted)
                    {
                        searchResults.Add(new SearchResult()
                        {
                            Name = task.Exception.Message                            
                        });
                    }
                    else
                    {
                        searchResults.AddRange(task.Result);
                    }
                }

                /*
                tasks.Where(task => !task.IsFaulted).ToList()
                      .ForEach(task => searchResults.AddRange(task.Result));
                */

                // Sort alphabetical order
                searchResults = searchResults.OrderBy(sr => sr.Name).ToList();

                return searchResults;
            });
        }
    }
}
