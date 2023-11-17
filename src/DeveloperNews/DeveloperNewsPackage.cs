using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using DevNews.Resources;
using DevNews.ToolWindows;
using EnvDTE;
using EnvDTE80;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Imaging;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;

using Task = System.Threading.Tasks.Task;

namespace DevNews
{
    [PackageRegistration(UseManagedResourcesOnly = true, AllowsBackgroundLoading = true)]
    [Guid(PackageGuids.guidDeveloperNewsPackageString)]
    [ProvideMenuResource("Menus.ctmenu", 1)]
    [InstalledProductRegistration(Vsix.Name, Vsix.Description, Vsix.Version)]
    [ProvideProfile(typeof(DialogPageProvider.General), Vsix.Name, "General", 0, 110, false)]
    [ProvideToolWindow(typeof(NewsWindow), Style = VsDockStyle.Tabbed, Window = ToolWindowGuids.SolutionExplorer)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.NoSolution_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.SolutionHasSingleProject_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.SolutionHasMultipleProjects_string)]
    [ProvideToolWindowVisibility(typeof(NewsWindow), VSConstants.UICONTEXT.EmptySolution_string)]
    [ProvideAutoLoad(VSConstants.UICONTEXT.ShellInitialized_string, PackageAutoLoadFlags.BackgroundLoad)]
    public sealed class DeveloperNewsPackage : AsyncPackage
    {
        public static FeedStore Store { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            SetupStatusBarIconAsync().FileAndForget(nameof(DevNews));

            await NewsWindowCommand.InitializeAsync(this);
        }

        private async Task SetupStatusBarIconAsync()
        {
            await JoinableTaskFactory.SwitchToMainThreadAsync();

            DockPanel panel = new()
            {
                Margin = new Thickness(0, 0, 5, 0),
                Width = 24,
                Height = 22,
                ToolTip = "Open Developer News (Ctrl+Alt+N)",
            };

            CrispImage img = new()
            {
                Moniker = KnownMonikers.Dictionary,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Width = 16,
                Height = 16,
            };

            panel.Children.Add(img);
            panel.MouseUp += StatusIconClicked;
            panel.MouseEnter += Panel_MouseEnter;
            panel.MouseLeave += Panel_MouseEnter;

            await StatusBarInjector.InjectControlAsync(panel);
        }

        private void Panel_MouseEnter(object sender, MouseEventArgs e)
        {
            var panel = (Panel)sender;

            if (panel.IsMouseOver)
            {
                panel.Background = new SolidColorBrush(Colors.White) { Opacity = 0.1 };
            }
            else
            {
                panel.Background = Brushes.Transparent;
            }
        }

        private void StatusIconClicked(object sender, MouseButtonEventArgs e)
        {
            ThreadHelper.ThrowIfNotOnUIThread();
            var dte = GetService(typeof(DTE)) as DTE2;
            dte.Commands.Raise(PackageGuids.guidDeveloperNewsPackageCmdSetString, PackageIds.NewsWindowCommandId, null, null);
        }

        public override IVsAsyncToolWindowFactory GetAsyncToolWindowFactory(Guid toolWindowType)
        {
            return toolWindowType.Equals(PackageGuids.guidToolWindow) ? this : null;
        }

        protected override string GetToolWindowTitle(Type toolWindowType, int id)
        {
            return toolWindowType == typeof(NewsWindow) ? Text.WindowTitle : base.GetToolWindowTitle(toolWindowType, id);
        }

        protected override async Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            Store = new FeedStore(ApplicationRegistryRoot);
            return await Store.GetFeedAsync();
        }
    }
}