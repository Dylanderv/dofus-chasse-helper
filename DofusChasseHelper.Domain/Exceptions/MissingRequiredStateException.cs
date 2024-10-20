namespace DofusChasseHelper.Domain.Exceptions;

public class MissingRequiredStateException : Exception
{
    public MissingRequiredStateException(string missingState) : base($"Missing required {missingState}")
    {
    }
}