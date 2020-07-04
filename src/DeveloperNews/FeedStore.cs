using System;
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
            FeedInfos = GetFeedInfos();
        }

        public static IEnumerable<FeedInfo> FeedInfos { get; private set; }

        public async Task<SyndicationFeed> GetFeedAsync(bool force = false)
        {
            var orchestrator = new FeedOrchestrator(Vsix.Name, Vsix.Description);
            var selection = GeneralOptions.Instance.FeedSelection.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            IEnumerable<FeedInfo> infos = FeedInfos;

            if (selection.Any())
            {
                infos = FeedInfos.Where(f => selection.Contains($"{f.Name}:true", StringComparer.OrdinalIgnoreCase));
            }

            return await orchestrator.GetFeedsAsync(infos, force);
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
