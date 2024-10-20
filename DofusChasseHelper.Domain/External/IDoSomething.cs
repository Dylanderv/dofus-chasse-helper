namespace DofusChasseHelper.Domain.External;

public interface IDoSomething
{
    public Task<(Coords coords, string nextHint, Arrow direction)> DoTheThings();
}