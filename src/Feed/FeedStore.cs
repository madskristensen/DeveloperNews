using Microsoft.Win32;

using System.Collections.Generic;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

namespace DeveloperNews
{
	public class FeedStore
	{
		private readonly RegistryKey _rootKey;

		public FeedStore(RegistryKey rootKey)
		{
			_rootKey = rootKey;
		}

		public async Task<SyndicationFeed> GetFeedAsync(bool force = false)
		{
			var orchestrator = new FeedOrchestrator();
			var feedInfos = GetFeedInfos();

			return await orchestrator.GetFeedsAsync(feedInfos, force);
		}

		private IEnumerable<FeedInfo> GetFeedInfos()
		{
			using (var key = _rootKey.OpenSubKey("DeveloperNews\\Feeds"))
			{
				var names = key.GetValueNames();

				foreach (var name in names)
				{
					yield return new FeedInfo
					{
						Name = name,
						Url = key.GetValue(name)?.ToString()
					};
				}
			}
		}
	}
}
