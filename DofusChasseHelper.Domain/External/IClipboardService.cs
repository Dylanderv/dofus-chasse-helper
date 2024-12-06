namespace DofusChasseHelper.Domain.External;

public interface IClipboardService
{
    public Task SetInClipboard(string text);
    public Task<string?> GetFromClipboard();
}