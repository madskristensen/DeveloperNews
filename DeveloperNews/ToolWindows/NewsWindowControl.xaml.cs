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
			BindPosts(feed);
		}

		private void BindPosts(SyndicationFeed feed)
		{
			pnlPosts.Children.Clear();
			string currentTime = Timestamp(feed.Items.FirstOrDefault());

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

					PostControl post = new PostControl(item);
					pnlPosts.Children.Add(post);
				}
			}

			lblTotal.Content = $"{feed.Items.Count()} total";
		}

		private void AddTimeLabel(string timestamp)
		{
			var time = new Label
			{
				Content = timestamp,
				FontWeight = FontWeights.Medium,
				Opacity = 0.8,
			};

			pnlPosts.Children.Add(time);
		}

		private static string Timestamp(SyndicationItem item)
		{
			if (item == null) return null;

			if (item.PublishDate.Date >= System.DateTime.Now.AddDays(-1))
			{
				return "Today";
			}
			else if (item.PublishDate.Date >= System.DateTime.Now.AddDays(-2))
			{
				return "Yesterday";
			}
			else if (item.PublishDate.Date >= System.DateTime.Now.AddDays(-7))
			{
				return "Past week";
			}

			return "Older";
		}

		private void RefreshClick(object sender, RoutedEventArgs e)
		{

		}
	}
}