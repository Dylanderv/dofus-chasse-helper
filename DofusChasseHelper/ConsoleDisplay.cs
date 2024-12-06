using DofusChasseHelper.Domain;
using Spectre.Console;

namespace dofus_chasse_helper;

public class ConsoleDisplay
{
    private HuntSolverState? State;

    public void TriggerDisplay()
    {
        var markups = this.BuildMarkups();
        foreach (var markup in markups)
        {
            AnsiConsole.Write(markup);
            AnsiConsole.WriteLine();
        }
    }

    public void NotifyState(HuntSolverState state)
    {
        this.State = state;
        this.TriggerDisplay();
    }

    public IReadOnlyCollection<Markup> BuildMarkups()
    {
        if (this.State is not null)
        {
            return
            [
                new Markup($"Position: {this.State.CurrentPosition.X},{this.State.CurrentPosition.Y}"),
                new Markup(this.State.NextHint is null ? string.Empty : $"Next Hint: {this.State.NextHint.SearchedObject}, {Enum.GetName(this.State.NextHint.Direction)}"),
            ];
        }

        return [];
    }
}