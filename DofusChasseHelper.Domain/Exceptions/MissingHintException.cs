namespace DofusChasseHelper.Domain.Exceptions;

public class MissingHintException : Exception
{
    public MissingHintException(Hint hint, Exception? innerException) : base($"Api did not respond with searched hint: {hint.SearchedObject}", innerException)
    {
    }
}