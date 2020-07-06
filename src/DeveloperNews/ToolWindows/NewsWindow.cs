﻿using System;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;
using DevNews.Resources;
using Microsoft.VisualStudio.Shell;

namespace DevNews.ToolWindows
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
