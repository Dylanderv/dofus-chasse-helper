using System.Text.Json;
using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;
using DofusChasseHelper.Infrastructure.Exceptions;

namespace DofusChasseHelper.Infrastructure;

public class DofusTextProvider : IDofusTextProvider
{
    public DofusTextId GetIdFromName(string name)
    {
        FileStream fileStream = File.OpenRead("./text.json");
        var text = JsonSerializer.Deserialize<Dictionary<string, string>>(fileStream);

        var hintId = text?.FirstOrDefault(x => x.Value.Equals(name, StringComparison.OrdinalIgnoreCase)).Key;

        if (hintId is null)
            throw new NameNotFoundException(name);

        return DofusTextId.FromString(hintId);
    }
}

