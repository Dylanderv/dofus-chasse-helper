using DofusChasseHelper.Domain.External;
using DofusChasseHelper.Infrastructure.Interfaces;
using Microsoft.Extensions.DependencyInjection;

namespace DofusChasseHelper.Infrastructure.Configuration;

public static class IServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection serviceCollection)
    {
        serviceCollection.AddSingleton<IClipboardService, TextCopyService>();
        
        serviceCollection.AddSingleton<HeadlessBrowserHuntSolver>();
        serviceCollection.AddSingleton<IHeadlessBrowserSetup>(sp => sp.GetRequiredService<HeadlessBrowserHuntSolver>());
        serviceCollection.AddSingleton<IHeadlessBrowserHuntSolver>(sp => sp.GetRequiredService<HeadlessBrowserHuntSolver>());
        
        serviceCollection.AddSingleton<IDofusTextProvider, DofusTextProvider>();
        serviceCollection.AddSingleton<IApi, Api>();
        serviceCollection.AddSingleton<IOcrEngine, OcrEngine>();
        serviceCollection.AddSingleton<IScreenshotProvider, ScreenshotProvider>();
        serviceCollection.AddSingleton<Cv2Engine>();

        return serviceCollection;
    }
}