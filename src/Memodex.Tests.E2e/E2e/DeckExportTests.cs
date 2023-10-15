using System.Text.Json;

namespace Memodex.Tests.E2e.E2e;

public class DeckExportTests : PageTest
{
    private const string Password = "password";
    private string _username = RandomString.Generate();
    private DbFixture _dbFixture = new();

    [SetUp]
    public async Task Setup()
    {
        _username = RandomString.Generate();
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
    public async Task GivenExistingDeck_WhenUserExports_DownloadsJsonFile()
    {
        FakeDeck deck = (await _dbFixture.SeedDecks(1)).Single();

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "edit deck" })
            .ClickAsync();

        Task<IDownload> waitForDownloadTask = Page.WaitForDownloadAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "export deck" })
            .ClickAsync();

        IDownload download = await waitForDownloadTask;

        Assert.That(download.SuggestedFilename, Is.EqualTo($"{deck.Name}.json"));
    }

    private class ExportedDeck
    {
        public string Name { get; set; } = "";
        public string Description { get; set; } = "";
        public List<ExportedFlashcard> Flashcards { get; set; } = new();
    }

    private class ExportedFlashcard
    {
        public string Question { get; init; } = "";
        public string Answer { get; init; } = "";
    }

    [Test]
    public async Task GivenExistingDeck_WhenUserExports_JsonFileHasCorrectContent()
    {
        await _dbFixture.CreateUserDb();
        await using SqliteConnection connection = _dbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand insertDeckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES ('Test Deck', 'Test Description')
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await insertDeckCmd.ExecuteScalarAsync());

        await using SqliteCommand insertFlashcardsCmd = connection.CreateCommand(
            """
            INSERT INTO flashcards (question, answer, deckId)
            VALUES
                ('Question 1', 'Answer 1', @deckId),
                ('Question 2', 'Answer 2', @deckId),
                ('Question 3', 'Answer 3', @deckId)
            RETURNING id;
            """);
        insertFlashcardsCmd.Parameters.AddWithValue("@deckId", deckId);
        await insertFlashcardsCmd.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        await Page.GetByRole(AriaRole.Navigation)
            .GetByRole(AriaRole.Link, new LocatorGetByRoleOptions { Name = "Decks" })
            .ClickAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "edit deck" })
            .ClickAsync();

        Task<IDownload> waitForDownloadTask = Page.WaitForDownloadAsync();

        await Page.GetByRole(AriaRole.Link, new PageGetByRoleOptions { Name = "export deck" })
            .ClickAsync();

        IDownload download = await waitForDownloadTask;

        await download.SaveAsAsync("deck.json");

        string json = await File.ReadAllTextAsync("deck.json");

        ExportedDeck? deck = JsonSerializer.Deserialize<ExportedDeck>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

        Assert.Multiple(() =>
        {
            Assert.That(deck, Is.Not.Null);
            Assert.That(deck!.Name, Is.EqualTo("Test Deck"));
            Assert.That(deck.Description, Is.EqualTo("Test Description"));
            Assert.That(deck.Flashcards, Has.Count.EqualTo(3));
            Assert.That(deck.Flashcards[0].Question, Is.EqualTo("Question 1"));
            Assert.That(deck.Flashcards[0].Answer, Is.EqualTo("Answer 1"));
            Assert.That(deck.Flashcards[1].Question, Is.EqualTo("Question 2"));
            Assert.That(deck.Flashcards[1].Answer, Is.EqualTo("Answer 2"));
            Assert.That(deck.Flashcards[2].Question, Is.EqualTo("Question 3"));
            Assert.That(deck.Flashcards[2].Answer, Is.EqualTo("Answer 3"));
        });
    }
}
