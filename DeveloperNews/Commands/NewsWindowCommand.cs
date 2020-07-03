using DeveloperNews.ToolWindows;

using Microsoft.VisualStudio.Shell;

using System;
using System.ComponentModel.Design;

using Task = System.Threading.Tasks.Task;

namespace DeveloperNews
{
	internal sealed class NewsWindowCommand
	{
		private readonly AsyncPackage package;

		private NewsWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
		{
			this.package = package ?? throw new ArgumentNullException(nameof(package));
			commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

			CommandID menuCommandID = new CommandID(PackageGuids.guidDeveloperNewsPackageCmdSet, PackageIds.NewsWindowCommandId);
			MenuCommand menuItem = new MenuCommand(Execute, menuCommandID);
			commandService.AddCommand(menuItem);
		}

		public static NewsWindowCommand Instance
		{
			get;
			private set;
		}

		public static async Task InitializeAsync(AsyncPackage package)
		{
			await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

			OleMenuCommandService commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
			Instance = new NewsWindowCommand(package, commandService);
		}

		private void Execute(object sender, EventArgs e)
		{
			package.JoinableTaskFactory.RunAsync(async delegate
			{
				ToolWindowPane window = await package.ShowToolWindowAsync(typeof(NewsWindow), 0, true, package.DisposalToken);
				if ((null == window) || (null == window.Frame))
				{
					throw new NotSupportedException("Cannot create tool window");
				}
			});
		}
	}
}
