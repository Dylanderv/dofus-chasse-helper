using System.Diagnostics.CodeAnalysis;
using System.Drawing;
using System.Text.RegularExpressions;
using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;
using DofusChasseHelper.Infrastructure.Exceptions;
using DofusChasseHelper.Infrastructure.Extensions;
using Flurl.Http.Testing;
using Tesseract;
using ImageFormat = System.Drawing.Imaging.ImageFormat;
using Rect = Tesseract.Rect;

namespace DofusChasseHelper.Infrastructure;

public class OcrEngine : IOcrEngine
{
    private readonly Cv2Engine _cv2Engine;
    // private const string TessdataPath = @"C:\tessdata";
    private const string TessdataPath = @".\tessdata";
    // private const string BasePath = @"C:\temp";
    private const string BasePath = @".\base";
    
    private const bool IsDebug = true;
    private static readonly string[] HeaderDebugDirectoryPathParts = ["debug", "header"];
    private static readonly string[] FooterDebugDirectoryPathParts = ["debug", "footer"];
    private static readonly string[] BoxDetailsDebugDirectoryPathParts = ["debug", "boxDetails"];
    private static readonly string[] CurrentPositionDebugDirectoryPathParts = ["debug", "currentPosition"];

    private static readonly string[] AproxBoxImagePathParts = ["roughbox.png"];
    private static readonly string[] CurrentPosFullImagePathParts = ["currPosFull.png"];
    private static readonly string[] CurrentPosImagePathParts = ["currPos.png"];
    private static readonly string[] BoxImagePathParts = ["found.png"];
    private static readonly string[] CurrentImagePathParts = ["process-action-current.png"];
    
    private static readonly TextTemplate HeaderTextTemplate = new("header" ,["CHASSE AUX TRÉSOR"]);
    private static readonly TextTemplate FooterTextTemplate = new("footer" ,["essais restants", "essai restant"]);
    private static readonly TextTemplate CurrentTextTemplate = new("current" ,["encours", "en cours"], TemplateSearchOptions.FindLast);
    private static readonly TextTemplate StartTextTemplate = new("start" ,["départ"]);
    private static readonly TextTemplate CurrentPositionTextTemplate = new("currentPosition" ,["- Niveau"]);

    public OcrEngine(Cv2Engine cv2Engine)
    {
        _cv2Engine = cv2Engine;
        Directory.CreateDirectory(BasePath);
        Directory.CreateDirectory(BuildPath(HeaderDebugDirectoryPathParts));
        Directory.CreateDirectory(BuildPath(FooterDebugDirectoryPathParts));
        Directory.CreateDirectory(BuildPath(BoxDetailsDebugDirectoryPathParts));
        Directory.CreateDirectory(BuildPath(CurrentPositionDebugDirectoryPathParts));
    }

    private TesseractEngine GetEngine()
    {
        var tesseractEngine = new TesseractEngine(TessdataPath, "fra");
        tesseractEngine.SetVariable("TESSDATA_PREFIX", TessdataPath);
        return tesseractEngine;
    }
    
    private static List<Rectangle> GetSegmentedImage(
        TesseractEngine engine, 
        Bitmap image,
        PageSegMode pageSegMode,
        PageIteratorLevel pageIteratorLevel
    )
    {
        using var process = engine.Process(image, pageSegMode);
        return process.GetSegmentedRegions(pageIteratorLevel);
    }

