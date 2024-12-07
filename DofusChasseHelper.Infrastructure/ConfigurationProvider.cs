using System.Text.Json;
using System.Text.Json.Serialization;
using DofusChasseHelper.Domain;

namespace DofusChasseHelper.Infrastructure;

public class ConfigurationProvider : IConfigurationProvider
{
    public string GetCharacterName()
    {
        var rawConfig = File.ReadAllText(@".\configuration.json");
        var config = JsonSerializer.Deserialize<Config>(rawConfig);

        if (config is null)
        {
            throw new Exception("Unable to read configuration");
        }
        
        return config.CharacterName;
    }
}

public class Config
{
    [JsonPropertyName("characterName")]
    public string CharacterName { get; set; }
}