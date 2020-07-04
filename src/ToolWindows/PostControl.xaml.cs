using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Windows.Controls;
using System.Windows.Input;

namespace DeveloperNews.ToolWindows
{
	public partial class PostControl : UserControl
	{
		private static readonly Regex _regex = new Regex(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", RegexOptions.Singleline);
		private readonly Uri _url;

		public PostControl(SyndicationItem item)
		{
			_url = item.Links.FirstOrDefault().Uri;

			InitializeComponent();

			lblTitle.Content = item.Title.Text;
			lblSummary.Text = TruncateHtml(item.Summary.Text);
		}

		private void Button_Click(object sender, System.Windows.RoutedEventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			if (GeneralOptions.Instance.OpenInDefaultBrowser)
			{
				Process.Start(_url.OriginalString);
			}
			else
			{
				var service = Package.GetGlobalService(typeof(IVsWebBrowsingService)) as IVsWebBrowsingService;
				service.Navigate(_url.OriginalString, (uint)__VSWBNAVIGATEFLAGS.VSNWB_WebURLOnly, out _);
			}
		}

		public static string TruncateHtml(string input, int length = 200, string ommission = "...")
		{
			string clearText = _regex.Replace(input, "");

			int nextSpace = clearText.LastIndexOf(" ", Math.Min(length, clearText.Length));

			return string.Format("{0}" + ommission,
								  clearText.Substring(0, (nextSpace > 0) ? nextSpace : length).Trim());
		}
	}
}
