using System;
using System.ServiceModel.Syndication;
using System.Text.RegularExpressions;
using System.Windows.Controls;

namespace DeveloperNews.ToolWindows
{
	public partial class PostControl : UserControl
	{
		private static readonly Regex regex = new Regex(@"</?\w+((\s+\w+(\s*=\s*(?:"".*?""|'.*?'|[^'"">\s]+))?)+\s*|\s*)/?>", RegexOptions.Singleline);

		public PostControl(SyndicationItem item)
		{
			InitializeComponent();

			lblTitle.Text = item.Title.Text;
			lblSummary.Text = TruncateHtml(item.Summary.Text);
		}

		public static string TruncateHtml(string input, int length = 200, string ommission = "...")
		{
			string clearText = regex.Replace(input, "");

			int nextSpace = clearText.LastIndexOf(" ", Math.Min(length, clearText.Length));

			return string.Format("{0}" + ommission,
								  clearText.Substring(0, (nextSpace > 0) ? nextSpace : length).Trim());
		}
	}
}
