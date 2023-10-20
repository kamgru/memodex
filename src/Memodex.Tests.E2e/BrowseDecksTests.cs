namespace Memodex.Tests.E2e;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class BrowseDecksTests : AuthenticatedPageTest
{
    [Test]
    public async Task BrowseDecks_DisplaysListOfDecks()
    {
        await DbFixture.SeedDecks();

        await Page.GotoAsync(Config.BaseUrl);

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Expect(Page)
            .ToHaveURLAsync(new Regex(".*BrowseDecks"));

        ILocator listLocator = Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "deck list" });

        await Expect(listLocator)
            .ToHaveTextAsync(new Regex("(?:.*Test Deck [0-9].*)"));
    }
}
