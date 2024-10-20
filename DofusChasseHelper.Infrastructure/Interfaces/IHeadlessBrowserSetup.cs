namespace DofusChasseHelper.Infrastructure.Interfaces;

public interface IHeadlessBrowserSetup
{
    public Task DownloadBrowserIfNecessary();

    public Task StartBrowser();

    public Task StopBrowser();

    public Task<bool> IsBrowserRunning();
}