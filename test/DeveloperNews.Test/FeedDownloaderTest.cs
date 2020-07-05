using System;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DeveloperNews.Test
{
    [TestClass]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "<Pending>")]
    public class FeedDownloaderTest
    {
        private static readonly string _folder = Path.Combine(Path.GetTempPath(), "FeedManagerTest");

        [TestInitialize]
        public void Setup()
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, true);
            }
        }

        [TestMethod]
        [DataRow("VS blog", "https://devblogs.microsoft.com/visualstudio/rss")]
        [DataRow("hanselman", "http://feeds.hanselman.com/ScottHanselman")]
        public async Task FetchAsyncOnline(string name, string url)
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = name,
                Url = url
            };

            SyndicationFeed feed = await downloader.FetchAsync(feedInfo, false);

            Assert.IsTrue(feed.Items.Count() > 0);
        }

        [TestMethod]
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

            Assert.AreEqual(lastModified, lastModified2);
        }

        [TestMethod]
        public async Task FetchAsync404()
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = "Doesn't exist",
                Url = "https://fail.example.com"
            };

            SyndicationFeed feed = await downloader.FetchAsync(feedInfo, false);

            Assert.IsNull(feed);
        }

        [TestMethod]
        public async Task FetchAsyncInvalidRss()
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = "Doesn't exist",
                Url = "https://example.com"
            };

            SyndicationFeed feed = await downloader.FetchAsync(feedInfo, false);

            Assert.IsNull(feed);
        }

        [TestMethod]
        public async Task FetchAsyncEmptyUrl()
        {
            var downloader = new FeedDownloader(_folder);
            var feedInfo = new FeedInfo
            {
                Name = "Doesn't exist",
                Url = ""
            };

            SyndicationFeed feed = await downloader.FetchAsync(feedInfo, false);

            Assert.IsNull(feed);
        }
    }
}
