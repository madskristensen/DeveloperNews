using System;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Threading;

namespace DevNews.ToolWindows
{
    public partial class NewsWindowControl : UserControl
    {
        private readonly NewsViewModel _viewModel;

        public NewsWindowControl(SyndicationFeed feed)
        {
            InitializeComponent();
            
            _viewModel = new NewsViewModel();
            DataContext = _viewModel;
            
            BindPosts(feed);
            RefreshClick(this, null);
        }

        private void SettingsSaved(object sender, EventArgs e)
        {
            RefreshClick(this, null);
        }

        private void BindPosts(SyndicationFeed feed)
        {
            _viewModel.UpdateFeed(feed);

            _ = ThreadHelper.JoinableTaskFactory.StartOnIdle(async () =>
            {
                Options options = await Options.GetLiveInstanceAsync();
                options.LastRead = DateTime.Now;
                options.UnreadPosts = 0;
                await options.SaveAsync();
            });
        }

        private void RefreshClick(object sender, RoutedEventArgs e)
        {
            prsLoader.Visibility = Visibility.Visible;
            lnkRefresh.IsEnabled = false;
            scrollViewer.IsEnabled = false;

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
                    scrollViewer.IsEnabled = true;
                }
            }).Task.Forget();
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