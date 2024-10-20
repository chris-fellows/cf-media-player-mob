using CFMediaPlayer.Interfaces;
using CFMediaPlayer.Models;
using CFMediaPlayer.Utilities;

namespace CFMediaPlayer.Services
{
    public class StreamSourceService : IStreamSourceService
    {
        private readonly string _folder;

        public StreamSourceService(string folder)
        {
            _folder = folder;
        }

        public List<MediaItem> GetAll()
        {
            var file = Path.Combine(_folder, "StreamSources.xml");
            if (File.Exists(file))
            {
                var mediaItems = XmlUtilities.DeserializeFromString<List<MediaItem>>(File.ReadAllText(file));
                return mediaItems;
            }
            return new();
        }

        public void LoadDefaults()
        {
            var mediaItems = new List<MediaItem>();

            mediaItems.Add(new MediaItem()
            {
                Name = "BBC Radio 1",
                FilePath = "https://as-hls-ww.live.cf.md.bbci.co.uk/pool_904/live/ww/bbc_radio_one/bbc_radio_one.isml/bbc_radio_one-audio%3d96000.norewind.m3u8",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "BBC Radio 2",
                FilePath = "https://as-hls-ww.live.cf.md.bbci.co.uk/pool_904/live/ww/bbc_radio_two/bbc_radio_two.isml/bbc_radio_two-audio%3d96000.norewind.m3u8",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "BBC Radio 3",
                FilePath = "https://as-hls-ww-live.akamaized.net/pool_904/live/ww/bbc_radio_three/bbc_radio_three.isml/bbc_radio_three-audio%3d96000.norewind.m3u8",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "BBC Radio 4",
                FilePath = "https://as-hls-ww-live.akamaized.net/pool_904/live/ww/bbc_radio_fourfm/bbc_radio_fourfm.isml/bbc_radio_fourfm-audio%3d96000.norewind.m3u8",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "BBC Radio 4 Extra",
                FilePath = "https://as-hls-ww.live.cf.md.bbci.co.uk/pool_904/live/ww/bbc_radio_four_extra/bbc_radio_four_extra.isml/bbc_radio_four_extra-audio%3d96000.norewind.m3u8",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "BBC Radio 5 Live",
                FilePath = "https://as-hls-ww.live.cf.md.bbci.co.uk/pool_904/live/ww/bbc_radio_five_live/bbc_radio_five_live.isml/bbc_radio_five_live-audio%3d96000.norewind.m3u8",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "Kerrang! Radio",
                FilePath = "https://stream-al.hellorayo.co.uk/kerrang.mp3",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "Magic Radio",
                FilePath = "https://stream-mz.hellorayo.co.uk/magicnational.mp3",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "Planet Rock",
                FilePath = "https://stream-mz.hellorayo.co.uk/planetrock.mp3",
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "Talk Radio",
                FilePath = "https://radio.talkradio.co.uk/stream"
            });
            mediaItems.Add(new MediaItem()
            {
                Name = "talkSPORT",
                FilePath = "https://radio.talksport.com/stream"
            });
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});
            //mediaItems.Add(new MediaItem()
            //{
            //    Name = "",
            //    FilePath = ""
            //});

            Save(mediaItems);
        }

        public void Save(List<MediaItem> mediaItems)
        {
            var file = Path.Combine(FileSystem.AppDataDirectory, "StreamSources.xml");
            File.WriteAllText(file, XmlUtilities.SerializeToString(mediaItems), System.Text.Encoding.UTF8);
        }
    }
}
