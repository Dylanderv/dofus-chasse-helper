namespace DofusChasseHelper.Domain;

public interface IConfigurationProvider
{
    string GetCharacterName();
    bool GetCharacterScopedScreenshotSetting();
}