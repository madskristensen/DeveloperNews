using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading;
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

            var feedInfoList = feedInfos.ToList();
            if (!feedInfoList.Any())
            {
                return feed;
            }

            // Use SemaphoreSlim to limit concurrent downloads to prevent overwhelming servers
            const int maxConcurrentDownloads = 6; // Reasonable limit for RSS feeds
            using (var semaphore = new SemaphoreSlim(maxConcurrentDownloads, maxConcurrentDownloads))
            {
                // Create download tasks with concurrency control
                var downloadTasks = feedInfoList.Select(async feedInfo =>
                {
                    await semaphore.WaitAsync();
                    try
                    {
                        var fetchedFeed = await downloader.DownloadAsync(feedInfo, force);
                        if (fetchedFeed?.Items != null)
                        {
                            fetchedFeed.Title = new TextSyndicationContent(feedInfo.DisplayName);

                            // Set source feed for all items efficiently
                            foreach (var item in fetchedFeed.Items)
                            {
                                item.SourceFeed = fetchedFeed;
                            }
                        }
                        return fetchedFeed;
                    }
                    finally
                    {
                        semaphore.Release();
                    }
                });

                // Wait for all downloads to complete
                var fetchedFeeds = await Task.WhenAll(downloadTasks);

                // Combine and process all items in a single optimized LINQ chain
                feed.Items = fetchedFeeds
                    .Where(f => f?.Items != null)
                    .SelectMany(f => f.Items)
                    .Where(i => !string.IsNullOrWhiteSpace(i.Title?.Text)) // Filter invalid items first
                    .GroupBy(i => i.Title.Text, StringComparer.OrdinalIgnoreCase) // Case-insensitive grouping
                    .Select(g => g.OrderByDescending(i => i.PublishDate).First()) // Keep latest of duplicates
                    .OrderByDescending(i => i.PublishDate.Date)
                    .Take(100) // Limit early to avoid processing unnecessary items
                    .ToList(); // Materialize once
            }

            feed.LastUpdatedTime = DateTime.UtcNow;

            // Ensure directory exists before writing
            Directory.CreateDirectory(_folder);

            // Write to file (removed redundant Take(100))
            using (var writer = XmlWriter.Create(_combinedFile))
            {
                feed.SaveAsRss20(writer);
            }

            // Update unread count if running in VS
            if (Application.Current != null)
            {
                try
                {
                    var options = await Options.GetLiveInstanceAsync();
                    var newPostsCount = feed.Items.Count(i => i.PublishDate > options.LastRead);
                    options.UnreadPosts += newPostsCount;
                    await options.SaveAsync();

                    FeedUpdated?.Invoke(this, options.UnreadPosts);
                }
                catch (Exception ex)
                {
                    // Log error but don't fail the entire operation
                    Trace.TraceError($"Failed to update unread count: {ex.Message}");
                }
            }

            return feed;
        }

        public static event EventHandler<int> FeedUpdated;
    }
}
