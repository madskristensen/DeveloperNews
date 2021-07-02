using System;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;
using DevNews.Resources;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;

namespace DevNews.ToolWindows
{
    /// <summary>
    /// The tool window hosting the Developer News content.
    /// </summary>
    [Guid(PackageGuids.guidToolWindowString)]
    public class NewsWindow : ToolWindowPane
    {
        public NewsWindow(SyndicationFeed feed) : base(null)
        {
            BitmapImageMoniker = KnownMonikers.Dictionary;
            Caption = Text.WindowTitle;
            Content = new NewsWindowControl(feed);
        }
    }
}
