using System.ComponentModel;
using Spectre.Console.Cli;

namespace CLI.Commands;

// ReSharper disable once ClassNeverInstantiated.Global
public class ListCommand : Command<ListCommand.Settings>
{
    public override int Execute(CommandContext context, Settings settings)
    {
        return Lister.Execute(settings);
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<TargetAssembly>")]
        [Description("Target assembly to list resources of")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public required string Target { get; set; }
    }
}