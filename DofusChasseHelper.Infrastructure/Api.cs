using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;
using Flurl;
using Flurl.Http;

namespace DofusChasseHelper.Infrastructure;

public class Api : IApi
{
    public async Task<IReadOnlyCollection<PossibleHint>> GetAllPossibleHintsForDirection(Coords coords, Arrow direction)
    {
        var directionParam = direction switch
        {
            Arrow.Up => "up",
            Arrow.Down => "down",
            Arrow.Left => "left",
            Arrow.Right => "right",
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null)
        };
        
        var url = "https://dofus-map.com/huntTool/getData.php"
            .SetQueryParam("x", coords.X)
            .SetQueryParam("y", coords.Y)
            .SetQueryParam("direction", directionParam)
            .SetQueryParam("world", 0)
            .SetQueryParam("language", "fr");
        
        
        var response = await url
            .WithHeader("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64; rv:131.0) Gecko/20100101 Firefox/131.0")
            .GetAsync();

        var getDataResponse = await response.GetJsonAsync<GetDataResponse>();

        return getDataResponse.Hints
            .Select(x => new PossibleHint(new DofusTextId(x.N), new Coords(x.X, x.Y), x.D))
            .ToList();
    }
}

