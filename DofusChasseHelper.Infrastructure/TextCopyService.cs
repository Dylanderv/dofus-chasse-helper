using DofusChasseHelper.Domain.External;
using TextCopy;

namespace DofusChasseHelper.Infrastructure;

public class TextCopyService : IClipboardService
{
    public async Task SetInClipboard(string text)
    {
        await ClipboardService.SetTextAsync(text);
    }
}