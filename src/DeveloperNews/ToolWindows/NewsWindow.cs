using System;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;

using Microsoft.VisualStudio.Shell;

namespace DeveloperNews.ToolWindows
{
    [Guid(WindowGuidString)]
    public class NewsWindow : ToolWindowPane
    {
        public const string WindowGuidString = "7b3eb750-0ca7-4a08-b0a7-b48c654b741c";

        public const string Title = "News";

        public NewsWindow(SyndicationFeed feed) : base(null)
        {
            Caption = Title;
            Content = new NewsWindowControl(feed);
        }
    }
}
