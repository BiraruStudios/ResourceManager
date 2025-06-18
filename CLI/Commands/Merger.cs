using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Spectre.Console;

namespace CLI.Commands;

public static class Merger
{
    /*
     * Return Codes:
     * 0 = Success
     * 1 = Target file missing
     * 2 = Source file missing
     * 3 = Invalid target extension
     * 4 = Invalid source extension
     * 5 = Fatal error
     * 6 = No Resource To Embed
     */
    public static int Execute(MergeCommand.Settings settings)
    {
        var validation = TryValidatePaths(settings, out var targetPath, out var sourcePath);
        if (validation != 0)
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
            if (successEmbed != 0)
                return successEmbed;

            RemoveAssemblyLinkedResources(targetModule, sourceModule);

            targetModule.Write(outputPath, writerOptions);

            UIHelpers.WriteSpacer();
            AnsiConsole.Write(new Markup($"[lightgreen]Merged output saved to: {outputPath}[/]").Centered());
            UIHelpers.WriteSpacer();

            return 0;
        }
        catch (Exception ex)
        {
            return UIHelpers.PrintFatalError(ex, 6);
        }
    }

    private static int EmbedResources(ModuleDefMD target, ModuleDefMD source)
    {
        var toEmbedList = source.Resources
            .OfType<EmbeddedResource>()
            .ToList();

        if (toEmbedList.Count == 0) return UIHelpers.PrintError("No resources to embed.", 6);

        UIHelpers.WriteSpacer();

        foreach (var resource in toEmbedList)
        {
            AnsiConsole.Write(new Markup($"[grey]Embedding:[/] [aqua]{resource.Name}[/]").Centered());
            var data = resource.CreateReader().ToArray();
            target.Resources.Add(new EmbeddedResource(resource.Name, data, resource.Attributes));
        }

        return 0;
    }

    private static void RemoveAssemblyLinkedResources(ModuleDefMD target, ModuleDefMD source)
    {
        var toRemove = target.Resources
            .OfType<AssemblyLinkedResource>()
            .Where(r => string.Equals(r.Assembly.Name, source.Assembly.Name, StringComparison.OrdinalIgnoreCase))
            .ToList();

        UIHelpers.WriteSpacer();

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

    private static int TryValidatePaths(MergeCommand.Settings settings, out string target, out string source)
    {
        target = Path.GetFullPath(settings.Target);
        source = Path.GetFullPath(settings.Source);

        AnsiConsole.Write(new Markup($"[grey]Target Assembly:[/] [lightgreen]{target}[/]").Centered());
        AnsiConsole.Write(new Markup($"[grey]Source Assembly:[/] [lightgreen]{source}[/]").Centered());

        if (!File.Exists(target))
            return UIHelpers.PrintError("Target file not found.", 1);

        if (!File.Exists(source))
            return UIHelpers.PrintError("Source file not found.", 2);

        if (!Utils.IsValidExtension(Path.GetExtension(target)))
            return UIHelpers.PrintError("Target file must be a .exe or a .dll.", 3);

        if (!Utils.IsValidExtension(Path.GetExtension(source)))
            return UIHelpers.PrintError("Source file must be a .exe or a .dll.", 4);

        return 0;
    }
}