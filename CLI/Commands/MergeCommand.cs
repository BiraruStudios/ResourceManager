using System.ComponentModel;
using Spectre.Console.Cli;

namespace CLI.Commands;

[Description("Embeds all resources from the source assembly into the target assembly.")]
// ReSharper disable once ClassNeverInstantiated.Global
public class MergeCommand : Command<MergeCommand.Settings>
{
    public override int Execute(CommandContext context, Settings settings)
    {
        return Merger.Execute(settings);
    }

    // ReSharper disable once ClassNeverInstantiated.Global
    public sealed class Settings : CommandSettings
    {
        [CommandArgument(0, "<TargetAssembly>")]
        [Description("Target assembly to merge into")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public required string Target { get; set; }

        [CommandArgument(1, "<SourceAssembly>")]
        [Description("Source assembly to merge from")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public required string Source { get; set; }

        [CommandOption("-o|--output")]
        [Description("The merged assembly to output")]
        // ReSharper disable once UnusedAutoPropertyAccessor.Global
        public string? Output { get; set; }
    }
}