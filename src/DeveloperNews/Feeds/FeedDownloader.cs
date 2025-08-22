using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace DevNews
{
    /// <summary>
    /// Downloads the individual feeds to disk
    /// </summary>
    public class FeedDownloader
    {
        private readonly string _folder;
        
        // Shared HttpClient instance for better performance and socket reuse
        private static readonly HttpClient _httpClient = new HttpClient();

        static FeedDownloader()
        {
            // Configure the shared HttpClient with common settings
            _httpClient.DefaultRequestHeaders.Add("User-Agent", "Developer News for Visual Studio");
            _httpClient.Timeout = TimeSpan.FromSeconds(30); // Set a reasonable timeout
        }

        public FeedDownloader(string folder)
        {
            _folder = folder;
        }

        /// <summary>
        /// Downloads the feed
        /// </summary>
        /// <param name="feedInfo">Contains information needed to download the feed.</param>
        /// <param name="force">If true, ignores file timestamp and downloads the latest feed.</param>
        public async Task<SyndicationFeed> DownloadAsync(FeedInfo feedInfo, bool force = false)
        {
            var file = Path.Combine(_folder, feedInfo.DisplayName + ".xml");
            DateTime lastModified = File.Exists(file) ? File.GetLastWriteTimeUtc(file) : DateTime.UtcNow.AddMonths(-2);

            if (force || lastModified < DateTime.UtcNow.AddHours(-4))
            {
                SyndicationFeed feed = await DownloadUrlAsync(feedInfo.Url, lastModified);

                if (feed != null)
                {
                    WriteFeedToDisk(file, feed);
                    return feed;
                }
            }

            if (File.Exists(file))
            {
                using (var reader = XmlReader.Create(file))
                {
                    return SyndicationFeed.Load(reader);
                }
            }

            return null;
        }

        private void WriteFeedToDisk(string file, SyndicationFeed feed)
        {
            Directory.CreateDirectory(_folder);

            using (var writer = XmlWriter.Create(file))
            {
                // Limit feed items to something reasonable for perf reasons
                feed.Items = feed.Items.Take(20);
                feed.SaveAsRss20(writer);
            }

            if (feed.LastUpdatedTime.DateTime != DateTime.MinValue)
            {
                File.SetLastWriteTimeUtc(file, feed.LastUpdatedTime.DateTime);
            }
        }

        private async Task<SyndicationFeed> DownloadUrlAsync(string url, DateTime lastModified)
        {
            if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
            {
                return null;
            }

            try
            {
                // Create a new request message to set per-request headers
                using (var request = new HttpRequestMessage(HttpMethod.Get, url))
                {
                    request.Headers.IfModifiedSince = lastModified;

                    HttpResponseMessage result = await _httpClient.SendAsync(request);

                    if (result.IsSuccessStatusCode)
                    {
                        return await ProcessResultAsync(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Trace.TraceError(ex.ToString());
            }

            return null;
        }

        private static async Task<SyndicationFeed> ProcessResultAsync(HttpResponseMessage result)
        {
            Stream stream = await result.Content.ReadAsStreamAsync();

            using (var reader = XmlReader.Create(stream))
            {
                var feed = SyndicationFeed.Load(reader);

                if (result.Content.Headers.LastModified.HasValue)
                {
                    feed.LastUpdatedTime = result.Content.Headers.LastModified.Value;
                }

                return feed;
            }
        }
    }
}
