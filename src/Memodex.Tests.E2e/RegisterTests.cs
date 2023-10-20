namespace Memodex.Tests.E2e;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class RegisterTests : PageTest
{
    [Test]
    public async Task Register_WhenUsernameAndPasswordCorrect_RegistersUser()
    {
        string username = RandomString.Generate();
        string password = Guid.NewGuid()
            .ToString();

        await Page.GotoAsync($"{Config.BaseUrl}/Register");

        await Page.GetByLabel("Username")
            .FillAsync(username);

        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true })
            .FillAsync(password);

        await Page.GetByLabel("Confirm Password")
            .FillAsync(password);

        await Page.GetByRole(AriaRole.Button)
            .ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading))
            .ToHaveTextAsync("Ready to Test Your Knowledge?");

        string databaseName = new UserDatabaseName(username).ToString();
        File.Delete(databaseName);
    }

    [Test]
    public async Task Register_WhenUsernameAlreadyTaken_ShowsError()
    {
        const string username = "register_existing_user";
        const string password = "password";

        DbFixture dbFixture = new(username);
        await dbFixture.EnsureUserExistsAsync(username, password);

        await Page.GotoAsync($"{Config.BaseUrl}/Register");

        await Page.GetByLabel("Username")
            .FillAsync(username);

        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true })
            .FillAsync(password);

        await Page.GetByLabel("Confirm Password")
            .FillAsync(password);

        await Page.GetByRole(AriaRole.Button)
            .ClickAsync();

        await Expect(Page)
            .ToHaveURLAsync($"{Config.BaseUrl}/Register");

        ILocator alertLocator =
            Page.GetByRole(AriaRole.Alert, new PageGetByRoleOptions { Name = "Registration error" });

        await Expect(alertLocator)
            .ToHaveTextAsync($"User {username} already exists.");
    }

    [Test]
    public async Task Register_WhenPasswordsNotMatch_ShowsError()
    {
        const string username = "register_pwds_no_match";
        const string password = "password";
        const string confirmPassword = "noMatch";

        await Page.GotoAsync($"{Config.BaseUrl}/Register");

        await Page.GetByLabel("Username")
            .FillAsync(username);

        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true })
            .FillAsync(password);

        await Page.GetByLabel("Confirm Password")
            .FillAsync(confirmPassword);

        await Page.GetByRole(AriaRole.Button)
            .ClickAsync();

        await Expect(Page)
            .ToHaveURLAsync($"{Config.BaseUrl}/Register");

        ILocator alertLocator = Page.GetByRole(AriaRole.Alert,
            new PageGetByRoleOptions { Name = "Password confirmation error" });

        await Expect(alertLocator)
            .ToHaveTextAsync("'ConfirmPassword' and 'Password' do not match.");
    }
}
