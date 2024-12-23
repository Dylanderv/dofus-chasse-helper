namespace DofusChasseHelper.Domain;

public interface IConfigurationProvider
{
    string GetCharacterName();
    bool GetCharacterScopedScreenshotSetting();
    bool ShouldUseFullscreenScreenshot();
    BrowserHuntSolver GetHuntSolverToUse();
    HuntBoxApproximation GetHuntBoxApproximationSettings();
    OcrSettings GetOcrSettings();
}