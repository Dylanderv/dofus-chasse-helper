using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand.Abstractions;

public interface IConsoleCommand
{
    public Task<int> Run(params string[] args);
    public IReadOnlyCollection<Markup> Usage();
}