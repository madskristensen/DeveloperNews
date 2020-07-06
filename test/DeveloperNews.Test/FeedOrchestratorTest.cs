using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace DevNews.Test
{
    [TestClass]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Style", "VSTHRD200:Use \"Async\" suffix for async methods", Justification = "<Pending>")]
    public class FeedOrchestratorTest
    {
        [TestMethod]
        public async Task FetchAsyncOnline()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            var feedInfos = new List<FeedInfo> {
                new FeedInfo { Name = "test1", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                new FeedInfo { Name = "test2", Url = "http://feeds.hanselman.com/ScottHanselman"}
            };

            System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

            Assert.AreEqual(18, feed.Items.Count());
        }

        [TestMethod]
        public async Task FetchAsyncFromCache()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            for (var i = 0; i < 2; i++)
            {
                var feedInfos = new List<FeedInfo> {
                    new FeedInfo { Name = "test1", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                    new FeedInfo { Name = "test2", Url = "http://feeds.hanselman.com/ScottHanselman"}
                };

                System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

                Assert.AreEqual(18, feed.Items.Count());
            }
        }

        [TestMethod]
        public async Task FetchAsyncDuplicatesWithDifferentNames()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            for (var i = 0; i < 2; i++)
            {
                var feedInfos = new List<FeedInfo> {
                    new FeedInfo { Name = "test1", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                    new FeedInfo { Name = "test2", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                };

                System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

                Assert.AreEqual(10, feed.Items.Count());
            }
        }

        [TestMethod]
        public async Task FetchAsyncDuplicatesFromCache()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            for (var i = 0; i < 2; i++)
            {
                var feedInfos = new List<FeedInfo> {
                    new FeedInfo { Name = "test1", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                    new FeedInfo { Name = "test1", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                };

                System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

                Assert.AreEqual(10, feed.Items.Count());
            }
        }

        [TestMethod]
        public async Task FetchAsyncEmptyList()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            var feedInfos = new List<FeedInfo>();
            System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

            Assert.IsTrue(feed.Items.Count() == 0);
            Assert.AreEqual("Name", feed.Title.Text);
        }

        [TestMethod]
        public async Task FetchAsyncEmptyListForce()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            var feedInfos = new List<FeedInfo>();
            System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

            Assert.IsTrue(feed.Items.Count() == 0);
            Assert.AreEqual("Name", feed.Title.Text);
        }

        [TestMethod]
        public async Task FetchAsyncNullList()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(null, false);

            Assert.IsTrue(feed.Items.Count() == 0);
            Assert.AreEqual("Name", feed.Title.Text);
        }
    }
}
