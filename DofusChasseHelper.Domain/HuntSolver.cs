using System.Runtime.CompilerServices;
using System.Text.Json;
using DofusChasseHelper.Domain.Exceptions;
using DofusChasseHelper.Domain.External;

namespace DofusChasseHelper.Domain;

public class HuntSolver
{
    private Coords? CurrentPosition { get; set; }
    private Hint? NextHint { get; set; }

    public async Task Initialize(IScreenshotProvider screenshotProvider, IOcrEngine ocrEngine, IConsoleLogger consoleLogger)
    {
        var screenShot = screenshotProvider.ScreenShot();

        (Coords startPosition, Hint firstHint) = await ocrEngine.GetFirstHint(screenShot);

        this.UpdatePosition(startPosition, consoleLogger);

        this.NextHint = firstHint;
        
        consoleLogger.LogInfo($"Hunt start position: {startPosition.X},{startPosition.Y}");
        consoleLogger.LogInfo($"First hint: {firstHint.SearchedObject}");
        consoleLogger.LogInfo($"Direction: {Enum.GetName(firstHint.Direction)}");
    }

    public async Task GetNextHint(IScreenshotProvider screenshotProvider, IOcrEngine ocrEngine, IConsoleLogger consoleLogger)
    {
        if (this.CurrentPosition is null)
            throw new MissingRequiredStateException(nameof(this.CurrentPosition));
        
        var screenShot = screenshotProvider.ScreenShot();

        this.NextHint = await ocrEngine.GetNextHint(screenShot);
        
        consoleLogger.LogInfo($"Hunt current position: {this.CurrentPosition.X},{this.CurrentPosition.Y}");
        consoleLogger.LogInfo($"Next hint: {this.NextHint.SearchedObject}");
        consoleLogger.LogInfo($"Direction: {Enum.GetName(this.NextHint.Direction)}");
    }

    public async Task GetNextPosition(IHeadlessBrowserHuntSolver headlessBrowserHuntSolver, IClipboardService clipboardService, IConsoleLogger consoleLogger)
    {
        if (this.NextHint is null || this.CurrentPosition is null)
        {
            throw new MissingRequiredStateException(this.NextHint is null
                ? nameof(this.NextHint)
                : nameof(this.CurrentPosition));
        }

        Coords destination;
        
        try
        {
            destination = await headlessBrowserHuntSolver.SolveWithDofusPourLesNoobs(this.CurrentPosition,
                this.NextHint.Direction, this.NextHint.SearchedObject);
        }
        catch (Exception e)
        {
            throw new MissingHintException(this.NextHint, e);
        }

        var autopilotCommand = this.FormatAutopilotCommand(destination);

        await clipboardService.SetInClipboard(autopilotCommand);

        this.UpdatePosition(destination, consoleLogger);

        
        consoleLogger.LogInfo($"Found hint position: {this.CurrentPosition.X},{this.CurrentPosition.Y}");
    }

    private string FormatAutopilotCommand(Coords coords) => $"/travel {coords.X},{coords.Y}";

    public void ForceCurrentPosition(Coords forcedCurrentPosition, IConsoleLogger consoleLogger)
    {
        this.UpdatePosition(forcedCurrentPosition, consoleLogger);

        consoleLogger.LogInfo($"Hunt current position forced at: {this.CurrentPosition.X},{this.CurrentPosition.Y}");
    }

    public async Task SetCurrentPositionWithCurrentCharPosition(IScreenshotProvider screenshotProvider, IOcrEngine ocrEngine, IConsoleLogger consoleLogger, IHeadlessBrowserHuntSolver headlessBrowserHuntSolver)
    {
        var screenShot = screenshotProvider.ScreenShot();

        var currentPos = await ocrEngine.GetCurrentPos(screenShot);

        this.UpdatePosition(currentPos, consoleLogger);

        await headlessBrowserHuntSolver.UpdatePosForDofusPourLesNoobs(currentPos);
        
        consoleLogger.LogInfo($"New position: {currentPos.X},{currentPos.Y}");
    }

    private void UpdatePosition(Coords newCoords, IConsoleLogger logger)
    {
        this.CurrentPosition = newCoords;
        logger.NotifyHuntSolverState(this.BuildState());
    }

    private HuntSolverState BuildState()
    {
        return new HuntSolverState(this.CurrentPosition, this.NextHint);
    }
}

public record Hint(string SearchedObject, Arrow Direction);

public record HuntSolverState(Coords? CurrentPosition, Hint? NextHint);