using System.Text.Json;
using System.Text.Json.Serialization;
using DofusChasseHelper.Domain;

namespace DofusChasseHelper.Infrastructure;

public class ConfigurationProvider : IConfigurationProvider
{
    public string GetCharacterName()
    {
        var config = ReadConfig();

        return config.CharacterName;
    }

    private static Config ReadConfig()
    {
        var rawConfig = File.ReadAllText(@".\configuration.json");
        var config = JsonSerializer.Deserialize<Config>(rawConfig);

        if (config is null)
        {
            throw new Exception("Unable to read configuration");
        }

        return config;
    }

    public bool GetCharacterScopedScreenshotSetting()
    {
        var config = ReadConfig();

        return config.CharacterScopedScreenshot;
    }

    public bool ShouldUseFullscreenScreenshot()
    {
        var config = ReadConfig();

        return config.ShouldUseFullscreenScreenshot;
    }

    public BrowserHuntSolver GetHuntSolverToUse()
    {
        var config = ReadConfig();

        return Enum.Parse<BrowserHuntSolver>(config.BrowserHuntSolver);
    }

    public HuntBoxApproximation GetHuntBoxApproximationSettings()
    {
        var config = ReadConfig();

        return new HuntBoxApproximation(config.HuntBoxApproximation.Width, config.HuntBoxApproximation.Height);
    }

    public OcrSettings GetOcrSettings()
    {
        var config = ReadConfig();

        return new OcrSettings(
            HeaderTexts: config.Ocr.HeaderTexts,
            FooterTexts: config.Ocr.FooterTexts,
            CurrentTexts: config.Ocr.CurrentTexts,
            StartTexts: config.Ocr.StartTexts,
            CurrentPositionTexts: config.Ocr.CurrentPositionTexts
        );
    }
}

public class Config
{
    [JsonPropertyName("characterName")]
    public string CharacterName { get; set; }
    
    [JsonPropertyName("characterScopedScreenshot")]
    public bool CharacterScopedScreenshot { get; set; }
    
    [JsonPropertyName("shouldUseFullscreenScreenshot")]
    public bool ShouldUseFullscreenScreenshot { get; set; }

    [JsonPropertyName("huntBoxApproximation")]
    public HuntBoxApproximationDto HuntBoxApproximation { get; set; }

    [JsonPropertyName("browserHuntSolver")]
    public string BrowserHuntSolver { get; set; }
    
    [JsonPropertyName("ocr")]
    public OcrSettingsDto Ocr { get; set; } = new OcrSettingsDto();
    
    public class OcrSettingsDto
    {
        public string[] HeaderTexts { get; set; } = ["CHASSE AUX TRÉSOR"];
        public string[] FooterTexts { get; set; } = ["essais restants", "essai restant", "essais restant"];
        public string[] CurrentTexts { get; set; } = ["encours", "en cours"];
        public string[] StartTexts { get; set; } = ["départ"];
        public string[] CurrentPositionTexts { get; set; } = ["- Niveau"];
    }
    
    
    public class HuntBoxApproximationDto
    {
        [JsonPropertyName("width")]
        public int Width { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}