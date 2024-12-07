using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;

namespace DofusChasseHelper.Application;

public class UpdateCurrentPostWithCurrentCharPos
{
    private readonly HuntSolver _huntSolver;
    private readonly IScreenshotProvider _screenshotProvider;
    private readonly IOcrEngine _ocrEngine;
    private readonly IHeadlessBrowserHuntSolver _headlessBrowserHuntSolver;
    private readonly IClipboardService _clipboardService;
    private readonly IConsoleLogger _consoleLogger;

    public UpdateCurrentPostWithCurrentCharPos(
        HuntSolver huntSolver,
        IScreenshotProvider screenshotProvider, 
        IOcrEngine ocrEngine, 
        IHeadlessBrowserHuntSolver headlessBrowserHuntSolver,
        IClipboardService clipboardService,
        IConsoleLogger consoleLogger)
    {
        _huntSolver = huntSolver;
        _screenshotProvider = screenshotProvider;
        _ocrEngine = ocrEngine;
        _headlessBrowserHuntSolver = headlessBrowserHuntSolver;
        _clipboardService = clipboardService;
        _consoleLogger = consoleLogger;
    }

    public async Task Run()
    {
        await this._huntSolver.SetCurrentPositionWithCurrentCharPosition(this._screenshotProvider, this._ocrEngine, this._consoleLogger, this._headlessBrowserHuntSolver);
    }
}