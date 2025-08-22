using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;

namespace DevNews.ToolWindows
{
    public partial class PostControl : UserControl
    {
        public PostControl()
        {
            InitializeComponent();
        }

        private PostViewModel ViewModel => DataContext as PostViewModel;

        private void PostClick(object sender, RoutedEventArgs e)
        {
            OpenInDefaultBrowserClick(this, null);
        }

        private void OpenInDefaultBrowserClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ViewModel?.Url))
            {
                Process.Start(ViewModel.Url);
            }
        }

        private void CopyUrlClick(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ViewModel?.Url))
            {
                Clipboard.SetText(ViewModel.Url);
            }
        }
    }
}
