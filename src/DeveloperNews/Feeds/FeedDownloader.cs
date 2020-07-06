﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace DeveloperNews
{
    public class FeedDownloader
    {
        private readonly string _folder;

        public FeedDownloader(string folder)
        {
            _folder = folder;
        }

        public async Task<SyndicationFeed> FetchAsync(FeedInfo feedInfo, bool force = false)
        {
            var file = Path.Combine(_folder, feedInfo.DisplayName + ".xml");
            DateTime lastModified = File.Exists(file) ? File.GetLastWriteTime(file) : DateTime.MinValue;

            if (force || lastModified < DateTime.Now.AddHours(-4))
            {
                SyndicationFeed feed = await DownloadFeedAsync(feedInfo.Url, lastModified);

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
                feed.Items = feed.Items.Take(100);
                feed.SaveAsRss20(writer);
            }

            File.SetLastWriteTime(file, feed.LastUpdatedTime.DateTime);
        }

        private async Task<SyndicationFeed> DownloadFeedAsync(string url, DateTime lastModified)
        {
            try
            {
                using (var client = new HttpClient())
                {
                    client.DefaultRequestHeaders.IfModifiedSince = lastModified;
                    client.DefaultRequestHeaders.Add("User-Agent", "Developer News for Visual Studio");

                    HttpResponseMessage result = await client.GetAsync(url);

                    if (result.IsSuccessStatusCode)
                    {
                        return await ProcessResultAsync(result);
                    }
                }
            }
            catch (Exception ex)
            {
                Debug.Write(ex);
            }

            return null;
        }

        private static async Task<SyndicationFeed> ProcessResultAsync(HttpResponseMessage result)
        {
            Stream stream = await result.Content.ReadAsStreamAsync();

            using (var reader = XmlReader.Create(stream))
            {
                var feed = SyndicationFeed.Load(reader);

                if (result.Content.Headers.TryGetValues("last-modified", out IEnumerable<string> values))
                {
                    feed.LastUpdatedTime = DateTime.Parse(values.First());
                }

                return feed;
            }
        }
    }
}
