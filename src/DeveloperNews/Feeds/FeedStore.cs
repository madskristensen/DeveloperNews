using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace DeveloperNews
{
    public class FeedStore
    {
        private readonly RegistryKey _rootKey;

        public FeedStore(RegistryKey rootKey)
        {
            _rootKey = rootKey;
        }

        public IEnumerable<FeedInfo> FeedInfos { get; set; }

        public async Task<SyndicationFeed> GetFeedAsync(bool force = false)
        {
            var orchestrator = new FeedOrchestrator(Vsix.Name, Vsix.Description);
            FeedInfos = GetFeedInfos();

            return await orchestrator.GetFeedsAsync(FeedInfos.Where(f => f.IsSelected), force);
        }

        public void SaveSelection()
        {
            var sb = new StringBuilder();

            foreach (FeedInfo feedInfo in FeedInfos)
            {
                sb.AppendLine(feedInfo.ToString());
            }

            Options.Instance.FeedSelection = sb.ToString();
            Options.Instance.Save();
        }

        private IEnumerable<FeedInfo> GetFeedInfos()
        {
            using (RegistryKey key = _rootKey.OpenSubKey("DeveloperNews\\Feeds"))
            {
                var names = key.GetValueNames();

                foreach (var name in names)
                {
                    var feedInfo = new FeedInfo
                    {
                        Name = name,
                        Url = key.GetValue(name)?.ToString()
                    };

                    feedInfo.IsSelected = CheckIfSelected(feedInfo);
                    yield return feedInfo;
                }
            }
        }

        private bool CheckIfSelected(FeedInfo feedInfo)
        {
            var raw = Environment.NewLine + Options.Instance.FeedSelection;

            if (string.IsNullOrEmpty(raw) || raw.IndexOf(feedInfo.Name, StringComparison.OrdinalIgnoreCase) == -1)
            {
                return true;
            }

            return raw.IndexOf($"{Environment.NewLine}{feedInfo.Name}:true", StringComparison.OrdinalIgnoreCase) > -1;
        }
    }
}
