using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;
using System.Xml;

namespace FeedManager
{
	public class FeedOrchestrator
	{
		private readonly string _folder;
		private readonly string _combinedFile;
		
		private readonly string _name;
		private readonly string _description;

		public FeedOrchestrator(string name, string description)
		{
			_name = name;
			_description = description;

			_folder = Path.Combine(Path.GetTempPath(), name);
			_combinedFile= Path.Combine(_folder, "_feed.xml");
		}

		public async Task<SyndicationFeed> GetFeedsAsync(IEnumerable<FeedInfo> feedInfos, bool force = false)
		{
			if (feedInfos == null)
			{
				return new SyndicationFeed(_name, _description, null);
			}

			if (!force && 
				File.Exists(_combinedFile) && 
				File.GetLastWriteTime(_combinedFile) >= DateTime.Now.AddHours(-4))
			{
				using (XmlReader reader = XmlReader.Create(_combinedFile))
				{
					return SyndicationFeed.Load(reader);
				}
			}

			return await CreateNewCombinedFeedAsync(feedInfos, force);
		}

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
