using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using DevNews.Resources;
using DevNews.ToolWindows;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace DevNews
{
    [Guid(PackageGuids.guidDeveloperNewsPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [ProvideToolWindow(typeof(NewsWindow), Style = VsDockStyle.Tabbed, Window = ToolWindowGuids.SolutionExplorer)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.EmptySolution_string)]
    public sealed class DeveloperNewsPackage : AsyncPackage
    {
        public static FeedStore Store { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            OutputWindowTraceListener.Register(Vsix.Name, nameof(DevNews));

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
