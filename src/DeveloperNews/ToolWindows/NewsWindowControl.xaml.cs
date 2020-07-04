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

            if (item.PublishDate.Date >= System.DateTime.Now.AddDays(-1))
            {
                return Text.Timeline_today;
            }
            else if (item.PublishDate.Date >= System.DateTime.Now.AddDays(-2))
            {
                return Text.Timeline_today;
            }
            else if (item.PublishDate.Date >= System.DateTime.Now.AddDays(-7))
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
            pnlSettings.Visibility = pnlSettings.Visibility == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
        }

        private void OpenInVS_Click(object sender, RoutedEventArgs e)
        {
            GeneralOptions.Instance.OpenInDefaultBrowser = cbOpenInVS.IsChecked.Value;
            GeneralOptions.Instance.Save();
        }
    }
}