namespace DofusChasseHelper.Infrastructure.Exceptions;

public class BrowserIsNotRunningException : Exception
{
    public BrowserIsNotRunningException() : base("Browser is not running")
    {
    }
}