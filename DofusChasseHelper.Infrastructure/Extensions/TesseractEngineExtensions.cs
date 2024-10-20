using System.Drawing;
using Tesseract;

namespace DofusChasseHelper.Infrastructure.Extensions;

public static class TesseractEngineExtensions
{
    public static Page? Process(this TesseractEngine engine, Bitmap image, Rectangle rectangle, PageSegMode pageSegMode, int offset = 0)
        => engine.Process(image,
            new Rect(rectangle.X - offset, rectangle.Y - offset, rectangle.Width + offset, rectangle.Height + offset),
            pageSegMode);
}