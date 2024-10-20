namespace DofusChasseHelper.Infrastructure.Exceptions;

public class HuntSolverDidNotFoundSearchedObjectException : Exception
{
    public HuntSolverDidNotFoundSearchedObjectException(string searchedObject) : base($"Unable to find object {searchedObject}")
    {
    }
}