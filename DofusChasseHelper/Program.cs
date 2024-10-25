﻿using dofus_chasse_helper;
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


// await Action(host.Services);

static async Task Action(IServiceProvider hostProvider)
{
    using IServiceScope serviceScope = hostProvider.CreateScope();
    IServiceProvider provider = serviceScope.ServiceProvider;

    var command = provider.GetRequiredService<StartHuntAction>();
    
    await command.Run();
    
    Console.WriteLine("Done");
}