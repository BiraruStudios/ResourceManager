using System.Diagnostics;

namespace CLI;

public static class Utils
{
    private static readonly string[] ValidExtensions = [".exe", ".dll"];

    public static string GetExecutableName()
    {
        try
        {
            return Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName ?? "");
        }
        catch
        {
            return Path.GetFileName(Environment.GetCommandLineArgs()[0]);
        }
    }

    public static bool IsValidExtension(string ext)
    {
        return ValidExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
    }
}