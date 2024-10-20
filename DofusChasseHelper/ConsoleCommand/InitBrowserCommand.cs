
using dofus_chasse_helper.ConsoleCommand.Abstractions;
using DofusChasseHelper.Infrastructure.Interfaces;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class InitBrowserCommand : IConsoleCommand
{
    private readonly IHeadlessBrowserSetup _headlessBrowserSetup;

    public InitBrowserCommand(IHeadlessBrowserSetup headlessBrowserSetup)
    {
        _headlessBrowserSetup = headlessBrowserSetup;
    }

    public async Task<int> Run(params string[] args)
    {
        await AnsiConsole.Progress()
            .StartAsync(async ctx => 
            {
                var download = ctx.AddTask("[green]Downloading browser[/]", new ProgressTaskSettings()
                {
                    MaxValue = 1
                });
                var startingBrowser = ctx.AddTask("[green]Starting browser[/]", new ProgressTaskSettings()
                {
                    MaxValue = 1
                });
                
                download.StartTask();
                await this._headlessBrowserSetup.DownloadBrowserIfNecessary();
                download.Increment(1);
                
                startingBrowser.StartTask();
                await this._headlessBrowserSetup.StartBrowser();
                startingBrowser.Increment(1);
            });
        
        
        
        return 0;

    }

    public IReadOnlyCollection<Markup> Usage()
    {
        throw new NotImplementedException();
    }
}