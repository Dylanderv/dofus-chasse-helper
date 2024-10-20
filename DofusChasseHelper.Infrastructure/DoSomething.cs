using System.Drawing;
using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;
using OpenCvSharp;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Point = System.Drawing.Point;
using Rect = Tesseract.Rect;

namespace DofusChasseHelper.Infrastructure;

public class DoSomething : IDoSomething
{
    private const bool IsDebug = false;
    private static readonly string[] CurrentTextTemplate = ["encours", "en cours"];

    public async Task<(Coords coords, string nextHint, Arrow direction)> DoTheThings()
    {
        var box = new Bitmap(@"C:\temp\found.png");
        return await Action(box);
    }
    
    public static Bitmap? GetBox(Bitmap sourceImage)
    {
        var pageSegMode = PageSegMode.SparseText;
        var pageIteratorLevel = PageIteratorLevel.Block;

        // var imagePath = "C:\\temp\\tempTesseract.png";
        // sourceImage.Save(imagePath, ImageFormat.Png);


        using var tesseractEngine = new TesseractEngine(@"C:\tessdata", "fra");
        tesseractEngine.SetVariable("TESSDATA_PREFIX", @"C:\tessdata");

        var segmentedRegions = GetSegmentedImage(tesseractEngine, sourceImage, pageSegMode, pageIteratorLevel);

        Rectangle? header = null, footer = null;

        CleanupOldDebug("header");

        foreach (var segmentedRegion in segmentedRegions)
        {
            using var regionProcess = tesseractEngine.Process(sourceImage,
                new Rect(segmentedRegion.X, segmentedRegion.Y, segmentedRegion.Width, segmentedRegion.Height),
                pageSegMode);

            var text = regionProcess.GetText();

            Debug(sourceImage, segmentedRegion, text, "header");


            if (text.Contains("CHASSE AUX TRÉSOR", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("header");
                header = segmentedRegion;
                using var headerImg = sourceImage.Clone(header.Value, sourceImage.PixelFormat);
                headerImg.Save("C:\\temp\\header.png");
                break;
            }
        }

        if (header is null)
        {
            Console.WriteLine("Header not found");
            return null;
        }

        var valueX = Math.Max(header.Value.X - 150, 0);
        var valueY = Math.Max(header.Value.Y - 10, 0);
        var valueWidth = Math.Min(header.Value.Width + 300, sourceImage.Width);
        var valueHeight = Math.Min(header.Value.Height + 350, sourceImage.Height);


        var roughBoxSize = new Rectangle(valueX, valueY, valueWidth, valueHeight);
        using var roughBoxImage = sourceImage.Clone(roughBoxSize, sourceImage.PixelFormat);
        roughBoxImage.Save("C:\\temp\\roughbox.png");

        var boxSegmentedImage = GetSegmentedImage(tesseractEngine, roughBoxImage, pageSegMode, pageIteratorLevel);

        CleanupOldDebug("footer");

        foreach (var segmentedRegion in boxSegmentedImage)
        {
            var offset = 5;
            using var regionProcess = tesseractEngine.Process(roughBoxImage,
                new Rect(segmentedRegion.X - offset, segmentedRegion.Y - offset, segmentedRegion.Width + offset,
                    segmentedRegion.Height + offset),
                pageSegMode);

            var text = regionProcess.GetText();

            Debug(roughBoxImage, segmentedRegion, text, "footer");

            if (text.Contains("essais restants", StringComparison.OrdinalIgnoreCase))
            {
                Console.WriteLine("footer");
                footer = segmentedRegion;
                break;
            }
        }

        roughBoxImage.Dispose();

        if (footer is null)
        {
            Console.WriteLine("Footer not found");
            return null;
        }

        var left = Math.Max(header.Value.X - 100, 0);
        var top = Math.Max(header.Value.Y - 10, 0);

        var width = Math.Max(footer.Value.Width, 153);
        var right = Math.Min(header.Value.X + footer.Value.X + width + 60, sourceImage.Width);
        var bottom = Math.Min(header.Value.Y + footer.Value.Y + footer.Value.Height + 20, sourceImage.Height);

        var chasseRect = Rectangle.FromLTRB(left, top, right, bottom);

        Console.WriteLine(chasseRect);

        var bitmap = sourceImage.Clone(chasseRect, sourceImage.PixelFormat);
        bitmap.Save("C:\\temp\\found.png");

        return bitmap;
    }

    private static void CleanupOldDebug(string directory, bool overrideDebug = false)
    {
        if (!IsDebug && !overrideDebug) return;

        var directoryInfo = new DirectoryInfo($"C:\\temp\\{directory}");
        foreach (var file in directoryInfo.GetFiles()) file.Delete();
        foreach (var subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);
    }

    private static void Debug(Bitmap roughBoxImage, Rectangle segmentedRegion, string text, string folder,
        bool overrideDebug = false)
    {
        if (!IsDebug && !overrideDebug) return;

        var tempImg = roughBoxImage.Clone(segmentedRegion, roughBoxImage.PixelFormat);
        tempImg.Save(
            $"C:\\temp\\{folder}\\{segmentedRegion.X};{segmentedRegion.Y}-{segmentedRegion.Width}x{segmentedRegion.Height}.png",
            ImageFormat.Png);
        Console.WriteLine(segmentedRegion);
        Console.WriteLine(
            $"{segmentedRegion.X};{segmentedRegion.Y}-{segmentedRegion.Width}x{segmentedRegion.Height} - {text}");
    }

    private static List<Rectangle> GetSegmentedImage(TesseractEngine tesseractEngine, Bitmap image,
        PageSegMode pageSegMode,
        PageIteratorLevel pageIteratorLevel)
    {
        using var process = tesseractEngine.Process(image, pageSegMode);
        return process.GetSegmentedRegions(pageIteratorLevel);
    }


    public static void Test(Bitmap image)
    {
        var pageSegModes = Enum.GetValues<PageSegMode>();
        var pageIteratorLevels = Enum.GetValues<PageIteratorLevel>();

        foreach (var pageSegMode in pageSegModes)
        {
            foreach (var iteratorLevel in pageIteratorLevels)
            {
                var folderPath = GetFolderPath(pageSegMode, iteratorLevel);

                if (Directory.Exists(folderPath) is false)
                    Directory.CreateDirectory(folderPath);

                var directoryInfo = new DirectoryInfo(folderPath);
                foreach (var file in directoryInfo.GetFiles()) file.Delete();
                foreach (var subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);
            }

            var tesseractEngine = new TesseractEngine(@"./", "fra");

            var process = tesseractEngine.Process(image, pageSegMode);

            foreach (var iteratorLevel in pageIteratorLevels)
            {
                var segmentedRegions = process.GetSegmentedRegions(iteratorLevel);
                foreach (var segmentedRegion in segmentedRegions)
                {
                    var bitmap = image.Clone(segmentedRegion, image.PixelFormat);
                    bitmap.Save(
                        $"{GetFolderPath(pageSegMode, iteratorLevel)}\\{segmentedRegion.Width}x{segmentedRegion.Height}.png",
                        ImageFormat.Png);
                    Console.WriteLine(segmentedRegion);
                }
            }
        }
    }


    private static string GetFolderPath(PageSegMode pageSegMode1, PageIteratorLevel sPageIteratorLevel)
    {
        var pageSageModeName = Enum.GetName(pageSegMode1);
        var pageIteratorLevelName = Enum.GetName(sPageIteratorLevel);


        return $"C:\\temp\\{pageSageModeName}\\{pageIteratorLevelName}";
    }

    public static async Task<(Coords coords, string nextHint, Arrow direction)> Action(Bitmap box)
    {
        var pageSegMode = PageSegMode.SingleColumn;
        var pageIteratorLevel = PageIteratorLevel.TextLine;

        using var tesseractEngine = new TesseractEngine(@"C:\tessdata", "fra");
        tesseractEngine.SetVariable("TESSDATA_PREFIX", @"C:\tessdata");

        var process = tesseractEngine.Process(box);

        var segmentedRegions = process.GetSegmentedRegions(pageIteratorLevel);

        process.Dispose();

        CleanupOldDebug("box");

        Match? start = null, current = null;

        foreach (var segmentedRegion in segmentedRegions)
        {
            using var regionProcess = tesseractEngine.Process(box,
                new Rect(segmentedRegion.X, segmentedRegion.Y, segmentedRegion.Width, segmentedRegion.Height),
                pageSegMode);

            var text = regionProcess.GetText();


            Debug(box, segmentedRegion, text, "box");

            if (text.Contains("départ", StringComparison.OrdinalIgnoreCase)) start = new Match(text, segmentedRegion);

            if (CompareTextWithPossibleMatches(text, CurrentTextTemplate)) current = new Match(text, segmentedRegion);
        }

        if (start is null)
        {
            Console.WriteLine("Start not found");
            throw new InvalidOperationException();
        }

        if (current is null)
        {
            Console.WriteLine("Current not found");
            throw new InvalidOperationException();
        }

        return await ProcessAction(box, start, current);
    }

    private static async Task<(Coords coords, string nextHint, Arrow direction)> ProcessAction(Bitmap sourceImage, Match start, Match current)
    {
        var startPosition = start.Text.Split('[').Last().Split(']').First().Split(',');


        var currentImage = sourceImage.Clone(current.Rectangle, sourceImage.PixelFormat);
        currentImage.Save(@"C:\temp\process-action-current.png");

        var match = MatchArrow();

        if (match is null)
        {
            Console.WriteLine("Unable to find arrow match, aborting...");
            throw new InvalidOperationException();
        }

        var endIndex = -1;
        for (var i = 0; i < CurrentTextTemplate.Length && endIndex == -1; i++)
            endIndex = current.Text.IndexOf(CurrentTextTemplate[i], StringComparison.OrdinalIgnoreCase);

        var firstLetter = current.Text.IndexOf(current.Text.FirstOrDefault(char.IsLetter));

        var searchObject = current.Text.Substring(firstLetter, endIndex - firstLetter).Trim();

        Console.WriteLine($"Start: {startPosition[0]},{startPosition[1]}");
        Console.WriteLine($"Direction: {Enum.GetName(match.Arrow) ?? "Unknown"}");
        Console.WriteLine($"SearchedObject: {searchObject}");

        return (
            new Coords(int.Parse(startPosition[0]), int.Parse(startPosition[1])),
            searchObject,
            match.Arrow
        );
    }

    private static ArrowResult? MatchArrow()
    {
        var templatesName = new[]
        {
            (Arrow.Down, "arrow-down"),
            (Arrow.Up, "arrow-up"),
            (Arrow.Left, "arrow-left"),
            (Arrow.Right, "arrow-right")
        };

        using var img1 = new Mat(@"C:\temp\process-action-current.png");

        var templateMatches = new List<ArrowResult>();

        foreach ((var arrow, var name) in templatesName)
        {
            using var template = new Mat(@$"C:\temp\templates\{name}.png");
            using var match = new Mat();
            Cv2.MatchTemplate(img1, template, match, TemplateMatchModes.CCoeffNormed);
            match.MinMaxLoc(out _, out var maxVal, out _, out var maxLoc);
            templateMatches.Add(new ArrowResult(arrow, maxVal, new Point(maxLoc.X, maxLoc.Y)));
        }

        var mostProbableMatch = templateMatches.MaxBy(x => x.Score);

        return mostProbableMatch;
    }

    private static bool CompareTextWithPossibleMatches(string source, params string[] matches)
    {
        return matches.Any(x => source.Contains(x, StringComparison.OrdinalIgnoreCase));
    }
}

public record ArrowResult(Arrow Arrow, double Score, Point Point);

public record Match
{
    public Match(string Text, Rectangle Rectangle)
    {
        this.Text = Text.Trim();
        this.Rectangle = Rectangle;
    }

    public string Text { get; init; }
    public Rectangle Rectangle { get; init; }
}