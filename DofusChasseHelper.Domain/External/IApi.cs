namespace DofusChasseHelper.Domain.External;

public interface IApi
{
    public Task<IReadOnlyCollection<PossibleHint>> GetAllPossibleHintsForDirection(Coords coords, Arrow direction);

}