    public async Task<Coords> GetCurrentPos(Bitmap sourceImage)
    {
        var sourceImagePath = BuildPath(CurrentPosFullImagePathParts);
        sourceImage.Save(sourceImagePath);
        Rectangle rectangle;
        try
        {
            ChestMatch chestMatch = await this._cv2Engine.GetChestMatch(sourceImagePath);
            rectangle = new Rectangle(chestMatch.Point with { X = Math.Max(chestMatch.Point.X - 250, 0) }, new Size(chestMatch.Point.X, 25));
        }
        catch (Exception)
        {
            using var engine = this.GetEngine();

            const PageSegMode pageSegMode = PageSegMode.SingleColumn;
            const PageIteratorLevel pageIteratorLevel = PageIteratorLevel.TextLine;
            
            Match? currentPositionMatch = FindMatchWithMatchingText(
                engine,
                sourceImage,
                CurrentPositionTextTemplate,
                CurrentPositionDebugDirectoryPathParts,
                pageSegMode, 
                pageIteratorLevel
            );

            if (currentPositionMatch is null)
            {
                throw new Exception("Unable to find current position");
            }

            rectangle = currentPositionMatch.Rectangle;
        }
        

        var subImage = SaveSubImage(sourceImage, rectangle, CurrentPosImagePathParts);

        var tesseractEngine = this.GetEngine();
        var process = tesseractEngine.Process(subImage);

        var text = process.GetText();

        var regex = new Regex(@"(-?\d*), ?(-?\d*)");

        var groups = regex.Match(text).Groups;

        if (groups.Count != 3)
        {
            throw new Exception("Unable to parse current position");
        }
        // groups[0] is the whole match (i.e. for "-1, 3 - Niveau 10", groups[0] will be "-1, 3", groups[1] "-1", groups[2] "3")
        var coords = new Coords(int.Parse(groups[1].Value), int.Parse(groups[2].Value));

        //
        // IReadOnlyCollection<string> enumerable = text.Split(' ').Take(2).ToList();
        //
        // if (enumerable.First().Contains(','))
        // {
        //     enumerable = enumerable.First().Split(',');
        //     if (enumerable.Count > 2)
        //     {
        //         throw new Exception("Unable to parse current position");
        //     }
        // }
        // var coords = new Coords(int.Parse(enumerable.First().Trim().TrimEnd(',')), int.Parse(enumerable.Last().Trim().TrimEnd('-').Trim()));

        
        //
        //
        // var skipLast = text.Split("Niveau").First().Split('-').SkipLast(1);
        // var join = string.Join('-', skipLast);
        // var strings = join.Split(',');


        return coords;
    }
    
    public Task<(Coords startPosition, Hint firstHint)> GetFirstHint(Bitmap sourceImage)
    {
        using Bitmap huntBox = GetHuntBoxImage(sourceImage);
        (Match start, Match current) = FindStartCoordsAndNextHint(huntBox);

        Coords startPosition = GetStartPosition(start);
        ArrowResult direction = GetDirection(huntBox, current);
        var searchObject = DetermineTextName(current);

        return Task.FromResult((
            startPosition,
            new Hint(searchObject, direction.Arrow)
        ));
    }
    
    public Task<Hint> GetNextHint(Bitmap sourceImage)
    {
        using Bitmap huntBox = GetHuntBoxImage(sourceImage);
        (Match _, Match current) = FindStartCoordsAndNextHint(huntBox);

        ArrowResult direction = GetDirection(huntBox, current);
        var searchObject = DetermineTextName(current);

        return Task.FromResult(new Hint(searchObject, direction.Arrow));
    }

    private static string DetermineTextName(Match current)
    {
        var endIndex = -1;
        for (var i = 0; i < CurrentTextTemplate.OrTemplates.Length && endIndex == -1; i++)
            endIndex = current.Text.IndexOf(CurrentTextTemplate.OrTemplates[i], StringComparison.OrdinalIgnoreCase);

        var firstLetter = current.Text.IndexOf(current.Text.FirstOrDefault(char.IsLetter));
        
        var searchObject = current.Text.Substring(firstLetter, endIndex - firstLetter).Trim();

        var reversed = new string(searchObject.Reverse().ToArray());
        var lastLetter = reversed.IndexOf(reversed.FirstOrDefault(char.IsLetter));
        
        searchObject = searchObject.Substring(0, searchObject.Length - lastLetter).Trim();

        if (char.IsUpper(searchObject.First()) is false)
        {
            for (var index = 0; index < searchObject.Length; index++)
            {
                var c = searchObject[index];
                if (char.IsUpper(c))
                {
                    searchObject = searchObject[(index)..].Trim();
                    break;
                }
            }
        }
        
        if (searchObject.All(x => char.IsLetter(x) || char.IsWhiteSpace(x)))
        {
            return searchObject.Trim();
        }

        for (var index = 0; index < searchObject.Length; index++)
        {
            var c = searchObject[index];
            switch (char.IsLetter(c))
            {
                case false when index <= 3:
                    return searchObject[(index)..].Trim();
                case false:
                    return searchObject[..(index)].Trim();
            }
        }

        throw new InvalidOperationException("Could not determine text name");
    }

    private ArrowResult GetDirection(Bitmap huntBox, Match current)
    {
        this.SaveSubImage(huntBox, current.Rectangle, CurrentImagePathParts);

        ArrowResult? match = this._cv2Engine.MatchArrow(this.BuildPath(CurrentImagePathParts));

        if (match is null)
            throw new Cv2CouldNotFindRequiredElement("arrow");

        return match;
    }

    private static Coords GetStartPosition(Match start)
    {
        var startPosition = start.Text.Split('[').Last().Split(']').First().Split(',');
        return new Coords(int.Parse(startPosition[0]), int.Parse(startPosition[1]));
    }

