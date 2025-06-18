using CLI.Helpers;
using Spectre.Console.Cli;

namespace CLI.Hooks;

public class BeforeHook : ICommandInterceptor
{
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        UIHelpers.PrintBanner();
    }
}