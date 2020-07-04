using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace FeedManager.Test
{
    public class FeedDownloaderTest : IDisposable
    {
        private static readonly string _folder = Path.Combine(Path.GetTempPath(), "FeedManagerTest");

        public FeedDownloaderTest()
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, true);
            }
        }

        [Theory]
        [InlineData("VS blog", "https://devblogs.microsoft.com/visualstudio/rss")]
        [InlineData("hanselman", "http://feeds.hanselman.com/ScottHanselman")]
        public async Task FetchAsyncOnline(string name, string url)
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = name,
                Url = url
            };

            System.ServiceModel.Syndication.SyndicationFeed feed = await downloader.FetchAsync(feedInfo, false);

            Assert.True(feed.Items.Count() > 0);
        }

        [Fact]
        public async Task FetchAsyncFromCache()
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = "Visual Studio Blog",
                Url = "https://devblogs.microsoft.com/visualstudio/rss"
            };

            _ = await downloader.FetchAsync(feedInfo, false);

            var file = Path.Combine(_folder, feedInfo.Name + ".xml");
            DateTime lastModified = File.GetLastWriteTime(file);

            _ = await downloader.FetchAsync(feedInfo, false);
            DateTime lastModified2 = File.GetLastWriteTime(file);

            Assert.Equal(lastModified, lastModified2);
        }

        [Fact]
        public async Task FetchAsync404()
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = "Doesn't exist",
                Url = "https://fail.example.com"
            };

            System.ServiceModel.Syndication.SyndicationFeed feed = await downloader.FetchAsync(feedInfo, false);

            Assert.Null(feed);
        }

        [Fact]
        public async Task FetchAsyncInvalidRss()
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = "Doesn't exist",
                Url = "https://example.com"
            };

            System.ServiceModel.Syndication.SyndicationFeed feed = await downloader.FetchAsync(feedInfo, false);

            Assert.Null(feed);
        }

        [Fact]
        public async Task FetchAsyncEmptyUrl()
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = "Doesn't exist",
                Url = ""
            };

            System.ServiceModel.Syndication.SyndicationFeed feed = await downloader.FetchAsync(feedInfo, false);

            Assert.Null(feed);
        }

        public void Dispose()
        {

        }
    }
}
