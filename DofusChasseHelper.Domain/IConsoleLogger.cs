namespace DofusChasseHelper.Domain;

public interface IConsoleLogger
{
    void LogInfo(string info);

    void NotifyNewPosition(Coords coords);
    void NotifyHuntSolverState(HuntSolverState state);
}