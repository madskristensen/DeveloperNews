using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace DeveloperNews
{
	public class FeedOrchestrator
	{
		private static readonly string _folder = Path.Combine(Path.GetTempPath(), Vsix.Name);
		private static readonly string _combinedFile = Path.Combine(_folder, "_feed.xml");

		public async Task<SyndicationFeed> GetFeedsAsync(IEnumerable<FeedInfo> feedInfos, bool force = false)
		{
			if (!force && File.Exists(_combinedFile) && File.GetLastWriteTime(_combinedFile) >= DateTime.Now.AddHours(-4))
			{
				using (XmlReader reader = XmlReader.Create(_combinedFile))
				{
					return SyndicationFeed.Load(reader);
				}
			}

			return await CreateNewCombinedFeedAsync(feedInfos, force);
		}

		private async Task<SyndicationFeed> CreateNewCombinedFeedAsync(IEnumerable<FeedInfo> feedInfos, bool force)
		{
			var downloader = new FeedDownloader(_folder);
			var feed = new SyndicationFeed(Vsix.Name, Vsix.Description, null);

			foreach (var feedInfo in feedInfos)
			{
				var fetchedFeed = await downloader.FetchAsync(feedInfo, force);

				feed.Items = feed.Items
					.Union(fetchedFeed.Items)
					.GroupBy(i => i.Title.Text)
					.Select(i => i.First())
					.OrderByDescending(i => i.PublishDate.Date);
			}

			Directory.CreateDirectory(_folder);

			using (XmlWriter writer = XmlWriter.Create(_combinedFile))
			{
				feed.Items = feed.Items.Take(20);
				feed.SaveAsRss20(writer);
			}

			return feed;
		}
	}
}
