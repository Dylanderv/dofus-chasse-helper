using System.Drawing;
using System.Drawing.Imaging;
using DofusChasseHelper.Domain.External;
using ScaleHQ.DotScreen;

namespace DofusChasseHelper.Infrastructure;

public class ScreenshotProvider : IScreenshotProvider
{
    public Bitmap ScreenShot()
    {
        var screen = Screen.PrimaryScreen;

        var deviceName = screen.DeviceName;
        var screenSize = screen.Bounds;
        var workingArea = screen.WorkingArea;

        Console.WriteLine($"{deviceName} | {workingArea.Width}x{workingArea.Height}");

        var target = new Bitmap(screenSize.Width, screenSize.Height);
        using (var g = Graphics.FromImage(target))
        {
            g.CopyFromScreen(0, 0, 0, 0, new Size(screenSize.Width, screenSize.Height));
            target.Save($"C:\\temp\\{deviceName}.png", ImageFormat.Png);
        }

        return target;
    }
}