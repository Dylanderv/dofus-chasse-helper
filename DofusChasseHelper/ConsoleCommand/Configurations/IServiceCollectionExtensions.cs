using System.Runtime.CompilerServices;
using DofusChasseHelper.Domain;
using Microsoft.Extensions.DependencyInjection;

namespace dofus_chasse_helper.ConsoleCommand.Configurations;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddConsoleCommands(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<HelpCommand>()
            .AddSingleton<StartHuntCommand>()
            .AddSingleton<NextPositionCommand>()
            .AddSingleton<ExitCommand>()
            .AddSingleton<InitBrowserCommand>();

        serviceCollection.AddSingleton<IConsoleLogger, ConsoleLogger>();
        
        return serviceCollection;
    }
}