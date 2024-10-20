namespace DofusChasseHelper.Domain.External;

public interface IHeadlessBrowserHuntSolver
{
    public Task<Coords> DofusPourLesNoobs(Coords position, Arrow direction, string searchedObject);
}