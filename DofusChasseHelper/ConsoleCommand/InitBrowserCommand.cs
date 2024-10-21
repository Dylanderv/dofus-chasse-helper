
using dofus_chasse_helper.ConsoleCommand.Abstractions;
using DofusChasseHelper.Infrastructure.Interfaces;
using Spectre.Console;

namespace dofus_chasse_helper.ConsoleCommand;

public class InitBrowserCommand : IConsoleCommand
{
    private readonly IHeadlessBrowserSetup _headlessBrowserSetup;
    public string Command => "none";

    public InitBrowserCommand(IHeadlessBrowserSetup headlessBrowserSetup)
    {
        _headlessBrowserSetup = headlessBrowserSetup;
    }

    public async Task<int> Run(params string[] args)
    {
        await AnsiConsole.Status()
            .StartAsync("[green]Downloading browser[/]", async ctx =>
            {
                ctx.Spinner(Spinner.Known.GrowHorizontal);
                await this._headlessBrowserSetup.DownloadBrowserIfNecessary();

                ctx.Status("[green]Starting browser[/]");
                await this._headlessBrowserSetup.StartBrowser();
            });
        
        
        
        return 0;

    }

    public IReadOnlyCollection<Markup> Usage()
    {
        return [];
    }
}