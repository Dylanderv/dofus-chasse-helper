namespace DofusChasseHelper.Infrastructure.Exceptions;

public class OcrCouldNotFindRequiredElement : Exception
{
    public OcrCouldNotFindRequiredElement(string requiredElementName) : base($"Could not find required element: {requiredElementName}")
    {
    }
}