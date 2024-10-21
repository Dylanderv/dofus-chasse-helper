using OpenCvSharp;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand.Abstractions;

public interface IConsoleCommand
{
    public string Command { get; }
    public string Args  => "";
    public Task<int> Run(params string[] args);
    public IReadOnlyCollection<Markup> Usage();
}