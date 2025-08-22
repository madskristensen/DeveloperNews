using System;
using System.ComponentModel;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using DevNews.Resources;

namespace DevNews.ToolWindows
{
    /// <summary>
    /// View model for a news post item
    /// </summary>
    public class PostViewModel : INotifyPropertyChanged
    {
        private static readonly Regex _htmlRegex = new Regex(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", 
            RegexOptions.Singleline | RegexOptions.Compiled);

        // Constants for better maintainability
        private const string TrackingParameter = "?cid=vs_developer_news";
        private const int MaxSummaryLength = 1000;

        public PostViewModel(SyndicationItem item)
        {
            if (item == null) return;

            Title = WebUtility.HtmlDecode(item.Title?.Text ?? "").Trim();
            Url = item.Links?.FirstOrDefault()?.Uri?.OriginalString ?? "";
            
            // Add tracking parameter if not present
            if (!string.IsNullOrEmpty(Url) && !Url.Contains('?'))
            {
                Url += TrackingParameter;
            }

            var summary = item.Summary?.Text ?? Text.NoDescription;
            Summary = WebUtility.HtmlDecode(TruncateHtml(summary.Trim()));
            
            PublishDate = item.PublishDate.DateTime;
            Source = $"{item.PublishDate:MMM d} in {item.SourceFeed?.Title?.Text}";
            
            ToolTip = $"{Title}\r\n{item.PublishDate:MMMM d, yyyy}";
            
            HasDescription = Summary != Text.NoDescription;
        }

        public string Title { get; }
        public string Summary { get; }
        public string Url { get; }
        public DateTime PublishDate { get; }
        public string Source { get; }
        public string ToolTip { get; }
        public bool HasDescription { get; }

        private static string TruncateHtml(string input)
        {
            var clearText = _htmlRegex.Replace(input, "").Replace("\r", " ").Replace("\n", "");
            var maxLength = Math.Min(MaxSummaryLength, clearText.Length);
            return clearText.Substring(0, maxLength);
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }

    /// <summary>
    /// View model for a timeline header
    /// </summary>
    public class TimelineHeaderViewModel
    {
        public TimelineHeaderViewModel(string header)
        {
            Header = header;
        }

        public string Header { get; }
    }
}