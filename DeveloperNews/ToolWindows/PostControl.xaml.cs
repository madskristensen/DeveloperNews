using EnvDTE;
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
		private Uri _url;

		public PostControl(SyndicationItem item)
		{
			_url = item.Links.FirstOrDefault().Uri;

			InitializeComponent();
						
			lblTitle.Text = item.Title.Text;
			lblSummary.Text = TruncateHtml(item.Summary.Text);
		}

		protected override void OnPreviewMouseUp(MouseButtonEventArgs e)
		{
			ThreadHelper.ThrowIfNotOnUIThread();

			try
			{
				var service = Package.GetGlobalService(typeof(DTE)) as DTE;
				service.ItemOperations.Navigate(_url.OriginalString, vsNavigateOptions.vsNavigateOptionsDefault);
			}
			catch (Exception ex)
			{
				Debug.WriteLine(ex);
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
