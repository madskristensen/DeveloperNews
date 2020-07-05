using System;
using System.Collections.Generic;
using System.Text;

namespace FeedManager
{
    public class FeedSelector
    {
        private string _rawSettings;

        public FeedSelector(string rawSettings)
        {
            _rawSettings = rawSettings;
        }

        public IEnumerable<FeedInfo> LoadSelectionData(IEnumerable<FeedInfo> feedInfos)
        {
            foreach (FeedInfo feedInfo in feedInfos)
            {
                if (string.IsNullOrEmpty(_rawSettings) || _rawSettings.IndexOf(feedInfo.Name, StringComparison.OrdinalIgnoreCase) == -1)
                {
                    feedInfo.IsSelected = true;
                }
                else
                {
                    feedInfo.IsSelected = _rawSettings.IndexOf($"{feedInfo.Name}:true", StringComparison.OrdinalIgnoreCase) > -1;
                }

                yield return feedInfo;
            }
        }

        public string GenerateRawSelectionSetting(IEnumerable<FeedInfo> feedInfos)
        {
            var sb = new StringBuilder();

            foreach (FeedInfo feedInfo in feedInfos)
            {
                sb.AppendLine(feedInfo.ToString());
            }

            _rawSettings = sb.ToString();

            return _rawSettings;
        }
    }
}
