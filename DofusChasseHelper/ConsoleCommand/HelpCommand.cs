using dofus_chasse_helper.ConsoleCommand.Abstractions;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class HelpCommand : IConsoleCommand
{
    public Task<int> Run(params string[] args)
    {
        AnsiConsole.MarkupLine("[italic]- \"help\" to list all commands[/]");
        AnsiConsole.MarkupLine("[italic]- \"exit\" to stop the app[/]");
        
        return Task.FromResult(0);
    }

    public IReadOnlyCollection<Markup> Usage()
    {
        throw new NotImplementedException();
    }
}