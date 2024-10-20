using Microsoft.Extensions.DependencyInjection;

namespace DofusChasseHelper.Domain.Configurations;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<HuntSolver>();

        return serviceCollection;
    }
}