using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;

namespace DofusChasseHelper.Application;

public class StartHuntAction
{
    private readonly HuntSolver _huntSolver;
    private readonly IScreenshotProvider _screenshotProvider;
    private readonly IOcrEngine _ocrEngine;
    private readonly IHeadlessBrowserHuntSolver _headlessBrowserHuntSolver;
    private readonly IClipboardService _clipboardService;
    private readonly IConfigurationProvider _configurationProvider;
    private readonly IConsoleLogger _consoleLogger;

    public StartHuntAction(
        HuntSolver huntSolver,
        IScreenshotProvider screenshotProvider, 
        IOcrEngine ocrEngine, 
        IHeadlessBrowserHuntSolver headlessBrowserHuntSolver,
        IClipboardService clipboardService,
        IConfigurationProvider configurationProvider,
        IConsoleLogger consoleLogger)
    {
        _huntSolver = huntSolver;
        _screenshotProvider = screenshotProvider;
        _ocrEngine = ocrEngine;
        _headlessBrowserHuntSolver = headlessBrowserHuntSolver;
        _clipboardService = clipboardService;
        _configurationProvider = configurationProvider;
        _consoleLogger = consoleLogger;
    }

    public async Task Run()
    {
        await this._huntSolver.Initialize(this._screenshotProvider, this._ocrEngine, this._consoleLogger, this._configurationProvider);
        
        await this._huntSolver.GetNextPosition(this._headlessBrowserHuntSolver, this._clipboardService, this._consoleLogger);
    }
}