using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;

namespace DofusChasseHelper.Application;

public class GetNextPositionAction
{
    private readonly HuntSolver _huntSolver;
    private readonly IOcrEngine _ocrEngine;
    private readonly IScreenshotProvider _screenshotProvider;
    private readonly IHeadlessBrowserHuntSolver _headlessBrowserHuntSolver;
    private readonly IClipboardService _clipboardService;
    private readonly IConsoleLogger _consoleLogger;

    public GetNextPositionAction(
        HuntSolver huntSolver,
        IOcrEngine ocrEngine,
        IScreenshotProvider screenshotProvider,
        IHeadlessBrowserHuntSolver headlessBrowserHuntSolver,
        IClipboardService clipboardService,
        IConsoleLogger consoleLogger)
    {
        _huntSolver = huntSolver;
        _ocrEngine = ocrEngine;
        _screenshotProvider = screenshotProvider;
        _headlessBrowserHuntSolver = headlessBrowserHuntSolver;
        _clipboardService = clipboardService;
        _consoleLogger = consoleLogger;
    }
    
    public async Task Run(Coords? forcedCurrentPosition = null)
    {
        if (forcedCurrentPosition is not null)
        {
            this._huntSolver.ForceCurrentPosition(forcedCurrentPosition, this._consoleLogger);
        }
        
        await this._huntSolver.GetNextHint(this._screenshotProvider, this._ocrEngine, this._consoleLogger);
        await this._huntSolver.GetNextPosition(this._headlessBrowserHuntSolver, this._clipboardService, this._consoleLogger);
    }
}