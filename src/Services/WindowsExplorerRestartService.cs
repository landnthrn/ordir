using System.Diagnostics;
using System.IO;

namespace Ordir.Services;

/// <summary>Restarts the Windows Explorer shell (explorer.exe) and optionally opens a folder afterward.</summary>
internal static class WindowsExplorerRestartService
{
    private static readonly string ExplorerExe = Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.Windows),
        "explorer.exe");

    /// <summary>
    /// Stops Explorer via PowerShell (<c>Stop-Process -Name explorer -Force</c>), waits briefly, then starts
    /// <c>explorer.exe</c>: with the folder path if valid, otherwise a normal launch (opens default / home window),
    /// even when the shell process is already running again in the background.
    /// </summary>
    public static async Task RestartAsync(string? openFolderAfterIfExists)
    {
        await Task.Run(() =>
        {
            var ps = Process.Start(new ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = "-NoProfile -NonInteractive -ExecutionPolicy Bypass -Command \"Stop-Process -Name explorer -Force\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                WindowStyle = ProcessWindowStyle.Hidden
            });
            try
            {
                ps?.WaitForExit(30_000);
            }
            finally
            {
                ps?.Dispose();
            }
        }).ConfigureAwait(true);

        await Task.Delay(800).ConfigureAwait(true);

        string? folderArg = null;
        if (!string.IsNullOrWhiteSpace(openFolderAfterIfExists))
        {
            try
            {
                var full = Path.GetFullPath(openFolderAfterIfExists.Trim());
                if (Directory.Exists(full))
                    folderArg = full;
            }
            catch
            {
                // ignore invalid path
            }
        }

        try
        {
            if (folderArg != null)
            {
                // Single explorer launch at the target folder (no separate "home" + folder).
                Process.Start(new ProcessStartInfo
                {
                    FileName = ExplorerExe,
                    Arguments = "\"" + folderArg.Replace("\"", "\\\"") + "\"",
                    UseShellExecute = true
                });
                await Task.Delay(600).ConfigureAwait(true);
                return;
            }

            // No target folder: always open a File Explorer window (Quick Access / home style), same as
            // Start-Process explorer.exe — even if explorer.exe is already running for the taskbar/shell.
            Process.Start(new ProcessStartInfo
            {
                FileName = ExplorerExe,
                UseShellExecute = true
            });
            await Task.Delay(600).ConfigureAwait(true);
        }
        catch
        {
            // Best-effort restart
        }
    }
}
