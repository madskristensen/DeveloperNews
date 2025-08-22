using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Win32;

namespace DevNews
{
    /// <summary>
    /// The entry point for the feeds. Handles the custom selection of feeds as well.
    /// </summary>
    public class FeedStore
    {
        private readonly RegistryKey _rootKey;
        private HashSet<string> _selectedFeedNames;
        private string _lastFeedSelectionString;

        // Constants for better maintainability
        private const string FeedsRegistryPath = "DeveloperNews\\Feeds";
        private const char OptionalFeedPrefix = '?';
        private const char DisabledFeedPrefix = '!';
        private const string FeedSelectionSeparator = ":true";

        public FeedStore(RegistryKey rootKey)
        {
            _rootKey = rootKey;
        }

        /// <summary>
        /// The feeds located in the registry.
        /// </summary>
        public IEnumerable<FeedInfo> FeedInfos { get; set; }

        public async Task<SyndicationFeed> GetFeedAsync(bool force = false)
        {
            try
            {
                var orchestrator = new FeedOrchestrator(Vsix.Name, Vsix.Description);
                FeedInfos = GetFeedInfos();

                return await orchestrator.GetFeedAsync(FeedInfos.Where(f => f.IsSelected), force);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>
        /// Saves the feed selection to the registry.
        /// </summary>
        public void SaveSelection()
        {
            var sb = new StringBuilder();

            foreach (FeedInfo feedInfo in FeedInfos)
            {
                sb.AppendLine(feedInfo.ToString());
            }

            Options.Instance.FeedSelection = sb.ToString();
            Options.Instance.Save();
            
            // Invalidate cache when selection changes
            _selectedFeedNames = null;
            _lastFeedSelectionString = null;
        }

        private IEnumerable<FeedInfo> GetFeedInfos()
        {
            using (RegistryKey key = _rootKey.OpenSubKey(FeedsRegistryPath))
            {
                if (key == null)
                {
                    yield break;
                }

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
            if (string.IsNullOrEmpty(feedInfo?.Name))
            {
                return false;
            }

            // Cache the parsed feed selections for better performance
            EnsureFeedSelectionCacheIsValid();

            // If the feed doesn't exist in settings, it's new and should be selected by default
            // Exception: feeds starting with '?' are optional and default to unselected
            if (_selectedFeedNames.Count == 0 || !_selectedFeedNames.Contains(feedInfo.Name))
            {
                return feedInfo.Name[0] != OptionalFeedPrefix;
            }

            // Use cached HashSet lookup instead of string operations
            return _selectedFeedNames.Contains($"{feedInfo.Name}{FeedSelectionSeparator}");
        }

        private void EnsureFeedSelectionCacheIsValid()
        {
            var currentFeedSelection = Options.Instance.FeedSelection;
            
            // Only rebuild cache if the feed selection string has changed
            if (_selectedFeedNames == null || _lastFeedSelectionString != currentFeedSelection)
            {
                _selectedFeedNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                _lastFeedSelectionString = currentFeedSelection;

                if (!string.IsNullOrEmpty(currentFeedSelection))
                {
                    // Parse the feed selection string once and cache the results
                    var lines = currentFeedSelection.Split(new[] { Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (var line in lines)
                    {
                        if (!string.IsNullOrWhiteSpace(line))
                        {
                            _selectedFeedNames.Add(line.Trim());
                            
                            // Also add the feed name separately for existence checks
                            var colonIndex = line.IndexOf(':');
                            if (colonIndex > 0)
                            {
                                var feedName = line.Substring(0, colonIndex).Trim();
                                _selectedFeedNames.Add(feedName);
                            }
                        }
                    }
                }
            }
        }
    }
}
