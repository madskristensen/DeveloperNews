﻿using FeedManager.ToolWindows;

using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Task = System.Threading.Tasks.Task;

namespace FeedManager
{
	[InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
	[PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
	[Guid(PackageGuids.guidDeveloperNewsPackageString)]
	[ProvideMenuResource("Menus.ctmenu", 1)]
	[ProvideToolWindow(typeof(NewsWindow), Style = VsDockStyle.Tabbed, Width = 300, Height = 600, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
	public sealed class DeveloperNewsPackage : AsyncPackage
	{
		public static FeedStore Store { get; private set; }

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
			Store = new FeedStore(ApplicationRegistryRoot);
			return await Store.GetFeedAsync();
		}
	}
}