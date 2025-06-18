using System.Reflection;
using CLI.Commands;
using CLI.Common;
using CLI.Hooks;
using Spectre.Console.Cli;

namespace CLI;

public static class Program
{
    public static int Main(string[] args)
    {
        var commandApp = new CommandApp();
        var exeName = Utils.GetExecutableName();
        var version = Assembly.GetExecutingAssembly()
            .GetName()
            .Version?
            .ToString() ?? "Unknown";

        commandApp.Configure(config =>
        {
            config.SetApplicationName(exeName);
            config.SetApplicationVersion(version);

            config.SetInterceptor(new BeforeHook());

            config.AddCommand<ListCommand>("list");
            config.AddCommand<MergeCommand>("merge");
        });

        return commandApp.Run(args);
    }
}