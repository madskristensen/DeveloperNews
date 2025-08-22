using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Windows;
using System.Xml;

namespace DevNews
{
    /// <summary>
    /// Is responsible for creating a single combined feed out of the individually selected feeds.
    /// </summary>
    public class FeedOrchestrator
    {
        private readonly string _folder;
        private readonly string _combinedFile;

        private readonly string _name;
        private readonly string _description;
        private DateTime _lastModified;

        public FeedOrchestrator(string name, string description)
        {
            _name = name;
            _description = description;

            _folder = Path.Combine(Path.GetTempPath(), name);
            _combinedFile = Path.Combine(_folder, "_feed.xml");
        }

        /// <summary>
        /// Acquires and combines the feeds into a single main feed.
        /// </summary>
        public async Task<SyndicationFeed> GetFeedAsync(IEnumerable<FeedInfo> feedInfos, bool force = false)
        {
            if (feedInfos == null)
            {
                return new SyndicationFeed(_name, _description, null);
            }

            if (File.Exists(_combinedFile))
            {
                _lastModified = File.GetLastWriteTimeUtc(_combinedFile);

                if (!force && _lastModified > DateTime.UtcNow.AddHours(-4))
                {
                    using (var reader = XmlReader.Create(_combinedFile))
                    {
                        var feed = SyndicationFeed.Load(reader);
                        feed.LastUpdatedTime = _lastModified;
                        return feed;
                    }
                }
            }

            return await CreateNewCombinedFeedAsync(feedInfos, force);
        }

        /// <summary>
        /// Used by unit tests to clear the cache
        /// </summary>
        public void ClearCache()
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, true);
            }
        }

        private async Task<SyndicationFeed> CreateNewCombinedFeedAsync(IEnumerable<FeedInfo> feedInfos, bool force)
        {
            var downloader = new FeedDownloader(_folder);
            var feed = new SyndicationFeed(_name, _description, null);

            // Download all feeds in parallel for better performance
            var feedInfoList = feedInfos.ToList();
            var downloadTasks = feedInfoList.Select(async feedInfo =>
            {
                var fetchedFeed = await downloader.DownloadAsync(feedInfo, force);
                if (fetchedFeed != null)
                {
                    fetchedFeed.Title = new TextSyndicationContent(feedInfo.DisplayName);
                    
                    // Set source feed for all items
                    foreach (SyndicationItem item in fetchedFeed.Items)
                    {
                        item.SourceFeed = fetchedFeed;
                    }
                }
                return fetchedFeed;
            });

            // Wait for all downloads to complete
            var fetchedFeeds = await Task.WhenAll(downloadTasks);

            // Combine all successful feed items
            var allItems = new List<SyndicationItem>();
            foreach (var fetchedFeed in fetchedFeeds.Where(f => f != null))
            {
                allItems.AddRange(fetchedFeed.Items);
            }

            feed.LastUpdatedTime = DateTime.UtcNow;

            // Optimize LINQ operations by combining them and avoiding multiple enumerations
            feed.Items = allItems
                .Where(i => !string.IsNullOrWhiteSpace(i.Title?.Text)) // validation first
                .GroupBy(i => i.Title.Text)
                .Select(g => g.OrderByDescending(i => i.PublishDate).First()) // dedupe and keep latest
                .OrderByDescending(i => i.PublishDate.Date)
                .ToList(); // materialize to avoid multiple enumerations

            Directory.CreateDirectory(_folder);

            using (var writer = XmlWriter.Create(_combinedFile))
            {
                feed.Items = feed.Items.Take(100);
                feed.SaveAsRss20(writer);
            }

            if (Application.Current != null) // if running in VS and not in unit test
            {
                Options options = await Options.GetLiveInstanceAsync();
                var newPosts = feed.Items.Where(i => i.PublishDate > options.LastRead).Count();
                options.UnreadPosts += newPosts;
                await options.SaveAsync();

                FeedUpdated?.Invoke(this, options.UnreadPosts);
            }

            return feed;
        }

        public static event EventHandler<int> FeedUpdated;
    }
}
