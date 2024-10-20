
using dofus_chasse_helper.ConsoleCommand.Abstractions;
using DofusChasseHelper.Infrastructure.Interfaces;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class ExitCommand : IConsoleCommand
{
    private readonly IHeadlessBrowserSetup _headlessBrowserSetup;

    public ExitCommand(IHeadlessBrowserSetup headlessBrowserSetup)
    {
        _headlessBrowserSetup = headlessBrowserSetup;
    }

    public async Task<int> Run(params string[] args)
    {
        await AnsiConsole.Progress()
            .StartAsync(async ctx => 
            {
                var stop = ctx.AddTask("[green]Stopping browser[/]", new ProgressTaskSettings()
                {
                    MaxValue = 1
                });
                
                stop.StartTask();
                await this._headlessBrowserSetup.StopBrowser();
                stop.Increment(1);
                
            });
        
        
        
        return 0;

    }

    public IReadOnlyCollection<Markup> Usage()
    {
        throw new NotImplementedException();
    }
}