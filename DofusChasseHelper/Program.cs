using dofus_chasse_helper;
using dofus_chasse_helper.ConsoleCommand;
using dofus_chasse_helper.ConsoleCommand.Abstractions;
using dofus_chasse_helper.ConsoleCommand.Configurations;
using dofus_chasse_helper.ConsoleCommand.Infrastructure;
using DofusChasseHelper.Application.Configurations;
using DofusChasseHelper.Domain.Configurations;
using DofusChasseHelper.Infrastructure.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var serviceCollection = new ServiceCollection();

serviceCollection.AddDomain();
serviceCollection.AddApplication();
serviceCollection.AddInfrastructure();
serviceCollection.AddConsoleCommands();

var serviceProvider = serviceCollection.BuildServiceProvider();

var panel = new Panel("Hello World");

AnsiConsole.Write(panel);

var consoleCommandDispatcher = new ConsoleCommandDispatcher(serviceProvider);

await consoleCommandDispatcher.Dispatch<InitBrowserCommand>();

await consoleCommandDispatcher.Dispatch<HelpCommand>();

using var hotkeyHandler = new HotkeyHandler();
hotkeyHandler.SetupHotkeys(consoleCommandDispatcher);

AnsiConsole.MarkupLine(string.Empty);
AnsiConsole.MarkupLine(string.Empty);


string response = string.Empty;

while (response.Equals("exit", StringComparison.OrdinalIgnoreCase) is false)
{
    AnsiConsole.MarkupLine(string.Empty);
    AnsiConsole.MarkupLine(string.Empty);
    response = AnsiConsole.Ask<string>("> ");
    var argv = response.Split(' ');
    
    var command = argv[0];
    var commandArgs = argv[1..];

    switch (command)
    {
        case "help":
        case "h":
            await consoleCommandDispatcher.Dispatch<HelpCommand>(commandArgs);
            break;
        case "start":
        case "a":
            await consoleCommandDispatcher.Dispatch<StartHuntCommand>(commandArgs);
            break;
        case "next":
        case "q":
            await consoleCommandDispatcher.Dispatch<NextPositionCommand>(commandArgs);
            break;
        case "/travel":
            await consoleCommandDispatcher.Dispatch<NextPositionAlternativeCommand>(commandArgs);
            break;
        case "next-from-clipboard":
        case "r":
            await consoleCommandDispatcher.Dispatch<NextPositionFromClipboardCommand>(commandArgs);
            break;
        case "update":
        case "z":
            await consoleCommandDispatcher.Dispatch<UpdatePosWithCurrentCharPosCommand>(commandArgs);
            break;
        case "exit":
            await consoleCommandDispatcher.Dispatch<ExitCommand>(commandArgs);
            break;
        case "short-mode":
            await ShortMode(consoleCommandDispatcher);
            break;
        default:
            AnsiConsole.MarkupLine("Invalid command");
            await consoleCommandDispatcher.Dispatch<HelpCommand>(commandArgs);
            break;
    }

    static async Task RunCommandWithoutException<TCommand>(ConsoleCommandDispatcher consoleCommandDispatcher)
        where TCommand : IConsoleCommand
    {
        try
        {
            await consoleCommandDispatcher.Dispatch<TCommand>([]);
        }
        catch
        {
            // ignored
        }
    }
    
static async Task ShortMode(ConsoleCommandDispatcher consoleCommandDispatcher)
{
    char lastKey = ' ';
    do
    {
        var consoleKeyInfo = Console.ReadKey();
        lastKey = consoleKeyInfo.KeyChar;
        switch (lastKey)
        {
            case 'h':
                await RunCommandWithoutException<HelpCommand>(consoleCommandDispatcher);
                break;
            case 'a':
                await RunCommandWithoutException<StartHuntCommand>(consoleCommandDispatcher);
                break;
            case 'q':
                await RunCommandWithoutException<NextPositionCommand>(consoleCommandDispatcher);
                break;
            case 'r':
                await RunCommandWithoutException<NextPositionFromClipboardCommand>(consoleCommandDispatcher);
                break;
            case 'z':
                await RunCommandWithoutException<UpdatePosWithCurrentCharPosCommand>(consoleCommandDispatcher);
                break;
        }
    } while (lastKey != 'x');
}
}
