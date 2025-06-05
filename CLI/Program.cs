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
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new FigletText("ResourceMerger").Centered().Color(Color.Yellow));
        AnsiConsole.WriteLine();
        AnsiConsole.Write(new Rule().RuleStyle("grey").Centered());
        AnsiConsole.WriteLine();

        var exeName = Path.GetFileName(Process.GetCurrentProcess().MainModule?.FileName ?? "");

        if (args.Length != 2)
        {
            AnsiConsole.Write(
                new Markup(
                        $"[grey]Usage:[/] [bold yellow]{exeName}[/] [lightgreen]<TargetAssembly>[/] [lightgreen]<SourceAssembly>[/]")
                    .Centered());
            return;
        }

        var targetAssembly = Path.GetFullPath(args[0]);
        var sourceAssembly = Path.GetFullPath(args[1]);

        if (!File.Exists(targetAssembly))
        {
            AnsiConsole.Write(new Markup("[red bold]Error:[/] [yellow]Target file not found.[/]").Centered());
            return;
        }

        if (!File.Exists(sourceAssembly))
        {
            AnsiConsole.Write(new Markup("[red bold]Error:[/] [yellow]Source file not found.[/]").Centered());
            return;
        }

        var targetExtension = Path.GetExtension(targetAssembly);
        var sourceExtension = Path.GetExtension(sourceAssembly);

        if (!IsValidExtension(targetExtension))
        {
            AnsiConsole.Write(new Markup("[red bold]Error:[/] [yellow]Target file must be a .exe or .dll.[/]")
                .Centered());
            return;
        }

        if (!IsValidExtension(sourceExtension))
        {
            AnsiConsole.Write(new Markup("[red bold]Error:[/] [yellow]Source file must be a .exe or .dll.[/]")
                .Centered());
            return;
        }

        AnsiConsole.Write(new Markup($"[grey]Target Assembly:[/] [lightgreen]{targetAssembly}[/]").Centered());
        AnsiConsole.Write(new Markup($"[grey]Source Assembly:[/] [lightgreen]{sourceAssembly}[/]").Centered());

        var mergedOutputPath = Path.Combine(
            Path.GetDirectoryName(targetAssembly)!,
            Path.GetFileNameWithoutExtension(targetAssembly) + "_Merged" + targetExtension
        );

        var targetModule = ModuleDefMD.Load(targetAssembly);
        var targetOptions = new ModuleWriterOptions(targetModule);
        var sourceModule = ModuleDefMD.Load(sourceAssembly);

        targetOptions.MetadataOptions.Flags |= MetadataFlags.KeepOldMaxStack;

        EmbedResources(targetModule, sourceModule);
        RemoveAssemblyLinkedResources(targetModule, sourceModule);

        targetModule.Write(mergedOutputPath, targetOptions);
        AnsiConsole.Write(new Markup($"[lightgreen]Merged output saved to: {mergedOutputPath}[/]").Centered());
    }

    private static bool IsValidExtension(string ext)
    {
        return Array.Exists(validExtensions, e => string.Equals(e, ext, StringComparison.OrdinalIgnoreCase));
    }

    private static void EmbedResources(ModuleDefMD target, ModuleDefMD source)
    {
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
        for (var i = target.Resources.Count - 1; i >= 0; i--)
            if (target.Resources[i] is AssemblyLinkedResource linked &&
                linked.Assembly.Name == source.Assembly.Name)
            {
                AnsiConsole.Write(new Markup($"[grey]Removing AssemblyLinkedResource:[/] [aqua]{linked.Name}[/]")
                    .Centered());
                target.Resources.RemoveAt(i);
            }
    }
}