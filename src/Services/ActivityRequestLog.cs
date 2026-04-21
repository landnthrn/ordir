using System.IO;

namespace Ordir.Services;

/// <summary>Formats activity lines like <c>workingDir&gt; message</c> (shell-style).</summary>
internal static class ActivityRequestLog
{
    internal static string FormatRequestLine(string? path, string messageWithOptionalLeadingGreater)
    {
        var msg = messageWithOptionalLeadingGreater ?? string.Empty;
        var pt = (path ?? string.Empty).Trim();
        if (string.IsNullOrEmpty(pt))
            return msg;

        string full;
        try
        {
            full = Path.GetFullPath(pt);
        }
        catch
        {
            full = pt;
        }

        var body = msg.TrimStart();
        if (body.StartsWith('>'))
            body = body[1..].TrimStart();

        return $"{full}> {body}";
    }
}
