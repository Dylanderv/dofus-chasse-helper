using dofus_chasse_helper.ConsoleCommand.Abstractions;
using DofusChasseHelper.Application;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class StartHuntCommand : IConsoleCommand
{
    private readonly StartHuntAction _action;

    public StartHuntCommand(StartHuntAction action)
    {
        _action = action;
    }
    
    public string Command => "start";

    public async Task<int> Run(params string[] args)
    {
        await _action.Run();

        return 0;
    }

    public IReadOnlyCollection<Markup> Usage()
    {
        return
        [
            new Markup("Initialize the hunt from the start location in the hunt box and find the first hint location")
        ];
    }
}