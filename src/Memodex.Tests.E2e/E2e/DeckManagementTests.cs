namespace Memodex.Tests.E2e.E2e;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class DeckManagementTests : AuthenticatedPageTest
{
    [Test]
    public async Task WhenDeckCreated_IsVisibleInDeckList()
    {
        string deckName = RandomString.Generate();

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Add Deck" })
            .ClickAsync();

        await Page.GetByLabel("Name")
            .FillAsync(deckName);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Expect(Page)
            .ToHaveURLAsync(new Regex(Regex.Escape(Config.BaseUrl) + @"\/EditDeck\?deckId=[0-9]*"));

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Back" })
            .ClickAsync();

        ILocator listLocator = Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "deck list" });

        await Expect(listLocator)
            .ToHaveTextAsync(new Regex($".*{deckName}.*"));
    }

    [Test]
    public async Task GivenDeckCreated_WhenUserEditsDeck_NotificationSuccessVisible()
    {
        string deckName = RandomString.Generate();
        string updatedDeckName = RandomString.Generate();
        string deckDescription = RandomString.Generate();

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Add Deck" })
            .ClickAsync();

        await Page.GetByLabel("Name")
            .FillAsync(deckName);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Page.GetByLabel("Name")
            .FillAsync(updatedDeckName);

        await Page.GetByLabel("Description")
            .FillAsync(deckDescription);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Update Deck" })
            .ClickAsync();

        ILocator alertLocator = Page.GetByRole(AriaRole.Alert, new PageGetByRoleOptions { Name = "notification" });

        await Expect(alertLocator)
            .ToHaveTextAsync(new Regex($"Deck {updatedDeckName} updated..*"));
    }

    [Test]
    public async Task GivenDeckCreated_WhenUserEditsDeck_ChangesAreVisibleInDeckList()
    {
        string deckName = RandomString.Generate();
        string updatedDeckName = RandomString.Generate();
        string deckDescription = RandomString.Generate();

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Add Deck" })
            .ClickAsync();

        await Page.GetByLabel("Name")
            .FillAsync(deckName);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Page.GetByLabel("Name")
            .FillAsync(updatedDeckName);

        await Page.GetByLabel("Description")
            .FillAsync(deckDescription);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Update Deck" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Back" })
            .ClickAsync();

        ILocator listLocator = Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "deck list" });

        await Expect(listLocator)
            .ToHaveTextAsync(new Regex($@"\s*{updatedDeckName}\s*{deckDescription}"));
    }

    [Test]
    public async Task GivenDeckCreated_WhenUserAddsFlashcard_ItsVisibleOnFlashcardList()
    {
        string deckName = RandomString.Generate();
        string question = $"{RandomString.Generate(3)} {RandomString.Generate(5)}";
        string answer = $"{RandomString.Generate(8)} {RandomString.Generate(2)}";

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Add Deck" })
            .ClickAsync();

        await Page.GetByLabel("Name")
            .FillAsync(deckName);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Edit Flashcards" })
            .ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading))
            .ToHaveTextAsync("Manage Your Flashcards");

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Add Flashcard" })
            .ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading))
            .ToHaveTextAsync("Add Flashcard");

        await Page.GetByLabel("Question")
            .FillAsync(question);

        await Page.GetByLabel("Answer")
            .FillAsync(answer);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        ILocator flashcard = Page.GetByRole(AriaRole.Listitem);

        await Expect(flashcard)
            .ToHaveTextAsync(new Regex($@"\s*{question}\s*{answer}"));
    }

    [Test]
    public async Task GivenFlashcardAdded_WhenUserMakesEdits_ChangesAreReflectedInFlashcardList()
    {
        string deckName = RandomString.Generate();
        string question = $"{RandomString.Generate(3)} {RandomString.Generate(5)}";
        string answer = $"{RandomString.Generate(8)} {RandomString.Generate(2)}";
        string updatedQuestion = $"{RandomString.Generate(7)} {RandomString.Generate(8)}";
        string updatedAnswer = $"{RandomString.Generate(1)} {RandomString.Generate(9)}";

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Add Deck" })
            .ClickAsync();

        await Page.GetByLabel("Name")
            .FillAsync(deckName);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Edit Flashcards" })
            .ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading))
            .ToHaveTextAsync("Manage Your Flashcards");

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Add Flashcard" })
            .ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading))
            .ToHaveTextAsync("Add Flashcard");

        await Page.GetByLabel("Question")
            .FillAsync(question);

        await Page.GetByLabel("Answer")
            .FillAsync(answer);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Listitem)
            .GetByRole(AriaRole.Button, new LocatorGetByRoleOptions { Name = "edit flashcard" })
            .ClickAsync();

        await Page.GetByLabel("Question")
            .FillAsync(updatedQuestion);

        await Page.GetByLabel("Answer")
            .FillAsync(updatedAnswer);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        ILocator flashcard = Page.GetByRole(AriaRole.Listitem);

        await Expect(flashcard)
            .ToHaveTextAsync(new Regex($@"\s*{updatedQuestion}\s*{updatedAnswer}"));
    }

    [Test]
    public async Task GivenDeckWithFlashcards_WhenUserAddsFlashcard_ItIsVisibleInFlashcardList()
    {
        FakeDeck deck = (await DbFixture.SeedDecks(1)).Single();
        string question = $"{RandomString.Generate(3)} {RandomString.Generate(5)}";
        string answer = $"{RandomString.Generate(8)} {RandomString.Generate(2)}";

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        ILocator deckList = Page.GetByRole(AriaRole.List, new PageGetByRoleOptions { Name = "deck list" });

        await Expect(deckList)
            .ToHaveTextAsync(new Regex($@"\s*{deck.Name}"));

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Edit Flashcards" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "Add Flashcard" })
            .ClickAsync();

        await Expect(Page.GetByRole(AriaRole.Heading))
            .ToHaveTextAsync("Add Flashcard");

        await Page.GetByLabel("Question")
            .FillAsync(question);

        await Page.GetByLabel("Answer")
            .FillAsync(answer);

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "Submit" })
            .ClickAsync();

        await Page.GetByTestId("flashcard list end")
            .ScrollIntoViewIfNeededAsync();

        await Expect(Page.GetByRole(AriaRole.List))
            .ToHaveTextAsync(new Regex($@"\s*{question}\s*{answer}"));
    }
}
