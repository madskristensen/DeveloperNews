﻿using System;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace FeedManager
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
			string file = Path.Combine(_folder, feedInfo.Name + ".xml");
			var lastModified = File.Exists(file) ? File.GetLastWriteTime(file) : DateTime.MinValue;

			if (force || lastModified < DateTime.Now.AddHours(-4))
			{
				var feed = await DownloadFeedAsync(feedInfo.Url, lastModified);

				if (feed != null)
				{
					Directory.CreateDirectory(_folder);

					using (XmlWriter writer = XmlWriter.Create(file))
					{
						feed.Items = feed.Items.Take(20);
						feed.SaveAsRss20(writer);
					}

					File.SetLastWriteTime(file, feed.LastUpdatedTime.DateTime);

					return feed;
				}
			}

			if (File.Exists(file))
			{
				using (XmlReader reader = XmlReader.Create(file))
				{
					return SyndicationFeed.Load(reader);
				}
			}

			return null;
		}

		private async Task<SyndicationFeed> DownloadFeedAsync(string url, DateTime lastModified)
		{
			try
			{
				using (var client = new HttpClient())
				{
					client.DefaultRequestHeaders.IfModifiedSince = lastModified;
					var result = await client.GetAsync(url);

					if (result.IsSuccessStatusCode)
					{
						var stream = await result.Content.ReadAsStreamAsync();

						using (var reader = XmlReader.Create(stream))
						{
							var feed = SyndicationFeed.Load(reader);

							if (result.Content.Headers.TryGetValues("last-modified", out var values))
							{
								feed.LastUpdatedTime = DateTime.Parse(values.First());
							}

							return feed;
						}
					}
				}
			}
			catch (Exception ex)
			{
				System.Diagnostics.Debug.Write(ex);
			}

			return null;
		}
	}
}