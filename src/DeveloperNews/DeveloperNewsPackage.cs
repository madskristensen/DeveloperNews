using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DeveloperNews.Resources;
using DeveloperNews.ToolWindows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace DeveloperNews
{
    [Guid(PackageGuids.guidDeveloperNewsPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideToolWindow(typeof(NewsWindow), Style = VsDockStyle.Tabbed, Window = "3ae79031-e1bc-11d0-8f78-00a0c9110057")]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.EmptySolution_string)]
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
            if (toolWindowType.Equals(PackageGuids.guidToolWindow))
            {
                return this;
            }

            return null;
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            if (toolWindowType == typeof(NewsWindow))
            {
                return Text.WindowTitle;
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
