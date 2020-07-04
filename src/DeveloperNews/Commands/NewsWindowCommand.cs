using System;
using System.ComponentModel.Design;
using DeveloperNews.ToolWindows;
using Microsoft;
using Microsoft.VisualStudio.Shell;
using Task = System.Threading.Tasks.Task;

namespace DeveloperNews
{
    internal sealed class NewsWindowCommand
    {
        public static async Task InitializeAsync(AsyncPackage package)
        {
            await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync(package.DisposalToken);

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Assumes.Present(commandService);

            var cmdId = new CommandID(PackageGuids.guidDeveloperNewsPackageCmdSet, PackageIds.NewsWindowCommandId);
            var cmd = new MenuCommand((s, e) => Execute(package), cmdId);
            commandService.AddCommand(cmd);
        }

        private static void Execute(AsyncPackage package)
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
