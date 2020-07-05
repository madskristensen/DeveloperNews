using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace FeedManager.Test
{
    public class FeedSelectorTest
    {
        [Fact]
        public void GenerateRawSelectionSetting()
        {
            var selector = new FeedSelector("");

            var feedInfos = new List<FeedInfo> {
                new FeedInfo { Name = "test1", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                new FeedInfo { Name = "test2", Url = "http://feeds.hanselman.com/ScottHanselman", IsSelected = true}
            };

            var result = selector.GenerateRawSelectionSetting(feedInfos);

            Assert.Contains("test1:False", result);
            Assert.Contains("test2:True", result);
        }

        [Fact]
        public void LoadSelectionData()
        {
            var selector = new FeedSelector("test1:true\n\rtest2:false");

            var feedInfos = new List<FeedInfo> {
                new FeedInfo { Name = "test1", Url = "https://devblogs.microsoft.com/visualstudio/rss"},
                new FeedInfo { Name = "test2", Url = "http://feeds.hanselman.com/ScottHanselman"}
            };

            IEnumerable<FeedInfo> result = selector.LoadSelectionData(feedInfos);

            Assert.True(result.ElementAt(0).IsSelected);
            Assert.False(result.ElementAt(1).IsSelected);
        }
    }
}
