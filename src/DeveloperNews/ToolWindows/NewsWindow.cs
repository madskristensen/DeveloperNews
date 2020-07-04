using System;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;
using DeveloperNews.Resources;
using Microsoft.VisualStudio.Shell;

namespace DeveloperNews.ToolWindows
{
    [Guid(PackageGuids.guidToolWindowString)]
    public class NewsWindow : ToolWindowPane
    {
        public NewsWindow(SyndicationFeed feed) : base(null)
        {
            Caption = Text.WindowTitle;
            Content = new NewsWindowControl(feed);
        }
    }
}
