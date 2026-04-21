using System.IO;
using System.Text;
using System.Windows;
using System.Windows.Threading;

namespace Ordir;

// Partial base class is in App.g.cs (System.Windows.Application). Avoid ": Application" here:
// the WPF markup temp project adds global System.Windows.Forms, so "Application" can mean WinForms.

public partial class App
{
    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);
        Current.DispatcherUnhandledException += OnDispatcherUnhandledException;
        AppDomain.CurrentDomain.UnhandledException += OnUnhandledException;
    }

    private static void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
    {
        try
        {
            WriteFaultLog("DispatcherUnhandledException", e.Exception);
        }
        catch
        {
            // ignore
        }

        _ = MessageBox.Show(
            e.Exception.Message + Environment.NewLine + Environment.NewLine + "Details were written to Ordir-fault.log next to this program.",
            "Ordir — error",
            MessageBoxButton.OK,
            MessageBoxImage.Error);
        e.Handled = true;
    }

    private static void OnUnhandledException(object sender, UnhandledExceptionEventArgs e)
    {
        if (e.ExceptionObject is Exception ex)
        {
            try
            {
                WriteFaultLog("AppDomain.UnhandledException", ex);
            }
            catch
            {
                // ignore
            }
        }
    }

    private static void WriteFaultLog(string kind, Exception ex)
    {
        var sb = new StringBuilder();
        sb.AppendLine(kind);
        sb.AppendLine(DateTime.Now.ToString("O"));
        sb.AppendLine();
        sb.AppendLine(ex.ToString());

        var dir = AppContext.BaseDirectory;
        var path = Path.Combine(dir, "Ordir-fault.log");
        File.WriteAllText(path, sb.ToString(), Encoding.UTF8);
    }
}
