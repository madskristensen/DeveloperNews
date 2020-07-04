using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using DeveloperNews.Resources;
using Microsoft.VisualStudio.Shell;

namespace DeveloperNews.ToolWindows
{
    public partial class NewsWindowControl : UserControl
    {
        public NewsWindowControl(SyndicationFeed feed)
        {
            InitializeComponent();
            BindPosts(feed);

            cbOpenInVS.IsChecked = GeneralOptions.Instance.OpenInDefaultBrowser;
        }

        private void BindSettings()
        {
            var selection = GeneralOptions.Instance.FeedSelection.Split(new[] { ';' }, StringSplitOptions.RemoveEmptyEntries);

            foreach (FeedManager.FeedInfo feedInfo in FeedStore.FeedInfos.OrderBy(f => f.Name))
            {
                var cb = new CheckBox
                {
                    Content = feedInfo.Name,
                    Padding = new Thickness(5, 2, 0, 2),
                };

                cb.IsChecked = !selection.Any() || selection.Contains($"{feedInfo.Name}:true", StringComparer.OrdinalIgnoreCase);
                cb.Click += FeedSelectionClick;

                pnlFeedSelection.Children.Add(cb);
            }
        }

        private void FeedSelectionClick(object sender, RoutedEventArgs e)
        {
            var selection = new List<string>();

            foreach (CheckBox cb in pnlFeedSelection.Children.Cast<CheckBox>())
            {
                var isChecked = cb.IsChecked.Value.ToString().ToLowerInvariant();
                selection.Add($"{cb.Content}:{isChecked}");
            }

            var setting = string.Join(";", selection);
            GeneralOptions.Instance.FeedSelection = setting;
            GeneralOptions.Instance.Save();
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
                Opacity = 0.7,
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
            ThreadHelper.JoinableTaskFactory.RunAsync(async () =>
            {
                SyndicationFeed feed = await DeveloperNewsPackage.Store.GetFeedAsync(true);

                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                BindPosts(feed);
            });
        }

        private void CrispImage_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (pnlFeedSelection.Children.Count == 0)
            {
                BindSettings();
            }

            pnlSettings.Visibility = pnlSettings.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;

            if (pnlSettings.Visibility == Visibility.Collapsed)
            {
                RefreshClick(this, null);
            }
        }

        private void OpenInVS_Click(object sender, RoutedEventArgs e)
        {
            GeneralOptions.Instance.OpenInDefaultBrowser = cbOpenInVS.IsChecked.Value;
            GeneralOptions.Instance.Save();
        }
    }
}