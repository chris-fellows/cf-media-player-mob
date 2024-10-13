using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CFMediaPlayer.Services
{
    public class MediaSearchService : IMediaSearchService
    {
        private readonly IMediaLocationService _mediaLocationService;
        private readonly IEnumerable<IMediaSource> _mediaSources;

        public MediaSearchService(IMediaLocationService mediaLocationService,
                        IEnumerable<IMediaSource> mediaSources)
        {
            _mediaLocationService = mediaLocationService;
            _mediaSources = mediaSources;
        }

        public Task<List<SearchResult>> Search(SearchOptions searchOptions)
        {
            // TODO: Make this async
            var searchResults = new List<SearchResult>();
           
            var mediaLocations = _mediaLocationService.GetAll();

            foreach(var mediaLocation in mediaLocations)
            {
                //var task = Task.Factory.StartNew(() =>
                //{

                //});

                // Get media source to process this media location
                var mediaSource = _mediaSources.First(ms => ms.MediaSourceType == mediaLocation.MediaSourceType);

                mediaSource.SetSource(mediaLocation.RootFolderPath);

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
