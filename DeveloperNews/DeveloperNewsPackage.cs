using DeveloperNews.ToolWindows;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace DeveloperNews
{
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[Guid(PackageGuids.guidDeveloperNewsPackageString)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[ProvideToolWindow(typeof(NewsWindow), Style = VsDockStyle.Tabbed, Window = "E13EEDEF-B531-4afe-9725-28A69FA4F896")]
	public sealed class DeveloperNewsPackage : AsyncPackage
	{
		protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
		{
			await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);
			await NewsWindowCommand.InitializeAsync(this);
		}

		public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
		{
			if (toolWindowType.Equals(new Guid(NewsWindow.WindowGuidString)))
			{
				return this;
			}

			return null;
		}

		protected override string GetToolWindowTitle(Type toolWindowType, int id)
		{
			if (toolWindowType == typeof(NewsWindow))
			{
				return NewsWindow.Title;
			}

			return base.GetToolWindowTitle(toolWindowType, id);
		}

		protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
		{
			FeedStore store = new FeedStore();
			return await store.GetFeedAsync();
		}
	}
}
