namespace DofusChasseHelper.Domain.External;

public interface IDofusTextProvider
{
    public DofusTextId GetIdFromName(string name);
}