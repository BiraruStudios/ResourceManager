using Spectre.Console.Cli;

namespace CLI;

public class BeforeHook : ICommandInterceptor
{
    public void Intercept(CommandContext context, CommandSettings settings)
    {
        UIHelpers.PrintBanner();
    }
}