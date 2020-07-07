using System;
using System.Diagnostics;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;
using task = System.Threading.Tasks.Task;

internal class OutputWindowTraceListener : TraceListener
{
    private static IVsOutputWindowPane _pane;
    private static IVsOutputWindow _output;

    public static void Register()
    {
        var instance = new OutputWindowTraceListener();
        Trace.Listeners.Add(instance);
    }

    public override void Write(string message)
        => WriteLine(message);

    public override void WriteLine(string message)
    {
        if (!string.IsNullOrEmpty(message) && message.Contains(nameof(DevNews)))
        {
            LogAsync(message + Environment.NewLine).ConfigureAwait(false);
        }
    }

    private static async task LogAsync(object message)
    {
        try
        {
            if (await EnsurePaneAsync())
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();
                _pane.OutputString(message + Environment.NewLine);
            }
        }
        catch (Exception ex)
        {
            Debug.Write(ex);
        }
    }

    private static async System.Threading.Tasks.Task<bool> EnsurePaneAsync()
    {
        if (_pane == null || _output == null)
        {
            try
            {
                await ThreadHelper.JoinableTaskFactory.SwitchToMainThreadAsync();

                _output = await ServiceProvider.GetGlobalServiceAsync(typeof(SVsOutputWindow)) as IVsOutputWindow;

                var guid = new Guid();
                _output.CreatePane(ref guid, DevNews.Vsix.Name, 1, 1);
                _output.GetPane(ref guid, out _pane);
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex, nameof(OutputWindowTraceListener));
            }
        }

        return _pane != null;
    }
}