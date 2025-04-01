using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace MiniTwit.UI.Tests
{
    public class UITests
    {
        private IPlaywright _playwright;
        private IBrowser _browser;

        [SetUp]
        public async Task Setup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = false
            });
        }

        [TearDown]
        public async Task Teardown()
        {
            await _browser.CloseAsync();
            _playwright.Dispose();
        }

        [Test]
        public async Task HomePage_ShouldLoad()
        {
            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync("http://localhost:8080"); 
            var content = await page.ContentAsync();

            Assert.That(content, Does.Contain("MiniTwit"));  
        }

        [Test]
    public async Task UserCanLogin()
    {
        var context = await _browser.NewContextAsync();
        var page = await context.NewPageAsync();

        // Go to the login page
        await page.GotoAsync("http://localhost:8080/login");

        // Give the page a moment before interacting
        await page.WaitForTimeoutAsync(1000);

        await page.WaitForSelectorAsync("#Username");
        await page.FillAsync("#Username", "testuser");

        await page.WaitForSelectorAsync("#Password");
        await page.FillAsync("#Password", "testpass");
        // Click the login button

        await page.PauseAsync();
        await page.ClickAsync("input[type='submit']");

        await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

        // Pausing for manual inspection
        await page.PauseAsync();

        // Check that login was successful
        var content = await page.ContentAsync();
        Assert.That(content, Does.Contain("You are now logged in"));}
        }
}