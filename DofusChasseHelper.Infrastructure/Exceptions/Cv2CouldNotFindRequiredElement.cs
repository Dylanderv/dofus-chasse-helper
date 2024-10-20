namespace DofusChasseHelper.Infrastructure.Exceptions;

public class Cv2CouldNotFindRequiredElement : Exception
{
    public Cv2CouldNotFindRequiredElement(string requiredElementName) : base($"Could not find required element: {requiredElementName}")
    {
    }
}