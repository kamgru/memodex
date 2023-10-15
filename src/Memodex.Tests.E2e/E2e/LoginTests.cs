namespace Memodex.Tests.E2e.E2e;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class LoginTests : PageTest
{
    [Test]
    public async Task Login_WhenUserExists_AndCredentialsCorrect_SignsInUser()
    {
        const string username = "login_existing_user";
        const string password = "password";
        
        DbFixture dbFixture = new(username);
        await dbFixture.EnsureUserExistsAsync(username, password);
        await dbFixture.CreateUserDb();
        
        await Page.GotoAsync($"{Config.BaseUrl}/Login");
        
        await Page.GetByLabel("Username")
            .FillAsync(username);

        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true })
            .FillAsync(password);

        await Page.GetByRole(AriaRole.Button)
            .ClickAsync();

        await Expect(Page)
            .ToHaveURLAsync($"{Config.BaseUrl}/");
    }

    [Test]
    public async Task Login_WhenUserDoesntExist_ShowsError()
    {
        string username = RandomString.Generate();
        const string password = "password";

        await Page.GotoAsync($"{Config.BaseUrl}/Login");

        await Page.GetByLabel("Username")
            .FillAsync(username);

        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true })
            .FillAsync(password);

        await Page.GetByRole(AriaRole.Button)
            .ClickAsync();
        
        ILocator alertLocator = Page.GetByRole(AriaRole.Alert,
            new PageGetByRoleOptions { Name = "Login error" });
        
        await Expect(alertLocator)
            .ToHaveTextAsync("Invalid login attempt.");
    }

    [Test]
    public async Task Login_WhenUserExists_AndPasswordInvalid_ShowsError()
    {
        const string username = "login_existing_user";
        const string password = "password";
        const string invalidPassword = "invalidPassword";
        
        DbFixture dbFixture = new(username);
        await dbFixture.EnsureUserExistsAsync(username, password);
        
        await Page.GotoAsync($"{Config.BaseUrl}/Login");
        
        await Page.GetByLabel("Username")
            .FillAsync(username);

        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true })
            .FillAsync(invalidPassword);

        await Page.GetByRole(AriaRole.Button)
            .ClickAsync();
        
        ILocator alertLocator = Page.GetByRole(AriaRole.Alert,
            new PageGetByRoleOptions { Name = "Login error" });
        
        await Expect(alertLocator)
            .ToHaveTextAsync("Invalid login attempt.");
    }
}