    private (Match start, Match current) FindStartCoordsAndNextHint(Bitmap huntBox)
    {
        const PageSegMode pageSegMode = PageSegMode.SingleColumn;
        const PageIteratorLevel pageIteratorLevel = PageIteratorLevel.TextLine;

        using var engine = this.GetEngine();
        
        Dictionary<string, Match>? matches = FindMatchWithMatchingText(
            engine,
            huntBox,
            [ CurrentTextTemplate, StartTextTemplate ],
            BoxDetailsDebugDirectoryPathParts,
            exitCondition: (_, _) => false,
            onMatchFound: (template, result, newMatch) =>
            {
                if (template.Name == StartTextTemplate.Name 
                    || result.TryGetValue(CurrentTextTemplate.Name, out var existingMatch) is false)
                {
                    result.Add(template.Name, newMatch);
                    return;
                }

                if (existingMatch.Rectangle.Y < newMatch.Rectangle.Y)
                {
                    result[CurrentTextTemplate.Name] = newMatch;
                }
            },
            pageSegMode, 
            pageIteratorLevel
        );

        Match? current = matches.GetValueOrDefault(CurrentTextTemplate.Name);
        Match? start = matches.GetValueOrDefault(StartTextTemplate.Name);

        if (start is null)
            throw new OcrCouldNotFindRequiredElement(nameof(start));

        if (current is null)
            throw new OcrCouldNotFindRequiredElement(nameof(current));
        
        return (start, current);
    }

    private Bitmap GetHuntBoxImage(Bitmap screenShot)
    {
        const PageSegMode pageSegMode = PageSegMode.SparseText;
        const PageIteratorLevel pageIteratorLevel = PageIteratorLevel.Block;

        using var engine = GetEngine();

        Match? header = FindMatchWithMatchingText(
            engine, 
            screenShot,
            HeaderTextTemplate,
            HeaderDebugDirectoryPathParts,
            pageSegMode, 
            pageIteratorLevel
        );
        
        if (header is null)
            throw new OcrCouldNotFindRequiredElement(nameof(header));
        
        Rectangle roughBoxSize = ApproximateHuntBoxFromHeader(screenShot, header.Rectangle);
        using var roughBoxImage = SaveSubImage(screenShot, roughBoxSize, AproxBoxImagePathParts);

        Match? footer = FindMatchWithMatchingText(
            engine, 
            roughBoxImage,
            FooterTextTemplate,
            FooterDebugDirectoryPathParts,
            pageSegMode, 
            pageIteratorLevel,
            offset: 5
        );

        roughBoxImage.Dispose();

        if (footer is null)
            throw new OcrCouldNotFindRequiredElement(nameof(footer));

        var huntRect = CalculateHuntBoxRectangleFromHeaderAndFooter(screenShot, header, footer);

        return SaveSubImage(screenShot, huntRect, BoxImagePathParts);
    }

    private static Rectangle CalculateHuntBoxRectangleFromHeaderAndFooter(Bitmap screenShot,
         Match header, Match footer)
    {
        var left = Math.Max(header.Rectangle.X - 100, 0);
        var top = Math.Max(header.Rectangle.Y - 10, 0);

        var width = Math.Max(footer.Rectangle.Width, 153);
        var right = Math.Min(header.Rectangle.X + footer.Rectangle.X + width + 60, screenShot.Width);
        var bottom = Math.Min(header.Rectangle.Y + footer.Rectangle.Y + footer.Rectangle.Height + 20, screenShot.Height);

        var chasseRect = Rectangle.FromLTRB(left, top, right, bottom);
        return chasseRect;
    }

    private Match? FindMatchWithMatchingText(
        TesseractEngine engine,
        Bitmap sourceImage,
        TextTemplate textToSearch,
        string[] debugDirectoryPathParts,
        PageSegMode pageSegMode,
        PageIteratorLevel pageIteratorLevel,
        int offset = 0)
    {
        var matches = FindMatchWithMatchingText(engine, sourceImage, [textToSearch], debugDirectoryPathParts, pageSegMode, pageIteratorLevel, offset);
        return matches.GetValueOrDefault(textToSearch.Name);
    }
    
    private Dictionary<string, Match> FindMatchWithMatchingText(
        TesseractEngine engine, 
        Bitmap sourceImage, 
        TextTemplate[] textToSearch,
        string[] debugDirectoryPathParts,
        PageSegMode pageSegMode,
        PageIteratorLevel pageIteratorLevel,
        int offset = 0
    ) 
        => this.FindMatchWithMatchingText(
            engine,
            sourceImage,
            textToSearch,
            debugDirectoryPathParts,
            ((templates, result) => templates.All(x => result.ContainsKey(x.Name))),
            ((template, result, newMatch) => result.Add(template.Name, newMatch)),
            pageSegMode,
            pageIteratorLevel,
            offset);


