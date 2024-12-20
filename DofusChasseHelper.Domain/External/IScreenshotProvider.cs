using System.Drawing;

namespace DofusChasseHelper.Domain.External;

public interface IScreenshotProvider
{
    public Bitmap ScreenShot();

    public Bitmap PrintWindow(string procName);

}