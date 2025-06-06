using System.Diagnostics;
using dnlib.DotNet;
using dnlib.DotNet.Writer;
using Spectre.Console;

namespace CLI;

public static class Program
{
    private static readonly string[] validExtensions = [".exe", ".dll"];

    public static void Main(string[] args)
    {
        PrintBanner();

        var exeName = GetExecutableName();

        if (!TryGetValidatedPaths(args, exeName, out var targetAssembly, out var sourceAssembly))
            return;

        var mergedOutputPath = Path.Combine(
            Path.GetDirectoryName(targetAssembly)!,
            Path.GetFileNameWithoutExtension(targetAssembly) + "_Merged" + Path.GetExtension(targetAssembly)
        );

        try
        {
            var targetModule = ModuleDefMD.Load(targetAssembly);
            var targetOptions = new ModuleWriterOptions(targetModule)
            {
                MetadataOptions = { Flags = MetadataFlags.KeepOldMaxStack }
            };

            var sourceModule = ModuleDefMD.Load(sourceAssembly);

            EmbedResources(targetModule, sourceModule);
            RemoveAssemblyLinkedResources(targetModule, sourceModule);

            targetModule.Write(mergedOutputPath, targetOptions);

            AnsiConsole.Write(new Markup($"[lightgreen]Merged output saved to: {mergedOutputPath}[/]").Centered());
            WriteSpacer();
        }
        catch (Exception ex)
        {
            PrintFatalError(ex);
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

    private static bool TryGetValidatedPaths(string[] args, string exeName, out string target, out string source)
    {
        target = source = string.Empty;

        if (args.Length != 2)
        {
            AnsiConsole.Write(new Markup(
                    $"[grey]Usage:[/] [bold yellow]{exeName}[/] [lightgreen]<TargetAssembly>[/] [lightgreen]<SourceAssembly>[/]")
                .Centered());
            return false;
        }

        target = Path.GetFullPath(args[0]);
        source = Path.GetFullPath(args[1]);

        if (!File.Exists(target))
        {
            PrintError("Target file not found.");
            return false;
        }

        if (!File.Exists(source))
        {
            PrintError("Source file not found.");
            return false;
        }

        if (!IsValidExtension(Path.GetExtension(target)))
        {
            PrintError("Target file must be a .exe or .dll.");
            return false;
        }

        if (!IsValidExtension(Path.GetExtension(source)))
        {
            PrintError("Source file must be a .exe or .dll.");
            return false;
        }

        WriteSpacer();
        AnsiConsole.Write(new Markup($"[grey]Target Assembly:[/] [lightgreen]{target}[/]").Centered());
        AnsiConsole.Write(new Markup($"[grey]Source Assembly:[/] [lightgreen]{source}[/]").Centered());

        return true;
    }

    private static bool IsValidExtension(string ext)
    {
        return Array.Exists(validExtensions, e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase));
    }

    private static void EmbedResources(ModuleDefMD target, ModuleDefMD source)
    {
        WriteSpacer();

        foreach (var resource in source.Resources)
            if (resource is EmbeddedResource embedded)
            {
                AnsiConsole.Write(new Markup($"[grey]Embedding:[/] [aqua]{embedded.Name}[/]").Centered());
                var data = embedded.CreateReader().ToArray();

                target.Resources.Add(new EmbeddedResource(
                    embedded.Name,
                    data,
                    embedded.Attributes
                ));
            }
    }

    private static void RemoveAssemblyLinkedResources(ModuleDefMD target, ModuleDefMD source)
    {
        WriteSpacer();

        for (var i = target.Resources.Count - 1; i >= 0; i--)
            if (target.Resources[i] is AssemblyLinkedResource linked &&
                string.Equals(linked.Assembly.Name, source.Assembly.Name, StringComparison.OrdinalIgnoreCase))
            {
                AnsiConsole.Write(new Markup($"[grey]Removing AssemblyLinkedResource:[/] [aqua]{linked.Name}[/]")
                    .Centered());
                target.Resources.RemoveAt(i);
            }
    }

    private static void PrintError(string message)
    {
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
        AnsiConsole.Write(new Markup("[red bold]Error occurred:[/]").Centered());
        AnsiConsole.Write(new Markup($"[yellow]{message}[/]").Centered());
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
    }
    
    private static void PrintFatalError(Exception ex)
    {
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
        AnsiConsole.Write(new Markup("[red bold]Fatal error occurred:[/]").Centered());
        AnsiConsole.Write(new Markup($"[grey]{ex.GetType().Name}[/]: [yellow]{ex.Message}[/]").Centered());
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
    }

    private static void WriteSpacer()
    {
        AnsiConsole.Write(new Text("").Centered());
    }
}