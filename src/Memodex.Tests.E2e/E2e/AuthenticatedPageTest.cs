namespace Memodex.Tests.E2e.E2e;

public abstract class AuthenticatedPageTest : PageTest
{
    private const string Password = "password";
    private string _username = RandomString.Generate();
    protected DbFixture DbFixture = new();

    [SetUp]
    public async Task Setup()
    {
        _username = RandomString.Generate();
        DbFixture = new DbFixture(_username);
        await DbFixture.EnsureUserExistsAsync(_username, Password);
        await DbFixture.CreateUserDb();

        await Page.GotoAsync($"{Config.BaseUrl}/Login");

        await Page.GetByLabel("Username")
            .FillAsync(_username);

        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true })
            .FillAsync(Password);

        await Page.GetByRole(AriaRole.Button)
            .ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading))
            .ToHaveTextAsync("Ready to Test Your Knowledge?");
    }

    [TearDown]
    public void TearDown()
    {
        DbFixture.Dispose();
    }
}
