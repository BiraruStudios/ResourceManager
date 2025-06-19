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
        WriteInfo("[red bold]Error:[/]");
        WriteInfo($"[yellow]{message}[/]");
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
        WriteInfo("[red bold]Fatal error occurred:[/]");
        WriteInfo($"[grey]{ex.GetType().Name}[/]: [yellow]{ex.Message}[/]");
        WriteSpacer();
        AnsiConsole.Write(new Rule().RuleStyle("red dim").Centered());
        WriteSpacer();
        return code;
    }

    public static void WriteInfo(string message)
    {
        AnsiConsole.Write(new Markup($"[grey]{message}[/]").Centered());
    }

    public static void WriteSpacer()
    {
        AnsiConsole.Write(new Text("").Centered());
    }
}