using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Threading.Tasks;

using FeedManager;
using Microsoft.Win32;

namespace DeveloperNews
{
    public class FeedStore
    {
        private readonly RegistryKey _rootKey;

        public FeedStore(RegistryKey rootKey)
        {
            _rootKey = rootKey;
            IEnumerable<FeedInfo> feedInfos = GetFeedInfos();

            var selector = new FeedSelector(GeneralOptions.Instance.FeedSelection);
            FeedInfos = selector.LoadSelectionData(feedInfos).ToArray();
        }

        public IEnumerable<FeedInfo> FeedInfos { get; set; }

        public async Task<SyndicationFeed> GetFeedAsync(bool force = false)
        {
            var orchestrator = new FeedOrchestrator(Vsix.Name, Vsix.Description);

            return await orchestrator.GetFeedsAsync(FeedInfos.Where(f => f.IsSelected), force);
        }

        private IEnumerable<FeedInfo> GetFeedInfos()
        {
            using (RegistryKey key = _rootKey.OpenSubKey("DeveloperNews\\Feeds"))
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