    private Dictionary<string, Match> FindMatchWithMatchingText(
        TesseractEngine engine, 
        Bitmap sourceImage, 
        TextTemplate[] textToSearch,
        string[] debugDirectoryPathParts,
        Func<TextTemplate[], Dictionary<string, Match>, bool> exitCondition,
        Action<TextTemplate, Dictionary<string, Match>, Match> onMatchFound,
        PageSegMode pageSegMode,
        PageIteratorLevel pageIteratorLevel,
        int offset = 0)
    {
        var result = new Dictionary<string, Match>();

        var boxSegmentedImage = GetSegmentedImage(engine, sourceImage, pageSegMode, pageIteratorLevel);

        CleanupPreviousSavedImageForDebug(debugDirectoryPathParts);

        foreach (var segmentedRegion in boxSegmentedImage)
        {
            using var regionProcess = engine.Process(sourceImage, segmentedRegion, pageSegMode, offset);

            var text = regionProcess.GetText();

            LogAndSaveImageForDebug(sourceImage, segmentedRegion, text, debugDirectoryPathParts);

            foreach (var textTemplate in textToSearch)
            {
                if (!CompareTextWithPossibleMatches(text, textTemplate.OrTemplates))
                    continue;
                
                onMatchFound.Invoke(textTemplate, result, new Match(text, segmentedRegion));
            }

            if (exitCondition.Invoke(textToSearch, result))
            {
                return result;
            }
        }

        return result;
    }

    private Bitmap SaveSubImage(Bitmap screenShot, Rectangle roughBoxSize, string[] pathParts)
    {
        Bitmap? roughBoxImage = null;
        try
        {
            roughBoxImage = screenShot.Clone(roughBoxSize, screenShot.PixelFormat);
            roughBoxImage.Save(this.BuildPath(pathParts));
            return roughBoxImage;
        }
        catch
        {
            roughBoxImage?.Dispose();
            throw;
        }
    }

    private static Rectangle ApproximateHuntBoxFromHeader(Bitmap screenShot, [DisallowNull] Rectangle? header)
    {
        var valueX = Math.Max(header.Value.X - 150, 0);
        var valueY = Math.Max(header.Value.Y - 10, 0);
        var valueWidth = Math.Min(header.Value.Width + 300, screenShot.Width);
        var valueHeight = Math.Min(header.Value.Height + 350, screenShot.Height);


        var roughBoxSize = new Rectangle(valueX, valueY, valueWidth, valueHeight);
        return roughBoxSize;
    }

    private void CleanupPreviousSavedImageForDebug(string[] directoryPathParts, bool overrideDebug = false)
    {
        if (!IsDebug && !overrideDebug) return;

        var directoryInfo = new DirectoryInfo(this.BuildPath(directoryPathParts));
        foreach (var file in directoryInfo.GetFiles()) file.Delete();
        foreach (var subDirectory in directoryInfo.GetDirectories()) subDirectory.Delete(true);
    }

    private void LogAndSaveImageForDebug(Bitmap roughBoxImage, Rectangle segmentedRegion, string text, string[] directoryPathParts,
        bool overrideDebug = false)
    {
        if (!IsDebug && !overrideDebug) return;

        var tempImg = roughBoxImage.Clone(segmentedRegion, roughBoxImage.PixelFormat);
        tempImg.Save(
            this.BuildPath(
            [
                ..directoryPathParts,
                $"{segmentedRegion.X};{segmentedRegion.Y}-{segmentedRegion.Width}x{segmentedRegion.Height}.png"
            ]),
            ImageFormat.Png);
        Console.WriteLine(segmentedRegion);
        Console.WriteLine(
            $"{segmentedRegion.X};{segmentedRegion.Y}-{segmentedRegion.Width}x{segmentedRegion.Height} - {text}");
    }

    private void LogInfo(string log) => Console.WriteLine(log);

    private static bool CompareTextWithPossibleMatches(string source, string[] matches)
    {
        return matches.Any(x => source.Contains(x, StringComparison.OrdinalIgnoreCase));
    }

    private string BuildPath(string[] pathParts)
    {
        return pathParts.Aggregate(BasePath, (aggregator, current) => $"{aggregator}\\{current}");
    }
}

public record TextTemplate(string Name, string[] OrTemplates, TemplateSearchOptions SearchOption = TemplateSearchOptions.None);

public enum TemplateSearchOptions
{
    None = 1,
    FindLast = 2
}
