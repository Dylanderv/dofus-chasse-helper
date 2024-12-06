using System.Globalization;
using dofus_chasse_helper.ConsoleCommand.Abstractions;
using DofusChasseHelper.Application;
using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class NextPositionFromClipboardCommand : IConsoleCommand
{
    private static NumberFormatInfo _numberFormatInfo = new NumberFormatInfo()
    {
        NegativeSign = "-"
    };
    
    private readonly GetNextPositionAction _action;
    private readonly IClipboardService _clipboardService;

    public NextPositionFromClipboardCommand(GetNextPositionAction action, IClipboardService clipboardService)
    {
        _action = action;
        _clipboardService = clipboardService;
    }

    public string Command => "next-from-clipboard";

    public async Task<int> Run(params string[] args)
    {
        var clipboard = await this._clipboardService.GetFromClipboard();

        if (clipboard is null)
        {
            return 1;
        }
        
        Coords? coords = this.GetParams(clipboard);
        if (coords is null)
            return 1;
        
        await _action.Run(coords);

        return 0;
    }

    public IReadOnlyCollection<Markup> Usage()
    {
        return
        [
            new Markup("Find next position using the /travel command in the clipboard as start position"),
        ];
    }

    private Coords? GetParams(string args)
    {
        var split = args.Split(' ');

        if (split[0] != "/travel")
        {
            AnsiConsole.WriteLine("No /travel command found in the clipboard");
            return null;
        }
        
        string[] strings;
        if (split.Length == 2)
        {
            char splitParam = ',';
            strings = split[1].Split(splitParam);
        }
        else
        {
            strings = split[1..];
        }
        
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