using Microsoft.VisualStudio.Shell;

using System;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;

namespace FeedManager.ToolWindows
{
	[Guid(WindowGuidString)]
	public class NewsWindow : ToolWindowPane
	{
		public const string WindowGuidString = "7b3eb750-0ca7-4a08-b0a7-b48c654b741a";

		public const string Title = "News";

		public NewsWindow(SyndicationFeed feed) : base(null)
		{
			Caption = Title;
			Content = new NewsWindowControl(feed);
		}
	}
}
