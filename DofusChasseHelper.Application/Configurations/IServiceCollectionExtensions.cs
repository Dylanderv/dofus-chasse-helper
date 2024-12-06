using Microsoft.Extensions.DependencyInjection;

namespace DofusChasseHelper.Application.Configurations;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddApplication(this IServiceCollection serviceCollection)
    {
        serviceCollection
            .AddSingleton<StartHuntAction>()
            .AddSingleton<UpdateCurrentPostWithCurrentCharPos>()
            .AddSingleton<GetNextPositionAction>();

        return serviceCollection;
    }
}