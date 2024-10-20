using dofus_chasse_helper.ConsoleCommand.Abstractions;
using Microsoft.Extensions.DependencyInjection;

namespace dofus_chasse_helper.ConsoleCommand.Infrastructure;

public class ConsoleCommandDispatcher
{
    private readonly IServiceProvider _serviceProvider;

    public ConsoleCommandDispatcher(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
    }

    public async Task<int> Dispatch<TConsoleCommand>(params string[] args) where TConsoleCommand : IConsoleCommand
    {
        var consoleCommand = this._serviceProvider.GetRequiredService<TConsoleCommand>();

        return await consoleCommand.Run(args);
    }
}