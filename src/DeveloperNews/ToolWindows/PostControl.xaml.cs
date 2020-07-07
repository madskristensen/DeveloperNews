using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

namespace DevNews.ToolWindows
{
    public partial class PostControl : UserControl
    {
        private static readonly Regex _regex = new Regex(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", RegexOptions.Singleline);
        private string _url;

        public PostControl(SyndicationItem item)
        {
            InitializeComponent();
            SetUrl(item);

            lblTitle.Text = WebUtility.HtmlDecode(item.Title.Text);
            lblSummary.Text = WebUtility.HtmlDecode(TruncateHtml(item.Summary.Text));
        }

        private void SetUrl(SyndicationItem item)
        {
            _url = item.Links.FirstOrDefault()?.Uri?.OriginalString;

            if (!_url.Contains('?'))
            {
                // To track the referrals from this extension
                _url += "?utm_source=vs_developer_news&utm_medium=referral";
            }
        }

        private void PostClick(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            var ctrlHit = Keyboard.IsKeyDown(Key.LeftCtrl) || Keyboard.IsKeyDown(Key.RightCtrl);

            if (Options.Instance.OpenInDefaultBrowser || ctrlHit)
            {
                OpenInDefaultBrowserClick(this, null);
            }
            else
            {
                OpenInVsClick(this, null);
            }
        }

        public static string TruncateHtml(string input, int length = 200, string ommission = "...")
        {
            var clearText = _regex.Replace(input, "");

            var nextSpace = clearText.LastIndexOf(" ", Math.Min(length, clearText.Length));

            return string.Format("{0}" + ommission,
                                  clearText.Substring(0, (nextSpace > 0) ? nextSpace : length).Trim());
        }

        private void OpenInVsClick(object sender, RoutedEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var service = Package.GetGlobalService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
            service.Navigate(_url, (uint)__VSWBNAVIGATEFLAGS.VSNWB_WebURLOnly, out _);
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
