namespace DofusChasseHelper.Infrastructure.Exceptions;

public class NameNotFoundException : Exception
{
    public NameNotFoundException(string name) : base($"name {name} was not found in the id-name database")
    {
    }
}