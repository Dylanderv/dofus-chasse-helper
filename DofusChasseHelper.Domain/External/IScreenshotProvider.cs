using System.Drawing;

namespace DofusChasseHelper.Domain.External;

public interface IScreenshotProvider
{
    public Bitmap ScreenShot();
    public Bitmap ScreenShotUsingSizeFromOneCharacter(string characterName);

    public Bitmap PrintWindow(string procName);

}