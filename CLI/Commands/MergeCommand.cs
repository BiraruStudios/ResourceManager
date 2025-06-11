using System.ComponentModel;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Spectre.Console;
using Spectre.Console.Cli;

namespace CLI.Commands;

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
    }
}

public static class Merger
{
    private static readonly string[] ValidExtensions = [".exe", ".dll"];

    public static int Execute(MergeCommand.Settings settings)
    {
        /*
         * Return Codes:
         * 1 = Success
         * 2 = Target file missing
         * 3 = Source file missing
         * 4 = Invalid target extension
         * 5 = Invalid source extension
         * 6 = Fatal error
         * 7 = No Resource To Embed
         */

        PrintBanner();

        var validation = TryValidatePaths(settings, out var targetPath, out var sourcePath);
        if (validation != 1)
            return validation;

        var outputPath = Path.Combine(
            Path.GetDirectoryName(targetPath)!,
            Path.GetFileNameWithoutExtension(targetPath) + "_Merged" + Path.GetExtension(targetPath)
        );

        try
        {
            var targetModule = ModuleDefMD.Load(targetPath);
            var sourceModule = ModuleDefMD.Load(sourcePath);

            var writerOptions = new ModuleWriterOptions(targetModule)
            {
                MetadataOptions = { Flags = MetadataFlags.KeepOldMaxStack }
            };

            var successEmbed = EmbedResources(targetModule, sourceModule);
            if (successEmbed == 7)
                return successEmbed;

            RemoveAssemblyLinkedResources(targetModule, sourceModule);

            targetModule.Write(outputPath, writerOptions);

            WriteSpacer();
            AnsiConsole.Write(new Markup($"[lightgreen]Merged output saved to: {outputPath}[/]").Centered());
            WriteSpacer();

            return 1;
        }
        catch (Exception ex)
        {
            return PrintFatalError(ex, 6);
        }
    }

    private static void PrintBanner()
    {
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("grey").Centered());
        WriteSpacer();
        AnsiConsole.Write(new FigletText("ResourceMerger").Centered().Color(Color.Yellow));
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("grey").Centered());
    }

    private static int TryValidatePaths(MergeCommand.Settings settings, out string target, out string source)
    {
        target = Path.GetFullPath(settings.Target);
        source = Path.GetFullPath(settings.Source);

        WriteSpacer();
        AnsiConsole.Write(new Markup($"[grey]Target Assembly:[/] [lightgreen]{target}[/]").Centered());
        AnsiConsole.Write(new Markup($"[grey]Source Assembly:[/] [lightgreen]{source}[/]").Centered());

        if (!File.Exists(target))
            return PrintError("Target file not found.", 2);

        if (!File.Exists(source))
            return PrintError("Source file not found.", 3);

        if (!IsValidExtension(Path.GetExtension(target)))
            return PrintError("Target file must be a .exe or a .dll.", 4);

        if (!IsValidExtension(Path.GetExtension(source)))
            return PrintError("Source file must be a .exe or a .dll.", 5);

        return 1;
    }

    private static int EmbedResources(ModuleDefMD target, ModuleDefMD source)
    {
        var toEmbedList = source.Resources
            .OfType<EmbeddedResource>()
            .ToList();

        if (toEmbedList.Count == 0) return PrintError("No resources to embed.", 7);

        WriteSpacer();

        foreach (var resource in toEmbedList)
        {
            AnsiConsole.Write(new Markup($"[grey]Embedding:[/] [aqua]{resource.Name}[/]").Centered());
            var data = resource.CreateReader().ToArray();
            target.Resources.Add(new EmbeddedResource(resource.Name, data, resource.Attributes));
        }

        return 1;
    }

    private static void RemoveAssemblyLinkedResources(ModuleDefMD target, ModuleDefMD source)
    {
        var toRemove = target.Resources
            .OfType<AssemblyLinkedResource>()
            .Where(r => string.Equals(r.Assembly.Name, source.Assembly.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        WriteSpacer();

        if (toRemove.Count == 0)
        {
            AnsiConsole.Write(new Markup("[yellow]No assembly-linked resources to remove. Skipping.[/]").Centered());
            return;
        }

        foreach (var resource in toRemove)
        {
            AnsiConsole.Write(
                new Markup($"[grey]Removing AssemblyLinkedResource:[/] [aqua]{resource.Name}[/]").Centered());
            target.Resources.Remove(resource);
        }
    }

    private static bool IsValidExtension(string ext)
    {
        return ValidExtensions.Contains(ext, StringComparer.OrdinalIgnoreCase);
    }

    private static int PrintError(string message, int code)
    {
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
        AnsiConsole.Write(new Markup("[red bold]Error:[/]").Centered());
        AnsiConsole.Write(new Markup($"[yellow]{message}[/]").Centered());
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
        return code;
    }

    private static int PrintFatalError(Exception ex, int code)
    {
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
        AnsiConsole.Write(new Markup("[red bold]Fatal error occurred:[/]").Centered());
        AnsiConsole.Write(new Markup($"[grey]{ex.GetType().Name}[/]: [yellow]{ex.Message}[/]").Centered());
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
        return code;
    }

    private static void WriteSpacer()
    {
        AnsiConsole.Write(new Text("").Centered());
    }
}