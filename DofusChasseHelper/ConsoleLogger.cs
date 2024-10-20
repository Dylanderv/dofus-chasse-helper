using DofusChasseHelper.Domain;
using Spectre.Console;

namespace dofus_chasse_helper;

public class ConsoleLogger : IConsoleLogger
{
    public void LogInfo(string info)
    {
        AnsiConsole.WriteLine(info);
    }
}