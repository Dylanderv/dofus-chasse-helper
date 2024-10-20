using DofusChasseHelper.Domain;
using DofusChasseHelper.Domain.External;
using DofusChasseHelper.Infrastructure.Exceptions;
using DofusChasseHelper.Infrastructure.Interfaces;
using PuppeteerSharp;
using PuppeteerSharp.Dom;
using PuppeteerSharp.Input;

namespace DofusChasseHelper.Infrastructure;

public class HeadlessBrowserHuntSolver : IHeadlessBrowserHuntSolver, IHeadlessBrowserSetup, IDisposable, IAsyncDisposable
{
    private IBrowser? _browser;
    private IPage? _page;

    private const string CookiesButtonSelector = ".button__acceptAll";
    private const string XInputSelector = "#huntposx";
    private const string YInputSelector = "#huntposy";

    private Func<Arrow, string> _arrowsSelector = (Arrow direction) => direction switch {
        Arrow.Up => "label[for=\"huntupwards\"]",
        Arrow.Down => "label[for=\"huntdownwards\"]",
        Arrow.Left => "label[for=\"huntleft\"]",
        Arrow.Right => "label[for=\"huntright\"]",
        _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null) };

    private const string CluesSelector = "#clue-choice-select";
    private const string ButtonSubmitSelector = "input[class|=\"clue\"]";
    private const string ResultDivSelector = "#hunt-result-coord";
    private const string InputAutoCopySelector = "#huntautocopy";

    
    public async Task DownloadBrowserIfNecessary()
    {
        var browserFetcher = new BrowserFetcher();
        
        await browserFetcher.DownloadAsync();
    }

    public async Task StartBrowser()
    {
        this._browser = await Puppeteer.LaunchAsync(new LaunchOptions()
        {
            Headless = false,
            Args = [
                "--disable-web-security",
                "--disable-features=IsolateOrigins,site-per-process",
                $"--window-size=1000,1000"
            ],
            DefaultViewport = new ViewPortOptions()
            {
                Width = 1000,
                Height = 1000
            },
            
        });
        
        this._page = await this._browser!.NewPageAsync();
        
        const string url = "https://www.dofuspourlesnoobs.com/resolution-de-chasse-aux-tresors.html";

        string[] authorizedRequests =
        [
            url,
            "cdn2.editmysite.com"
        ];
        
        this._page.AddRequestInterceptor(req =>
        {
            if (authorizedRequests.Any(x => x.Contains(req.Url.Split("://").Last().Split('/').First())))
            {
                return req.ContinueAsync();
            }

            return req.AbortAsync();
        });
        
        await this._page.SetRequestInterceptionAsync(true);
        
        await this._page.GoToAsync(url);
        
        await AcceptCookies(CookiesButtonSelector);
    }

    public async Task StopBrowser()
    {
        if (this._page is not null)
        {
            await this._page.CloseAsync();
            this._page.Dispose();
        }
        
        if (this._browser is not null)
        {
            await this._browser.CloseAsync();
            this._browser.Dispose();
        }
    }

    public Task<bool> IsBrowserRunning()
    {
        return Task.FromResult(this._browser is not null && this._browser.IsConnected && this._page is not null);
    }
    
    
    public async Task<Coords> DofusPourLesNoobs(Coords position, Arrow direction, string searchedObject)
    {
        if ((await this.IsBrowserRunning()) is false)
        {
            throw new BrowserIsNotRunningException();
        }
        
        // await this.Page.WaitForNetworkIdleAsync();
        
        var xInput = await this._page!.QuerySelectorAsync(XInputSelector);
        var yInput = await this._page.QuerySelectorAsync(YInputSelector);
        
        await xInput.ClickAsync();
        await xInput.PressAsync(Key.Backspace);
        await xInput.PressAsync(Key.Backspace);
        await xInput.PressAsync(Key.Backspace);
        await xInput.PressAsync(Key.Backspace);
        await xInput.PressAsync(Key.Backspace);
        await xInput.PressAsync(Key.Backspace);
        await xInput.PressAsync(Key.Backspace);
        await xInput.TypeAsync(position.X.ToString());
        
        await yInput.ClickAsync();
        await yInput.PressAsync(Key.Backspace);
        await yInput.PressAsync(Key.Backspace);
        await yInput.PressAsync(Key.Backspace);
        await yInput.PressAsync(Key.Backspace);
        await yInput.PressAsync(Key.Backspace);
        await yInput.PressAsync(Key.Backspace);
        await yInput.PressAsync(Key.Backspace);
        await yInput.TypeAsync(position.Y.ToString());

        var arrow = await this._page.QuerySelectorAsync(_arrowsSelector.Invoke(direction));
        await arrow.ClickAsync();
        
        var selectClue = await this._page.QuerySelectorAsync(CluesSelector);
        var options = await selectClue.QuerySelectorAllAsync("option");

        bool found = false;
        for (var i = 0; i < options.Length && found is false; i++)
        {
            var elementHandle = options[i];
            
            var textContentAsync = await elementHandle.ToDomHandle<HtmlOptionElement>().GetTextContentAsync();
            if (textContentAsync.Contains(searchedObject, StringComparison.OrdinalIgnoreCase))
            {
                var value = await elementHandle.EvaluateFunctionAsync<string>("node => node.value");
                await selectClue.ToDomHandle<HtmlSelectElement>().SetValueAsync(value);
                found = true;
            }
        }

        if (found is false)
        {
            throw new HuntSolverDidNotFoundSearchedObjectException(searchedObject);
        }

        var autoCopyCommand = await this._page.QuerySelectorAsync(InputAutoCopySelector);
        await autoCopyCommand.ClickAsync();

        var buttonSubmit = await this._page.QuerySelectorAsync(ButtonSubmitSelector);
        await buttonSubmit.ClickAsync();

        var result = await this._page.QuerySelectorAsync(ResultDivSelector);
        var contentAsync = await result.ToDomHandle<HtmlDivElement>().GetTextContentAsync();

        await this._page.ScreenshotAsync(@"C:\temp\browser.png", new ScreenshotOptions()
        {
            Type = ScreenshotType.Png
        });

        string[] strings = contentAsync.Split('[').Last().Split(']').First().Split(',');

        object clipboardContent = await this._page.EvaluateExpressionAsync<object>("() => navigator.clipboard.readText()");
        
        return new Coords(int.Parse(strings[0]), int.Parse(strings[1]));
    }

    private async Task AcceptCookies(string cookiesButtonSelector)
    {
        try
        {
            var appConsent = await this._page!.QuerySelectorAsync("#appconsent");
            var iframe = await appConsent.QuerySelectorAsync("iframe");
            var frameContent = await iframe.ContentFrameAsync();
            var buttonSkip = await frameContent.QuerySelectorAsync(cookiesButtonSelector);
            await buttonSkip.ClickAsync();
        }
        catch (Exception)
        {
            // ignored
        }
    }


    [Obsolete("Does not work because of Google Recaptcha protection")]
    public async Task<Coords> DofusDbAborted()
    {
        const string xInputSelector = """
                              div > input[placeholder="X"]
                              """;

        const string yInputSelector = """
                              div > input[placeholder="Y"]
                              """;

        Func<Arrow, string> arrowsSelector = (Arrow direction) => $"""
                                      .treasure-hunt-direction-icon.fa-arrow-{direction switch {
                                          Arrow.Up => "up",
                                          Arrow.Down => "down",
                                          Arrow.Left => "left",
                                          Arrow.Right => "right",
                                          _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, null) }}
                                      """;

        string searchInputSelector = """
                                     input[type="search"]
                                     """;
        
        var browserFetcher = new BrowserFetcher();
        await browserFetcher.DownloadAsync();
        
        
        await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions()
        {
            Headless = false
        });
        
        await using var page = await browser.NewPageAsync();
        
        await page.SetUserAgentAsync(
            "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/114.0.0.0 Safari/537.36"
        );
        
        await page.GoToAsync("https://dofusdb.fr/fr/tools/treasure-hunt");
        
        await page.WaitForNetworkIdleAsync();

        var xInput = await page.WaitForSelectorAsync(xInputSelector);
        var yInput = await page.WaitForSelectorAsync(yInputSelector);

        await xInput.TypeAsync("13");
        await yInput.TypeAsync("18");

        var arrow = await page.WaitForSelectorAsync(arrowsSelector.Invoke(Arrow.Up));
        await arrow.ClickAsync();
        
        var searchInput = await page.WaitForSelectorAsync(searchInputSelector);
        await searchInput.TypeAsync("Tissu à carreaux noué");

        await page.WaitForNetworkIdleAsync();

        await page.ScreenshotAsync(@"C:\temp\browser.png", new ScreenshotOptions()
        {
            Type = ScreenshotType.Png
        });

        throw new NotImplementedException();
    }

    public void Dispose()
    {
        _page?.Dispose();
        _browser?.Dispose();
    }

    public async ValueTask DisposeAsync()
    {
        if (_browser != null) await _browser.DisposeAsync();
        if (_page != null) await _page.DisposeAsync();
    }
}