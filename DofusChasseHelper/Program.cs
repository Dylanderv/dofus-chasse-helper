using dofus_chasse_helper.ConsoleCommand;
using dofus_chasse_helper.ConsoleCommand.Configurations;
using dofus_chasse_helper.ConsoleCommand.Infrastructure;
using DofusChasseHelper.Application;
using DofusChasseHelper.Application.Configurations;
using DofusChasseHelper.Domain.Configurations;
using DofusChasseHelper.Infrastructure.Configuration;
using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using Microsoft.Extensions.DependencyInjection;
using Spectre.Console;

var serviceCollection = new ServiceCollection();

serviceCollection.AddDomain();
serviceCollection.AddApplication();
serviceCollection.AddInfrastructure();
serviceCollection.AddConsoleCommands();

var serviceProvider = serviceCollection.BuildServiceProvider();

var consoleCommandDispatcher = new ConsoleCommandDispatcher(serviceProvider);

await consoleCommandDispatcher.Dispatch<InitBrowserCommand>();

await consoleCommandDispatcher.Dispatch<HelpCommand>();

AnsiConsole.MarkupLine(string.Empty);
AnsiConsole.MarkupLine(string.Empty);

void RunNext(HotKey hotKey)
{
    if (hotKey.Key == VirtualKeyCode.KEY_Q)
    {
        Console.WriteLine($"HotKey Pressed: Id = {hotKey.Id}, Key = {hotKey.Key}, Modifiers = {hotKey.Modifiers}");
        _ = consoleCommandDispatcher.Dispatch<NextPositionCommand>([]).Result;
    }
    if (hotKey.Key == VirtualKeyCode.KEY_A)
    {
        Console.WriteLine($"HotKey Pressed: Id = {hotKey.Id}, Key = {hotKey.Key}, Modifiers = {hotKey.Modifiers}");
        _ = consoleCommandDispatcher.Dispatch<StartHuntCommand>([]).Result;
    }
}

using var hotKeyManager = new HotKeyManager();
hotKeyManager.HotKeyPressed.Subscribe(RunNext);
hotKeyManager.Register(VirtualKeyCode.KEY_Q, Modifiers.Control | Modifiers.Alt);
hotKeyManager.Register(VirtualKeyCode.KEY_A, Modifiers.Control | Modifiers.Alt);

string response = string.Empty;

while (response.Equals("exit", StringComparison.OrdinalIgnoreCase) is false)
{
    AnsiConsole.MarkupLine(string.Empty);
    AnsiConsole.MarkupLine(string.Empty);
    response = AnsiConsole.Ask<string>("> ");
    var argv = response.Split(' ');
    
    var command = argv[0];
    var commandArgs = argv[1..];
    
    try
    {
        switch (command)
        {
            case "help":
                await consoleCommandDispatcher.Dispatch<HelpCommand>(commandArgs);
                break;
            case "start":
                await consoleCommandDispatcher.Dispatch<StartHuntCommand>(commandArgs);
                break;
            case "next":
                await consoleCommandDispatcher.Dispatch<NextPositionCommand>(commandArgs);
                break;
            case "exit":
                await consoleCommandDispatcher.Dispatch<ExitCommand>(commandArgs);
                break;
            default:
                AnsiConsole.MarkupLine("Invalid command");
                await consoleCommandDispatcher.Dispatch<HelpCommand>(commandArgs);
                break;
        }

    }
    catch (Exception e)
    {
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine("An error occured while running the command");
        AnsiConsole.WriteException(e);
        
        AnsiConsole.MarkupLine(string.Empty);
        AnsiConsole.MarkupLine(string.Empty);
        
        await consoleCommandDispatcher.Dispatch<HelpCommand>();
    }
}


// await Action(host.Services);

static async Task Action(IServiceProvider hostProvider)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    var command = provider.GetRequiredService<StartHuntAction>();
    
    await command.Run();
    
    Console.WriteLine("Done");
}