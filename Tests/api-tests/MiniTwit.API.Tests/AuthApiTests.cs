using System.Net;
using System.Text;
using AngleSharp.Html.Parser;
using NUnit.Framework;
using Microsoft.AspNetCore.Mvc.Testing;

namespace MiniTwit.API.Tests;

// This class holds integration tests for authentication-related features in MiniTwit.
[TestFixture]
public class AuthApiTests
{
    private HttpClient _client;
    private WebApplicationFactory<Program> _factory;

    // 🔧 Set up a fresh HttpClient for each test
    [SetUp]
    public void SetUp()
    {
        _factory = new WebApplicationFactory<Program>();
        _client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false // So we can assert redirects ourselves
        });
    }

    // 🧹 Clean up after each test
    [TearDown]
    public void TearDown()
    {
        _client.Dispose();
        _factory.Dispose();
    }

    // ✅ Test: Register a new user and check if they are redirected to login
    [Test]
    public async Task Register_NewUser_ReturnsRedirectToLogin()
    {
        var randomUsername = $"apitestuser_{Guid.NewGuid():N}";

        // Get CSRF token from the registration page
        var getResponse = await _client.GetAsync("/register");
        var getHtml = await getResponse.Content.ReadAsStringAsync();
        var token = ExtractRequestVerificationToken(getHtml);

        // Fill in and submit the registration form
        var postData = new Dictionary<string, string>
        {
            { "Username", randomUsername },
            { "Email", $"{randomUsername}@example.com" },
            { "Password", "testpassword" },
            { "PasswordConfirmation", "testpassword" },
            { "__RequestVerificationToken", token }
        };

        var content = new FormUrlEncodedContent(postData);
        var response = await _client.PostAsync("/register", content);

        // 🔁 Expect redirect to login page
        Assert.That(response.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));
        Assert.That(response.Headers.Location?.ToString(), Is.EqualTo("/login"));
    }

    // ✅ Test: Post a message and verify it appears on the timeline
    [Test]
    public async Task PostMessage_AppearsInTimeline()
    {
        var username = $"testuser_{Guid.NewGuid():N}";
        var password = "testpassword";

        // Register user
        var regToken = ExtractRequestVerificationToken(await GetPageHtml("/register"));
        await _client.PostAsync("/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Username", username },
            { "Email", $"{username}@test.com" },
            { "Password", password },
            { "PasswordConfirmation", password },
            { "__RequestVerificationToken", regToken }
        }));

        // Login user
        var loginToken = ExtractRequestVerificationToken(await GetPageHtml("/login"));
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Username", username },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        });

        var loginResponse = await _client.PostAsync("/login", loginContent);
        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

        // 🧁 Carry cookies across requests (simulate being logged in)
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        _client.DefaultRequestHeaders.Remove("Cookie");
        _client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies));

        // Post a new message
        var messageText = $"Hello from test at {DateTime.UtcNow.Ticks}";
        var timelineHtml = await GetPageHtml("/");
        var postToken = ExtractRequestVerificationToken(timelineHtml);

        var postResponse = await _client.PostAsync("/add_message", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Text", messageText },
            { "__RequestVerificationToken", postToken }
        }));

        Assert.That(postResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

        // 🔍 Verify that the message shows up on the timeline
        var updatedTimelineHtml = await GetPageHtml("/");
        Assert.That(updatedTimelineHtml, Does.Contain(messageText));
    }

    // ✅ Test: User A follows and unfollows user B
    [Test]
    public async Task FollowAndUnfollowUser_WorksCorrectly()
    {
        var userA = $"userA_{Guid.NewGuid():N}";
        var userB = $"userB_{Guid.NewGuid():N}";
        var password = "testpassword";

        // Register user B
        var regTokenB = ExtractRequestVerificationToken(await GetPageHtml("/register"));
        await _client.PostAsync("/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Username", userB },
            { "Email", $"{userB}@test.com" },
            { "Password", password },
            { "PasswordConfirmation", password },
            { "__RequestVerificationToken", regTokenB }
        }));

        // Logout user B
        await _client.GetAsync("/logout");

        // Register user A
        var regTokenA = ExtractRequestVerificationToken(await GetPageHtml("/register"));
        await _client.PostAsync("/register", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Username", userA },
            { "Email", $"{userA}@test.com" },
            { "Password", password },
            { "PasswordConfirmation", password },
            { "__RequestVerificationToken", regTokenA }
        }));

        // Login user A
        var loginToken = ExtractRequestVerificationToken(await GetPageHtml("/login"));
        var loginContent = new FormUrlEncodedContent(new Dictionary<string, string>
        {
            { "Username", userA },
            { "Password", password },
            { "__RequestVerificationToken", loginToken }
        });

        var loginResponse = await _client.PostAsync("/login", loginContent);
        Assert.That(loginResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

        // 🍪 Set cookies to simulate logged-in session
        var cookies = loginResponse.Headers.GetValues("Set-Cookie");
        _client.DefaultRequestHeaders.Remove("Cookie");
        _client.DefaultRequestHeaders.Add("Cookie", string.Join("; ", cookies));

        // Follow user B
        var followResponse = await _client.GetAsync($"/follow/{userB}");
        Assert.That(followResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

        // ✅ Check follow status
        var followedHtml = await GetPageHtml($"/{userB}");
        Assert.That(followedHtml, Does.Contain("You are currently following this user"));

        // Unfollow user B
        var unfollowResponse = await _client.GetAsync($"/unfollow/{userB}");
        Assert.That(unfollowResponse.StatusCode, Is.EqualTo(HttpStatusCode.Redirect));

        // ✅ Check unfollow status
        var unfollowedHtml = await GetPageHtml($"/{userB}");
        Assert.That(unfollowedHtml, Does.Contain("You are not yet following this user"));
    }

    // --------------------------------------------------------
    // 🔧 Helper method to load HTML from a specific page path
    // --------------------------------------------------------
    private async Task<string> GetPageHtml(string path)
    {
        var response = await _client.GetAsync(path);
        response.EnsureSuccessStatusCode(); // Fail fast if page load fails
        return await response.Content.ReadAsStringAsync();
    }

    // --------------------------------------------------------
    // 🔧 Helper method to extract anti-forgery (CSRF) token from HTML
    // --------------------------------------------------------
    private string ExtractRequestVerificationToken(string html)
    {
        var parser = new HtmlParser();
        var document = parser.ParseDocument(html);
        var tokenElement = document.QuerySelector("input[name=__RequestVerificationToken]");
        if (tokenElement == null)
        {
            throw new InvalidOperationException("Anti-forgery token not found.");
        }

        return tokenElement.GetAttribute("value");
    }
}