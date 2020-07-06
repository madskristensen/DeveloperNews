using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DeveloperNews.ToolWindows
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }

        public void BindSettings()
        {
            cbOpenInVS.IsChecked = Options.Instance.OpenInDefaultBrowser;

            pnlFeedSelection.Children.Clear();

            foreach (FeedInfo feedInfo in DeveloperNewsPackage.Store.FeedInfos.OrderBy(f => f.Name))
            {
                var cb = new CheckBox
                {
                    Content = feedInfo.Name,
                    Padding = new Thickness(5, 2, 0, 2),
                    IsChecked = feedInfo.IsSelected,
                    Tag = feedInfo,
                };

                pnlFeedSelection.Children.Add(cb);
            }
        }

        private void SaveClick(object sender, RoutedEventArgs e)
        {
            var feedInfos = new List<FeedInfo>();

            foreach (CheckBox cb in pnlFeedSelection.Children.Cast<CheckBox>())
            {
                var feedInfo = cb.Tag as FeedInfo;

                feedInfo.IsSelected = cb.IsChecked.Value;
                feedInfos.Add(feedInfo);
            }

            // Set selected feeds
            DeveloperNewsPackage.Store.FeedInfos = feedInfos;
            DeveloperNewsPackage.Store.SaveSelection();

            // Set default opening behavior
            Options.Instance.OpenInDefaultBrowser = cbOpenInVS.IsChecked.Value;
            Options.Instance.Save();

            CancelClick(sender, e);

            Saved?.Invoke(this, EventArgs.Empty);
        }

        private void CancelClick(object sender, RoutedEventArgs e)
        {
            Visibility = Visibility.Collapsed;
        }

        public event EventHandler Saved;
    }
}
