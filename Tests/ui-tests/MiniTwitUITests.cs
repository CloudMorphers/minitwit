using System.Threading.Tasks;
using Microsoft.Playwright;
using NUnit.Framework;

namespace MiniTwit.UI.Tests
{
    public class UITests
    {
        private IPlaywright _playwright;
        private IBrowser _browser;
        private string _testUsername;
        private string _testPassword = "testpass";

        [SetUp]
        public async Task Setup()
        {
            _playwright = await Playwright.CreateAsync();
            _browser = await _playwright.Chromium.LaunchAsync(new BrowserTypeLaunchOptions
            {
                Headless = true
            });

            _testUsername = $"testuser_{Guid.NewGuid()}";
            await RegisterTestUserAsync();
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

            await page.GotoAsync("http://localhost:5037"); 
            var content = await page.ContentAsync();

            Assert.That(content, Does.Contain("MiniTwit"));  
        }

        private async Task<IPage> LoginAsTestUserAsync()
        {
            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync("http://localhost:5037/login"); // <-- changed port here
            await page.WaitForTimeoutAsync(1000);

            await page.FillAsync("#Username", _testUsername);
            await page.FillAsync("#Password", _testPassword);

            await page.ClickAsync("input[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            return page;
        }

        private async Task RegisterTestUserAsync()
        {
            var context = await _browser.NewContextAsync();
            var page = await context.NewPageAsync();

            await page.GotoAsync("http://localhost:5037/register");
            await page.FillAsync("#Username", _testUsername);
            await page.FillAsync("#Email", $"{_testUsername}@example.com");
            await page.FillAsync("#Password", _testPassword);
            await page.FillAsync("#PasswordConfirmation", _testPassword);
            await page.ClickAsync("input[type='submit']");
            await page.WaitForLoadStateAsync(LoadState.NetworkIdle);

            // Check registration succeeded
            var content = await page.ContentAsync();
            Assert.That(content, Does.Not.Contain("error"), "Registration failed!");

            // Only log out if registration succeeded and you are logged in
            if (content.Contains("logout"))
            {
                await page.GotoAsync("http://localhost:5037/logout");
            }
        }

        [Test]
        public async Task UserCanLogin()
        {
            var page = await LoginAsTestUserAsync();
            var content = await page.ContentAsync();
            Assert.That(content, Does.Contain("logged in"));
        }

        [Test]
        public async Task UserCanPost()
        {
            var page = await LoginAsTestUserAsync();
            await page.WaitForTimeoutAsync(1000);
            String timeOfDay = Convert.ToString(DateTime.Now.TimeOfDay);
            await page.FillAsync("#Text", "Test post. " + timeOfDay);
            await page.WaitForTimeoutAsync(1000);

            await page.ClickAsync("input[type='submit']");
            await page.WaitForTimeoutAsync(1000);
            var content = await page.ContentAsync();
            Assert.That(content, Does.Contain("Test post. " + timeOfDay));
        }

        

    }
}