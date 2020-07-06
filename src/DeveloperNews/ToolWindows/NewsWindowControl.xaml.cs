using System;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using DevNews.Resources;
using Microsoft.VisualStudio.Shell;

namespace DevNews.ToolWindows
{
    public partial class NewsWindowControl : UserControl
    {
        public NewsWindowControl(SyndicationFeed feed)
        {
            InitializeComponent();
            BindPosts(feed);
        }

        private void SettingsSaved(object sender, EventArgs e)
        {
            RefreshClick(this, null);
        }

        private void BindPosts(SyndicationFeed feed)
        {
            pnlPosts.Children.Clear();
            var currentTime = Timestamp(feed.Items.FirstOrDefault());

            if (!string.IsNullOrEmpty(currentTime))
            {
                AddTimeLabel(currentTime);

                foreach (SyndicationItem item in feed.Items)
                {
                    var newTime = Timestamp(item);

                    if (newTime != currentTime)
                    {
                        AddTimeLabel(newTime);
                        currentTime = newTime;
                    }

                    var post = new PostControl(item);
                    pnlPosts.Children.Add(post);
                }
            }

            lblTotal.Content = string.Format(Text.Totalcount, feed.Items.Count());
        }

        private void AddTimeLabel(string timestamp)
        {
            var time = new Label
            {
                Content = timestamp,
                FontWeight = FontWeights.Medium,
                Opacity = 0.6,
                Margin = new Thickness(0, 3, 0, 0),
            };

            pnlPosts.Children.Add(time);
        }

        private static string Timestamp(SyndicationItem item)
        {
            if (item == null)
            {
                return null;
            }

            if (item.PublishDate.Date >= DateTime.Now.AddDays(-1))
            {
                return Text.Timeline_today;
            }
            else if (item.PublishDate.Date >= DateTime.Now.AddDays(-2))
            {
                return Text.Timeline_today;
            }
            else if (item.PublishDate.Date >= DateTime.Now.AddDays(-7))
            {
                return Text.Timeline_pastweek;
            }

            return Text.Timeline_older;
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            prsLoader.Visibility = Visibility.Visible;
            lnkRefresh.IsEnabled = false;

            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                try
                {
                    SyndicationFeed feed = await DeveloperNewsPackage.Store.GetFeedAsync(true);

                    await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                    BindPosts(feed);
                }
                finally
                {
                    prsLoader.Visibility = Visibility.Hidden;
                    lnkRefresh.IsEnabled = true;
                }
            });
        }

        private void OpenSettings(object sender, RoutedEventArgs e)
        {
            ctrlSettings.Visibility = ctrlSettings.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            if (ctrlSettings.Visibility == Visibility.Visible)
            {
                ctrlSettings.BindSettings();
            }
        }
    }
}