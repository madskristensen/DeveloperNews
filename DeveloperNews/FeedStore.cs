using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace DeveloperNews
{
	public class FeedStore
	{
		private static readonly string _folder = Path.Combine(Path.GetTempPath(), Vsix.Name);
		private static readonly string _master = Path.Combine(_folder, "master.xml");
		private static readonly string _feed = Path.Combine(_folder, "feed.xml");

		public async Task<SyndicationFeed> GetFeedAsync()
		{
			if (!File.Exists(_feed))
			{
				await CreateFeedAsync();
			}

			if (File.Exists(_feed))
			{
				using (XmlReader reader = XmlReader.Create(_feed))
				{
					return SyndicationFeed.Load(reader);
				}
			}

			return null;
		}

		private async Task CreateFeedAsync()
		{
			SyndicationFeed rss = new SyndicationFeed(Vsix.Name, Vsix.Description, null);

			foreach (string key in new List<string> { "https://go.microsoft.com/fwlink/?linkid=2066144" })
			{
				SyndicationFeed feed = await DownloadFeedAsync(key);
				rss.Items = rss.Items.Union(feed.Items).GroupBy(i => i.Title.Text).Select(i => i.First()).OrderByDescending(i => i.PublishDate.Date);
			}

			Directory.CreateDirectory(_folder);

			using (XmlWriter writer = XmlWriter.Create(_master))
			{
				rss.SaveAsRss20(writer);
			}

			using (XmlWriter writer = XmlWriter.Create(_feed))
			{
				rss.Items = rss.Items.Take(10);
				rss.SaveAsRss20(writer);
			}
		}

		private async Task<SyndicationFeed> DownloadFeedAsync(string url)
		{
			try
			{
				using (WebClient client = new WebClient())
				{
					Stream stream = await client.OpenReadTaskAsync(url);
					return SyndicationFeed.Load(XmlReader.Create(stream));
				}
			}
			catch (Exception ex)
			{
				Debug.Write(ex);
				return new SyndicationFeed();
			}
		}
	}
}
