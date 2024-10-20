using System.Text.Json.Serialization;

namespace DofusChasseHelper.Infrastructure;

// TODO: Move in Infra and do a VO in the Domain

public class GetDataResponse
{
    [JsonPropertyName("from")]
    public FromResponse From { get; set; }
    [JsonPropertyName("hints")]
    public IReadOnlyCollection<HintResponse> Hints { get; set; }
}

public class FromResponse
{
    [JsonPropertyName("x")]
    public int X { get; set; }
    [JsonPropertyName("y")]
    public int Y { get; set; }
    [JsonPropertyName("di")]
    public string Direction { get; set; }
}

public class HintResponse
{
    [JsonPropertyName("n")]
    public int N { get; set; }
    
    [JsonPropertyName("x")]
    public int X { get; set; }
    
    [JsonPropertyName("y")]
    public int Y { get; set; }
    
    [JsonPropertyName("d")]
    public int D { get; set; }
}