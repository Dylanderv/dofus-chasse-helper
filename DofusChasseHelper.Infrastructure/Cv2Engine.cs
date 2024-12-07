using System.Drawing;
using DofusChasseHelper.Domain;
using OpenCvSharp;
using Point = System.Drawing.Point;
using Size = System.Drawing.Size;

namespace DofusChasseHelper.Infrastructure;

public class Cv2Engine
{
    public static readonly (Arrow arrow, string name)[] ArrowTemplates =
    [
        (Arrow.Down, "arrow-down"),
        (Arrow.Up, "arrow-up"),
        (Arrow.Left, "arrow-left"),
        (Arrow.Right, "arrow-right")
    ];

    private string BuildTemplatePath(string templateName) => @$".\templates\{templateName}.png";
    private string Build1080pTemplatePath(string templateName) => @$".\templates\1080p-{templateName}.png";
    
    public Task<ChestMatch> GetChestMatch(string path)
    {
        using var img1 = new Mat(path);

        var templateMatches = new List<ArrowResult>();

        var templatePath = BuildTemplatePath("pos-chest");
        
        using var template = new Mat(templatePath);
        using var match = new Mat();
        Cv2.MatchTemplate(img1, template, match, TemplateMatchModes.CCoeffNormed);
        match.MinMaxLoc(out _, out var maxVal, out _, out var maxLoc);

        if (maxVal < 0.6)
        {
            throw new Exception("Unable to match confidently the chest");
        }
        
        return Task.FromResult(new ChestMatch(new Point(maxLoc.X, maxLoc.Y)));
    }
    
    public ArrowResult? MatchArrow(string path)
    {
        using var img1 = new Mat(path);

        var templateMatches = new List<ArrowResult>();

        foreach ((var arrow, var name) in ArrowTemplates)
        {
            using var match = new Mat();
            try
            {
                using var template = new Mat(BuildTemplatePath(name));
                Cv2.MatchTemplate(img1, template, match, TemplateMatchModes.CCoeffNormed);
            }
            catch (OpenCVException e)
            {
                using var smallTemplate = new Mat(Build1080pTemplatePath(name));
                Cv2.MatchTemplate(img1, smallTemplate, match, TemplateMatchModes.CCoeffNormed);
            }
            match.MinMaxLoc(out _, out var maxVal, out _, out var maxLoc);
            templateMatches.Add(new ArrowResult(arrow, maxVal, new Point(maxLoc.X, maxLoc.Y)));
        }

        var mostProbableMatch = templateMatches.MaxBy(x => x.Score);

        return mostProbableMatch;
    }
}

public record ChestMatch(Point Point);