using System;
using System.ComponentModel.Design;

using DeveloperNews.ToolWindows;

using Microsoft.VisualStudio.Shell;

using Task = System.Threading.Tasks.Task;

namespace DeveloperNews
{
    internal sealed class NewsWindowCommand
    {
        private readonly AsyncPackage _package;

        private NewsWindowCommand(AsyncPackage package, OleMenuCommandService commandService)
        {
            _package = package ?? throw new ArgumentNullException(nameof(package));
            commandService = commandService ?? throw new ArgumentNullException(nameof(commandService));

            var menuCommandID = new CommandID(PackageGuids.guidDeveloperNewsPackageCmdSet, PackageIds.NewsWindowCommandId);
            var menuItem = new MenuCommand(Execute, menuCommandID);
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

            var commandService = await package.GetServiceAsync((typeof(IMenuCommandService))) as OleMenuCommandService;
            Instance = new NewsWindowCommand(package, commandService);
        }

        private void Execute(object sender, EventArgs e)
        {
            _package.JoinableTaskFactory.RunAsync(async delegate
            {
                ToolWindowPane window = await _package.ShowToolWindowAsync(typeof(NewsWindow), 0, true, _package.DisposalToken);
                if ((null == window) || (null == window.Frame))
                {
                    throw new NotSupportedException("Cannot create tool window");
                }
            });
        }
    }
}
