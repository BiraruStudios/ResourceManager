using System.Diagnostics;
using System.Reflection;
using CLI.Commands;
using Spectre.Console.Cli;

namespace CLI;

public static class Program
{
    public static int Main(string[] args)
    {
        var commandApp = new CommandApp();
        var exeName = GetExecutableName();
        var version = Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString() ?? "Unknown";

        commandApp.SetDefaultCommand<MergeCommand>();
        commandApp.Configure(config =>
        {
            config.SetApplicationName(exeName);
            config.SetApplicationVersion(version);
        });

        return commandApp.Run(args);
    }

    private static string GetExecutableName()
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
}