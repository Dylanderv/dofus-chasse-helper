using dofus_chasse_helper.ConsoleCommand.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand.Infrastructure;

public class ConsoleCommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public ConsoleCommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<int> Dispatch<TConsoleCommand>(params string[] args) where TConsoleCommand : IConsoleCommand
    {
        try
        {
            var consoleCommand = this._serviceProvider.GetRequiredService<TConsoleCommand>();

            return await consoleCommand.Run(args);
        }
        catch (Exception e)
        {
            AnsiConsole.MarkupLine(string.Empty);
            AnsiConsole.MarkupLine(string.Empty);
            AnsiConsole.MarkupLine("An error occured while running the command");
            AnsiConsole.WriteException(e);

            AnsiConsole.MarkupLine(string.Empty);
            AnsiConsole.MarkupLine(string.Empty);

            return 1;
        }
    }
}