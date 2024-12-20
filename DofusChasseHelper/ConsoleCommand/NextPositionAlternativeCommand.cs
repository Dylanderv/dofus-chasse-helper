using System.Globalization;
using dofus_chasse_helper.ConsoleCommand.Abstractions;
using DofusChasseHelper.Application;
using DofusChasseHelper.Domain;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class NextPositionAlternativeCommand : IConsoleCommand
{
    private static NumberFormatInfo _numberFormatInfo = new NumberFormatInfo()
    {
        NegativeSign = "-"
    };
    
    private readonly GetNextPositionAction _action;

    public NextPositionAlternativeCommand(GetNextPositionAction action)
    {
        _action = action;
    }

    public string Command => "/travel";
    public string Args => "[[x,y]]";

    public async Task<int> Run(params string[] args)
    {
        Coords? coords = null;
        if (args.Length != 0)
        {
            coords = this.GetParams(args);
            if (coords is null)
                return 1;
        }
        
        await _action.Run(coords);

        return 0;
    }

    public IReadOnlyCollection<Markup> Usage()
    {
        return
        [
            new Markup("Find next position"),
            new Markup("You can force the current position by adding the coordinates after the command"),
            new Markup($"ex: {Command} -3,18")
        ];
    }

    private Coords? GetParams(string[] args)
    {
        if (args.Length != 1)
        {
            AnsiConsole.WriteLine("Invalid number of arguments");
            DisplayUsage();
            return null;
        }

        var strings = args[0].Split(',');
        
        if (strings.Length != 2)
        {
            AnsiConsole.WriteLine("Invalid number of arguments");
            DisplayUsage();
            return null;
        }
        
        if (int.TryParse(strings[0], _numberFormatInfo, out var xPos) is false)
        {
            AnsiConsole.WriteLine("Invalid X parameter");
            DisplayUsage();
            return null;
        }

        if (int.TryParse(strings[1], _numberFormatInfo, out var yPos) is false)
        {
            AnsiConsole.WriteLine("Invalid Y parameter");
            DisplayUsage();
            return null;
        }

        return new Coords(xPos, yPos);
    }

    private void DisplayUsage()
    {
        foreach (var markup in this.Usage())
        {
            AnsiConsole.Write(markup);
            AnsiConsole.WriteLine(string.Empty);
        }
    }
}