using System;
using System.Runtime.InteropServices;
using System.ServiceModel.Syndication;
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
        private CrispImageWithCount _iconCounter;
        private SyndicationFeed _feed;

        public static FeedStore Store { get; private set; }

        protected override async Task InitializeAsync(CancellationToken cancellationToken, IProgress<ServiceProgressData> progress)
        {
            FeedOrchestrator.FeedUpdated += FeedOrchestrator_FeedUpdated;
            Store = new FeedStore(ApplicationRegistryRoot);

            await JoinableTaskFactory.SwitchToMainThreadAsync(cancellationToken);

            SetupStatusBarIconAsync().FileAndForget(nameof(DevNews));

            _feed = await Store.GetFeedAsync();
            await NewsWindowCommand.InitializeAsync(this);

            StartTimerToCheckForUpdates();
        }

        private void StartTimerToCheckForUpdates()
        {
            var timeInterval = 12 * 60 * 60 * 1000; // 12 hours
            var timer = new Timer((o) =>
            {
                _ = JoinableTaskFactory.StartOnIdle(async () =>
                {
                    _feed = await Store.GetFeedAsync();
                }, VsTaskRunContext.UIThreadBackgroundPriority);
            }, null, timeInterval, timeInterval);
        }

        protected override int QueryClose(out bool canClose)
        {
            try
            {
                if (_iconCounter?.Count > 0)
                {
                    OpenNewsWindow();
                }
            }
            catch
            {
                // Don't prevent VS shutdown
            }

            return base.QueryClose(out canClose);
        }

        private void FeedOrchestrator_FeedUpdated(object sender, int unreadPosts)
        {
            if (_iconCounter != null)
            {
                _iconCounter.Count = unreadPosts;
            }
        }

        private async Task SetupStatusBarIconAsync()
        {
            Options options = await Options.GetLiveInstanceAsync();

            await JoinableTaskFactory.SwitchToMainThreadAsync();

            DockPanel panel = new()
            {
                Margin = new Thickness(0, 0, 5, 0),
                Height = 22,
                ToolTip = "Open Developer News (Ctrl+Alt+N)", // TODO: Get the shortcut from the command dynamically
            };

            _iconCounter = new CrispImageWithCount()
            {
                Moniker = KnownMonikers.Dictionary,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                Count = options.UnreadPosts,
            };

            panel.Children.Add(_iconCounter);
            panel.MouseUp += OpenNewsWindow;
            panel.MouseEnter += Panel_MouseEnter;
            panel.MouseLeave += Panel_MouseEnter;

            await StatusBarInjector.InjectControlAsync(panel);
        }

        private void Panel_MouseEnter(object sender, MouseEventArgs e)
        {
            var panel = (Panel)sender;

            panel.Background = panel.IsMouseOver ? new SolidColorBrush(Colors.White) { Opacity = 0.1 } : (Brush)Brushes.Transparent;
        }

        private void OpenNewsWindow(object sender = null, MouseButtonEventArgs e = null)
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

        protected override Task<object> InitializeToolWindowAsync(Type toolWindowType, int id, CancellationToken cancellationToken)
        {
            return Task.FromResult<object>(_feed);
        }
    }
}