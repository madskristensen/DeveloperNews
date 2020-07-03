using Microsoft.Win32;

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
		private static readonly string _feed = Path.Combine(_folder, "feed.xml");
		private readonly RegistryKey _rootKey;

		public FeedStore(RegistryKey rootKey)
		{
			_rootKey = rootKey;
		}

		public async Task<SyndicationFeed> GetFeedAsync(bool forceOnline = false)
		{
			if (forceOnline || !File.Exists(_feed) || File.GetLastWriteTime(_feed) > DateTime.Now.AddHours(4))
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
			var urls = GetFeedUrls();

			foreach (string key in urls)
			{
				SyndicationFeed feed = await DownloadFeedAsync(key);

				rss.Items = rss.Items
					.Union(feed.Items)
					.GroupBy(i => i.Title.Text)
					.Select(i => i.First())
					.OrderByDescending(i => i.PublishDate.Date);
			}

			Directory.CreateDirectory(_folder);

			using (XmlWriter writer = XmlWriter.Create(_feed))
			{
				rss.Items = rss.Items.Take(20);
				rss.SaveAsRss20(writer);
			}
		}

		private IEnumerable<string> GetFeedUrls()
		{
			using (var key = _rootKey.OpenSubKey("DeveloperNews\\Feeds"))
			{
				var names = key.GetValueNames();
				foreach (var name in names)
				{
					yield return key.GetValue(name)?.ToString();
				}
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
