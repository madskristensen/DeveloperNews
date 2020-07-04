using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using Xunit;

namespace FeedManager.Test
{
    public class FeedOrchestratorTest
    {
        [Fact]
        public async Task FetchAsyncOnline()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            var feedInfos = new List<FeedInfo> {
                new FeedInfo { Name = "test1", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                new FeedInfo { Name = "test2", Url = "http://feeds.hanselman.com/ScottHanselman"}
            };

            System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

            Assert.Equal(18, feed.Items.Count());
        }

        [Fact]
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

                Assert.Equal(18, feed.Items.Count());
            }
        }

        [Fact]
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

                Assert.Equal(10, feed.Items.Count());
            }
        }

        [Fact]
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

                Assert.Equal(10, feed.Items.Count());
            }
        }

        [Fact]
        public async Task FetchAsyncEmptyList()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            var feedInfos = new List<FeedInfo>();
            System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

            Assert.Empty(feed.Items);
            Assert.Equal("Name", feed.Title.Text);
        }

        [Fact]
        public async Task FetchAsyncEmptyListForce()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            var feedInfos = new List<FeedInfo>();
            System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(feedInfos, false);

            Assert.Empty(feed.Items);
            Assert.Equal("Name", feed.Title.Text);
        }

        [Fact]
        public async Task FetchAsyncNullList()
        {
            var orchestrator = new FeedOrchestrator("Name", "Description");
            orchestrator.ClearCache();

            System.ServiceModel.Syndication.SyndicationFeed feed = await orchestrator.GetFeedsAsync(null, false);

            Assert.Empty(feed.Items);
            Assert.Equal("Name", feed.Title.Text);
        }
    }
}
