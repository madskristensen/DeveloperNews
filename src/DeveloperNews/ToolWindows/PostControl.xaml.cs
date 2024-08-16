using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Automation;
using System.Windows.Controls;
using System.Windows.Input;
using DevNews.Resources;
using Microsoft.VisualStudio.PlatformUI;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DevNews.ToolWindows
{
    public partial class PostControl : UserControl
    {
        private static readonly Regex _regex = new(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", RegexOptions.Singleline | RegexOptions.Compiled);
        private string _url;

        public PostControl(SyndicationItem item)
        {
            InitializeComponent();
            SetUrl(item);
            BindText(item);
        }

        private void BindText(SyndicationItem item)
        {
            var summary = item.Summary?.Text ?? Text.NoDescription;
            lblTitle.Text = WebUtility.HtmlDecode(item.Title.Text).Trim();
            lblTitle.ToolTip = $"{lblTitle.Text.Trim()}\r\n{item.PublishDate:MMMM d, yyyy}";
            lblSummary.Text = WebUtility.HtmlDecode(TruncateHtml(summary.Trim()));
            lblSource.Content = $"{item.PublishDate:MMM d} in {item.SourceFeed?.Title?.Text}";

            if (lblSummary.Text == Text.NoDescription)
            {
                lblSummary.SetResourceReference(ForegroundProperty, EnvironmentColors.CommandBarMenuWatermarkTextBrushKey);
            }

            SetValue(AutomationProperties.NameProperty, lblTitle.Text);
        }

        private void SetUrl(SyndicationItem item)
        {
            _url = item.Links.FirstOrDefault()?.Uri?.OriginalString;

            if (!_url.Contains('?'))
            {
                // To track the referrals from this extension
                _url += "?cid=vs_developer_news";
            }
        }

        private void PostClick(object sender, RoutedEventArgs e)
        {
            OpenInDefaultBrowserClick(this, null);
        }

        private static string TruncateHtml(string input)
        {
            var clearText = _regex.Replace(input, "").Replace("\r", " ").Replace("\n", "");
            var maxLength = Math.Min(1000, clearText.Length);

            return clearText.Substring(0, maxLength);
        }

        private void OpenInDefaultBrowserClick(object sender, RoutedEventArgs e)
        {
            Process.Start(_url);
        }

        private void CopyUrlClick(object sender, RoutedEventArgs e)
        {
            Clipboard.SetText(_url);
        }
    }
}
