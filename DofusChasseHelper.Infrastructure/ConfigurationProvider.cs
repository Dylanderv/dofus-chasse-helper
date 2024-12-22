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

        return new HuntBoxApproximation(config.HuntBoxApproximation.Witdh, config.HuntBoxApproximation.Height);
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
    
    public class HuntBoxApproximationDto
    {
        [JsonPropertyName("witdh")]
        public int Witdh { get; set; }
        [JsonPropertyName("height")]
        public int Height { get; set; }
    }
}