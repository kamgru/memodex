namespace Memodex.Tests.E2e;

[TestFixture]
[Parallelizable(ParallelScope.Self)]
public class EngageTests : AuthenticatedPageTest
{
    [Test]
    public async Task WhenEngageStarted_KeyboardInputHandled()
    {
        FakeDeck fakeDeck = await DbFixture.CreateFakeDeck();

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "start challenge" })
            .ClickAsync();

        await Expect(Page.GetByLabel("flashcard question"))
            .ToHaveTextAsync(fakeDeck.Flashcards.First().Question);

        await Expect(Page.GetByLabel("flashcard answer"))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Visible = false });

        await Page.Keyboard.PressAsync("r");

        await Expect(Page.GetByLabel("flashcard answer"))
            .ToBeVisibleAsync();

        await Expect(Page.GetByLabel("mark for review"))
            .ToBeCheckedAsync(new LocatorAssertionsToBeCheckedOptions { Checked = false });

        await Page.Keyboard.PressAsync("m");

        await Expect(Page.GetByLabel("mark for review"))
            .ToBeCheckedAsync();

        await Page.Keyboard.PressAsync("n");

        await Expect(Page.GetByLabel("flashcard question"))
            .ToHaveTextAsync(fakeDeck.Flashcards.ElementAt(1).Question);
    }

    [Test]
    public async Task GivenChallengeInReview_WhenReviewStarted_KeyboardInputHandled()
    {
        FakeDeck fakeDeck = await DbFixture.CreateFakeDeck(flashcardCount: 2);

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Button, new PageGetByRoleOptions { Name = "start challenge" })
            .ClickAsync();

        await Expect(Page.GetByLabel("flashcard question"))
            .ToHaveTextAsync(fakeDeck.Flashcards.First().Question);

        await Page.Keyboard.PressAsync("m");

        await Page.Keyboard.PressAsync("n");

        await Expect(Page.GetByLabel("flashcard question"))
            .ToHaveTextAsync(fakeDeck.Flashcards.ElementAt(1).Question);

        await Page.Keyboard.PressAsync("m");

        await Page.Keyboard.PressAsync("n");

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "review" })
            .ClickAsync();

        await Expect(Page.GetByLabel("flashcard question"))
            .ToHaveTextAsync(fakeDeck.Flashcards.First().Question);

        await Expect(Page.GetByLabel("flashcard answer"))
            .ToBeVisibleAsync(new LocatorAssertionsToBeVisibleOptions { Visible = false });

        await Page.Keyboard.PressAsync("r");

        await Expect(Page.GetByLabel("flashcard answer"))
            .ToBeVisibleAsync();

        await Page.Keyboard.PressAsync("n");

        await Expect(Page.GetByLabel("flashcard question"))
            .ToHaveTextAsync(fakeDeck.Flashcards.ElementAt(1).Question);
    }
}
