using dofus_chasse_helper.ConsoleCommand.Abstractions;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class HelpCommand : IConsoleCommand
{
    private readonly IServiceProvider _serviceProvider;

    public HelpCommand(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }
    public string Command => "help";

    public Task<int> Run(params string[] args)
    {
        var allCommands = _serviceProvider.GetRequiredService<IEnumerable<IConsoleCommand>>();

        foreach (var consoleCommand in allCommands)
        {
            if (consoleCommand.Command is "none") continue;
            
            AnsiConsole.MarkupLine($"[underline][olive]{consoleCommand.Command}[/][/] [green]{consoleCommand.Args}[/]");
            
            foreach (var markup in consoleCommand.Usage())
            {
                AnsiConsole.Write(markup);
                AnsiConsole.WriteLine(string.Empty);
            }
            
            AnsiConsole.WriteLine(string.Empty);
        }
        
        return Task.FromResult(0);
    }

    public IReadOnlyCollection<Markup> Usage()
    {
        return
        [
            new Markup("Shows all available commands")
        ];
    }
}