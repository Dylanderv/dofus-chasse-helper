namespace DofusChasseHelper.Domain;

public record DofusTextId(int Value)
{
    public static DofusTextId FromString(string value) => new DofusTextId(int.Parse(value));
}
