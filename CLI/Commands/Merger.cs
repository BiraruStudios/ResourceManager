using CLI.Common;
using CLI.Enums;
using CLI.Helpers;
using dnlib.DotNet;
using dnlib.DotNet.Writer;

namespace CLI.Commands;

public static class Merger
{
    public static int Execute(MergeCommand.Settings settings)
    {
        var validation = TryValidatePaths(settings, out var targetPath, out var sourcePath, out var outputPath);
        if (validation != (int)ExitCodes.Success)
            return validation;

        try
        {
            using var targetModule = AssemblyHelpers.LoadAssembly(targetPath);
            using var sourceModule = AssemblyHelpers.LoadAssembly(sourcePath);

            var writerOptions = new ModuleWriterOptions(targetModule)
            {
                WritePdb = true,
                PdbFileName = Path.ChangeExtension(outputPath, ".pdb"),
                MetadataOptions = { Flags = MetadataFlags.KeepOldMaxStack }
            };

            var successEmbed = EmbedResources(targetModule, sourceModule);
            if (successEmbed != (int)ExitCodes.Success)
                return successEmbed;

            RemoveAssemblyLinkedResources(targetModule, sourceModule);

            targetModule.Write(outputPath, writerOptions);

            UIHelpers.WriteSpacer();
            UIHelpers.WriteInfo($"[lightgreen]Merged output saved to: {outputPath}[/]");
            UIHelpers.WriteSpacer();

            return (int)ExitCodes.Success;
        }
        catch (BadImageFormatException)
        {
            return UIHelpers.PrintError("One of the input files is not a valid .NET assembly.",
                (int)ExitCodes.FatalError);
        }
        catch (IOException ex)
        {
            return UIHelpers.PrintError($"I/O error while loading assembly: {ex.Message}", (int)ExitCodes.FatalError);
        }
        catch (Exception ex)
        {
            return UIHelpers.PrintFatalError(ex, (int)ExitCodes.FatalError);
        }
    }

    private static int EmbedResources(ModuleDefMD target, ModuleDefMD source)
    {
        var toEmbedList = source.Resources
            .OfType<EmbeddedResource>()
            .ToList();

        if (toEmbedList.Count == 0)
            return UIHelpers.PrintError("No resources to embed.", (int)ExitCodes.NoResourceToEmbed);

        UIHelpers.WriteSpacer();

        const int maxAllowedSize = 50 * 1024 * 1024;

        foreach (var resource in toEmbedList)
        {
            var reader = resource.CreateReader();
            if (reader.Length > maxAllowedSize)
            {
                UIHelpers.WriteInfo(
                    $"[red]Skipping large resource:[/] [aqua]{resource.Name}[/] (size: {reader.Length / (1024 * 1024)} MB)");
                continue;
            }
            
            if (target.Resources.Any(r => r.Name == resource.Name))
            {
                UIHelpers.WriteInfo($"[yellow]Skipping duplicate resource:[/] [aqua]{resource.Name}[/]");
                continue;
            }

            var data = reader.ToArray();
            target.Resources.Add(new EmbeddedResource(resource.Name, data, resource.Attributes));
            UIHelpers.WriteInfo($"[grey]Embedded:[/] [aqua]{resource.Name}[/]");
        }

        return (int)ExitCodes.Success;
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
            UIHelpers.WriteInfo("[yellow]No assembly-linked resources to remove. Skipping.[/]");
            return;
        }

        foreach (var resource in toRemove)
        {
            UIHelpers.WriteInfo($"[grey]Removing AssemblyLinkedResource:[/] [aqua]{resource.Name}[/]");
            target.Resources.Remove(resource);
        }
    }

    private static int TryValidatePaths(MergeCommand.Settings settings, out string target, out string source,
        out string output)
    {
        target = Path.GetFullPath(settings.Target);
        source = Path.GetFullPath(settings.Source);
        output = string.Empty;

        UIHelpers.WriteInfo($"[grey]Target Assembly:[/] [lightgreen]{target}[/]");
        UIHelpers.WriteInfo($"[grey]Source Assembly:[/] [lightgreen]{source}[/]");

        if (!File.Exists(target))
            return UIHelpers.PrintError("Target file not found.", (int)ExitCodes.TargetMissing);

        if (!File.Exists(source))
            return UIHelpers.PrintError("Source file not found.", (int)ExitCodes.SourceMissing);

        if (!Utils.IsValidExtension(Path.GetExtension(target)))
            return UIHelpers.PrintError("Target file must be a .exe or a .dll.", (int)ExitCodes.TargetExtensionInvalid);

        if (!Utils.IsValidExtension(Path.GetExtension(source)))
            return UIHelpers.PrintError("Source file must be a .exe or a .dll.", (int)ExitCodes.SourceExtensionInvalid);

        if (string.Equals(target, source, StringComparison.OrdinalIgnoreCase))
            return UIHelpers.PrintError("Target and source must be different files.",
                (int)ExitCodes.TargetEqualsSource);

        output = PathHelpers.ResolveOutputPath(target, settings.Output);

        if (string.Equals(target, output, StringComparison.OrdinalIgnoreCase))
            return UIHelpers.PrintError("Output path must be different from the target file.",
                (int)ExitCodes.OutputEqualsTarget);

        if (string.Equals(source, output, StringComparison.OrdinalIgnoreCase))
            return UIHelpers.PrintError("Output path must be different from the source file.",
                (int)ExitCodes.OutputEqualsSource);

        var outputDir = Path.GetDirectoryName(output);
        if (string.IsNullOrEmpty(outputDir) || !Directory.Exists(outputDir))
            return UIHelpers.PrintError("Output directory does not exist.", (int)ExitCodes.OutputDirectoryInvalid);

        if (!string.Equals(Path.GetExtension(target), Path.GetExtension(output)))
            return UIHelpers.PrintError("Output file extension must match the target file's extension.",
                (int)ExitCodes.OutputExtensionInvalid);

        try
        {
            if (File.Exists(output) && FileHelpers.IsFileLocked(output))
                return UIHelpers.PrintError("The output file is currently in use.",
                    (int)ExitCodes.OutputPermissionDenied);

            using var fs = File.Create(output, 1, FileOptions.DeleteOnClose);
        }
        catch (UnauthorizedAccessException)
        {
            return UIHelpers.PrintError("Insufficient permissions to write to the output file.",
                (int)ExitCodes.OutputPermissionDenied);
        }
        catch (IOException)
        {
            return UIHelpers.PrintError("Output file is locked or in use by another process.",
                (int)ExitCodes.OutputPermissionDenied);
        }
        catch (Exception ex)
        {
            return UIHelpers.PrintFatalError(ex, (int)ExitCodes.FatalError);
        }

        return (int)ExitCodes.Success;
    }
}