using dofus_chasse_helper.ConsoleCommand.Abstractions;
using DofusChasseHelper.Application;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class UpdatePosWithCurrentCharPosCommand : IConsoleCommand
{
    private readonly UpdateCurrentPostWithCurrentCharPos _action;

    public UpdatePosWithCurrentCharPosCommand(UpdateCurrentPostWithCurrentCharPos action)
    {
        _action = action;
    }

    public string Command => "update";
    public async Task<int> Run(params string[] args)
    {
        await this._action.Run();

        return 0;
    }

    public IReadOnlyCollection<Markup> Usage()
    {
        return
        [
            new Markup("Update current position with current character pos")
        ];
    }
}