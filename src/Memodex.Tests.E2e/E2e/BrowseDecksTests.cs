namespace Memodex.Tests.E2e.E2e;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class BrowseDecksTests : PageTest
{
    private const string Password = "password";
    private readonly string _username = RandomString.Generate();
    private DbFixture _dbFixture = new();

    
    [SetUp]
    public async Task Setup()
    {
        _dbFixture = new DbFixture(_username);
        await _dbFixture.EnsureUserExistsAsync(_username, Password);
        await _dbFixture.CreateUserDb();
        
        await Page.GotoAsync($"{Config.BaseUrl}/Login");
        
        await Page.GetByLabel("Username")
            .FillAsync(_username);

        await Page.GetByLabel("Password", new PageGetByLabelOptions { Exact = true })
            .FillAsync(Password);

        await Page.GetByRole(AriaRole.Button)
            .ClickAsync();
        
        await Expect(Page)
            .ToHaveURLAsync($"{Config.BaseUrl}/");
    }

    [TearDown]
    public void TearDown()
    {
        _dbFixture.Dispose();
    }

    [Test]
    public async Task BrowseDecks_DisplaysListOfDecks()
    {
        await _dbFixture.SeedDecks();
        
        await Page.GotoAsync(Config.BaseUrl);
        
        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions {Name = "Decks"})
            .ClickAsync();

        await Expect(Page)
            .ToHaveURLAsync($"{Config.BaseUrl}/BrowseDecks");

        ILocator listLocator = Page.GetByRole(AriaRole.List, new PageGetByRoleOptions{Name = "deck list"});

        await Expect(listLocator)
            .ToHaveTextAsync(new Regex("(?:.*Test Deck [0-9].*)"));
    }
}
