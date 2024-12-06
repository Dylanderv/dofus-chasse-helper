using DofusChasseHelper.Domain;
using Spectre.Console;

namespace dofus_chasse_helper;

public class ConsoleLogger : IConsoleLogger
{
    private readonly ConsoleDisplay _consoleDisplay;

    public ConsoleLogger(ConsoleDisplay consoleDisplay)
    {
        _consoleDisplay = consoleDisplay;
    }

    public void LogInfo(string info)
    {
        AnsiConsole.WriteLine(info);
    }

    public void NotifyNewPosition(Coords coords)
    {
        AnsiConsole.WriteLine($"New position: {coords.X},{coords.Y}");
    }

    public void NotifyHuntSolverState(HuntSolverState state)
    {
        this._consoleDisplay.NotifyState(state);
        
    }
}