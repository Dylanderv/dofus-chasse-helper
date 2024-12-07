namespace DofusChasseHelper.Domain.External;

public interface IHeadlessBrowserHuntSolver
{
    public Task<Coords> SolveWithDofusPourLesNoobs(Coords position, Arrow direction, string searchedObject);
    public Task UpdatePosForDofusPourLesNoobs(Coords position);
}