using System.ComponentModel.Design;
using DevNews.ToolWindows;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace DevNews
{
    /// <summary>
    /// Handles the command invocation to show the News tool window
    /// </summary>
    internal sealed class NewsWindowCommand
    {
        /// <summary>
        /// Hooks up the "View -> Developer News" command and assigns execution handler.
        /// </summary>
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Assumes.Present(commandService);

            var cmdId = new CommandID(PackageGuids.guidDeveloperNewsPackageCmdSet, PackageIds.NewsWindowCommandId);
            var cmd = new MenuCommand((s, e) => Execute(package), cmdId);
            commandService.AddCommand(cmd);
        }

        /// <summary>
        /// Opens the "News" tool window in an asynchronous way.
        /// </summary>
        private static void Execute(AsyncPackage package)
        {
            package.JoinableTaskFactory.RunAsync(async delegate
            {
                ToolWindowPane window = await package.ShowToolWindowAsync(typeof(NewsWindow), 0, true, package.DisposalToken);
                Assumes.Present(window);
            }).FileAndForget(nameof(NewsWindowCommand));
        }
    }
}
