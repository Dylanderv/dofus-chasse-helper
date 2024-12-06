using System.Runtime.CompilerServices;
using dofus_chasse_helper.ConsoleCommand.Abstractions;
using DofusChasseHelper.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace dofus_chasse_helper.ConsoleCommand.Configurations;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddConsoleCommands(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<HelpCommand>()
            .AddSingleton<IConsoleCommand, HelpCommand>(sp => sp.GetRequiredService<HelpCommand>())
            .AddSingleton<StartHuntCommand>()
            .AddSingleton<IConsoleCommand, StartHuntCommand>(sp => sp.GetRequiredService<StartHuntCommand>())
            .AddSingleton<NextPositionCommand>()
            .AddSingleton<NextPositionAlternativeCommand>()
            .AddSingleton<NextPositionFromClipboardCommand>()
            .AddSingleton<IConsoleCommand, NextPositionCommand>(sp => sp.GetRequiredService<NextPositionCommand>())
            .AddSingleton<UpdatePosWithCurrentCharPosCommand>()
            .AddSingleton<IConsoleCommand, UpdatePosWithCurrentCharPosCommand>(sp => sp.GetRequiredService<UpdatePosWithCurrentCharPosCommand>())
            .AddSingleton<ExitCommand>()
            .AddSingleton<IConsoleCommand, ExitCommand>(sp => sp.GetRequiredService<ExitCommand>())
            .AddSingleton<InitBrowserCommand>()
            .AddSingleton<IConsoleCommand, InitBrowserCommand>(sp => sp.GetRequiredService<InitBrowserCommand>());

        serviceCollection.AddSingleton<IConsoleLogger, ConsoleLogger>();
        serviceCollection.AddSingleton<ConsoleDisplay>();
        
        return serviceCollection;
    }
}