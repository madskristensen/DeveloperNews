using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace DevNews.ToolWindows
{
    public partial class SettingsControl : UserControl
    {
        public SettingsControl()
        {
            InitializeComponent();
        }

        public void BindSettings()
        {
            pnlFeedSelection.Children.Clear();

            foreach (FeedInfo feedInfo in DeveloperNewsPackage.Store.FeedInfos.OrderBy(f => f.Name.TrimStart('?')))
            {
                var cb = new CheckBox
                {
                    Content = feedInfo.DisplayName,
                    Padding = new Thickness(5, 2, 0, 2),
                    IsChecked = feedInfo.IsSelected,
                    Tag = feedInfo,
                    IsEnabled = feedInfo.Name[0] != '!'
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

            // Save selection and Options
            DeveloperNewsPackage.Store.SaveSelection();

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
