using dofus_chasse_helper.ConsoleCommand;
using dofus_chasse_helper.ConsoleCommand.Infrastructure;
using GlobalHotKeys;
using GlobalHotKeys.Native.Types;
using Spectre.Console;

namespace dofus_chasse_helper;

public class HotkeyHandler : IDisposable
{
    private HotKeyManager? HotKeyManager;
    
    public void SetupHotkeys(ConsoleCommandDispatcher consoleCommandDispatcher)
    {
        void HotkeyReaction(HotKey hotKey)
        {
            AnsiConsole.MarkupLine($"HotKey Pressed: {hotKey.Modifiers} + {hotKey.Key}");
            if (hotKey.Key == VirtualKeyCode.KEY_Q)
            {
                AnsiConsole.MarkupLine($"Running Next command");
                _ = consoleCommandDispatcher.Dispatch<NextPositionCommand>([]).Result;
            }
            if (hotKey.Key == VirtualKeyCode.KEY_A)
            {
                AnsiConsole.MarkupLine($"Running Start command");
                _ = consoleCommandDispatcher.Dispatch<StartHuntCommand>([]).Result;
            }
        }

        this.HotKeyManager = new HotKeyManager();
        this.HotKeyManager.HotKeyPressed.Subscribe(HotkeyReaction);
        this.HotKeyManager.Register(VirtualKeyCode.KEY_Q, Modifiers.Control | Modifiers.Alt);
        this.HotKeyManager.Register(VirtualKeyCode.KEY_A, Modifiers.Control | Modifiers.Alt);
        
        ShowHotkeys();
    }

    private static void ShowHotkeys()
    {
        AnsiConsole.MarkupLine("[underline][maroon]Hotkeys[/][/]");
        AnsiConsole.MarkupLine("[underline]CTRL + ALT + A[/]: Run the [green]Start[/] command");
        AnsiConsole.MarkupLine("[underline]CTRL + ALT + Q[/]: Run the [green]Next[/] command");
    }

    public void Dispose()
    {
        ((IDisposable)HotKeyManager)?.Dispose();
    }
}