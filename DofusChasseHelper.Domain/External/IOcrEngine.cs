using System.Drawing;

namespace DofusChasseHelper.Domain.External;

public interface IOcrEngine
{
    Task<(Coords startPosition, Hint firstHint)> GetFirstHint(Bitmap sourceImage);
    Task<Hint> GetNextHint(Bitmap sourceImage);
}