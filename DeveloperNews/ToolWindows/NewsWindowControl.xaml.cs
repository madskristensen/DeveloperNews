using System.Linq;
using System.ServiceModel.Syndication;
using System.Windows;
using System.Windows.Controls;

namespace DeveloperNews.ToolWindows
{
	public partial class NewsWindowControl : UserControl
	{
		public NewsWindowControl(SyndicationFeed feed)
		{
			InitializeComponent();

			foreach (SyndicationItem item in feed.Items)
			{
				PostControl post = new PostControl(item);
				pnlPosts.Children.Add(post);
			}

			lblTotal.Content = $"{feed.Items.Count()} total";
		}

		private void RefreshClick(object sender, RoutedEventArgs e)
		{

		}

		private void MarkAllReadClick(object sender, RoutedEventArgs e)
		{

		}
	}
}