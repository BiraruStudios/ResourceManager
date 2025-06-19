using Spectre.Console;

namespace CLI.Helpers;

public static class UIHelpers
{
    public static void PrintBanner()
    {
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("grey").Centered());
        WriteSpacer();
        AnsiConsole.Write(new FigletText("ResourceManager").Centered().Color(Color.Yellow));
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("grey").Centered());
        WriteSpacer();
    }

    public static int PrintError(string message, int code)
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

    public static int PrintFatalError(Exception ex, int code)
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

    public static void WriteSpacer()
    {
        AnsiConsole.Write(new Text("").Centered());
    }
}