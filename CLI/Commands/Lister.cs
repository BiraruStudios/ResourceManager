using System.Collections;
using System.Resources;
using dnlib.DotNet;
using Spectre.Console;

namespace CLI.Commands;

public static class Lister
{
    /*
     * Return Codes:
     * 0 = Success
     * 1 = Target file missing
     * 2 = Invalid target extension
     * 3 = Fatal Error
     * 4 = No resources to list
     * 5 = Failed to parse .resources
     */

    public static int Execute(ListCommand.Settings settings)
    {
        var validation = TryValidatePaths(settings, out var targetPath);
        if (validation != 0)
            return validation;

        try
        {
            var targetModule = ModuleDefMD.Load(targetPath);

            return PrintResources(targetModule);
        }
        catch (Exception e)
        {
            return UIHelpers.PrintFatalError(e, 3);
        }
    }

    private static int PrintResources(ModuleDefMD module)
    {
        var resources = module.Resources.OfType<EmbeddedResource>().ToList();

        if (resources.Count == 0)
            return UIHelpers.PrintError("No resources to list.", 4);

        UIHelpers.WriteSpacer();

        foreach (var resource in resources)
        {
            AnsiConsole.Write(new Markup($"[aqua]{resource.Name}[/]").Centered());

            if (!resource.Name.EndsWith(".resources", StringComparison.OrdinalIgnoreCase)) continue;

            using var ms = new MemoryStream();
            resource.CreateReader().CopyTo(ms);
            ms.Position = 0;

            try
            {
                using var resourceSet = new ResourceSet(ms);
                foreach (DictionaryEntry entry in resourceSet)
                    AnsiConsole.Write(new Markup($"[grey]- {entry.Key}[/]").Centered());
            }
            catch (NotSupportedException)
            {
                AnsiConsole.Write(new Markup("[yellow]Skipped serialized resource entries[/]").Centered());
            }
            catch (Exception ex)
            {
                return UIHelpers.PrintFatalError(ex, 5);
            }
        }

        UIHelpers.WriteSpacer();

        return 0;
    }

    private static int TryValidatePaths(ListCommand.Settings settings, out string target)
    {
        target = Path.GetFullPath(settings.Target);

        AnsiConsole.Write(new Markup($"[grey]Target Assembly:[/] [lightgreen]{target}[/]").Centered());

        if (!File.Exists(target))
            return UIHelpers.PrintError("Target file not found.", 1);

        // ReSharper disable once ConvertIfStatementToReturnStatement
        if (!Utils.IsValidExtension(Path.GetExtension(target)))
            return UIHelpers.PrintError("Target file must be a .exe or a .dll.", 2);

        return 0;
    }
}