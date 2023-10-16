namespace Memodex.Tests.Integration.DataAccess;

[TestFixture]
public class ExportDeckReaderTests : TestFixtureBase
{
    [Test]
    public async Task GetDeckAsync_WhenDeckDoesntExist_ReturnsNull()
    {
        // Arrange
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand getMaxDeckIdCmd = connection.CreateCommand(
            """
            SELECT MAX(id) FROM decks;
            """);
        object? maxDeckIdObj = await getMaxDeckIdCmd.ExecuteScalarAsync();
        int maxDeckId = maxDeckIdObj is DBNull ? 1 : Convert.ToInt32(maxDeckIdObj);
        int deckId = maxDeckId + 1;
        await connection.CloseAsync();

        ExportDeck.ExportDeckReader exportDeckReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);

        // Act
        ExportDeck.DeckItem? deckItem = await exportDeckReader.GetDeckAsync(deckId);

        // Assert
        Assert.That(deckItem, Is.Null);
    }

    [Test]
    public async Task GetDeckAsync_WhenDeckExists_ReturnsDeck()
    {
        // Arrange
        string deckName = RandomString.Generate();
        string deckDescription = RandomString.Generate();

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand insertDeckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES (@deckName, @deckDescription)
            RETURNING id;
            """);
        insertDeckCmd.Parameters.AddWithValue("@deckName", deckName);
        insertDeckCmd.Parameters.AddWithValue("@deckDescription", deckDescription);
        int deckId = Convert.ToInt32(await insertDeckCmd.ExecuteScalarAsync());
        await connection.CloseAsync();

        ExportDeck.ExportDeckReader exportDeckReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);

        // Act
        ExportDeck.DeckItem? deckItem = await exportDeckReader.GetDeckAsync(deckId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deckItem, Is.Not.Null);
            Assert.That(deckItem!.Name, Is.EqualTo(deckName));
            Assert.That(deckItem.Description, Is.EqualTo(deckDescription));
        });
    }

    [Test]
    public async Task GetDeckAsync_WhenDeckDescriptionNull_ReturnsDeckWithEmptyDescription()
    {
        // Arrange
        string deckName = RandomString.Generate();

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand insertDeckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name)
            VALUES (@deckName)
            RETURNING id;
            """);
        insertDeckCmd.Parameters.AddWithValue("@deckName", deckName);
        int deckId = Convert.ToInt32(await insertDeckCmd.ExecuteScalarAsync());

        ExportDeck.ExportDeckReader exportDeckReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);

        // Act
        ExportDeck.DeckItem? deckItem = await exportDeckReader.GetDeckAsync(deckId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deckItem, Is.Not.Null);
            Assert.That(deckItem!.Name, Is.EqualTo(deckName));
            Assert.That(deckItem.Description, Is.Empty);
        });
    }

    [Test]
    public async Task GetDeckAsync_WhenDeckHasNoFlashcards_ReturnsEmptyDeck()
    {
        // Arrange
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand insertDeckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name)
            VALUES ('Test Deck')
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await insertDeckCmd.ExecuteScalarAsync());
        await connection.CloseAsync();

        ExportDeck.ExportDeckReader exportDeckReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);

        // Act 
        ExportDeck.DeckItem? deckItem = await exportDeckReader.GetDeckAsync(deckId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deckItem, Is.Not.Null);
            Assert.That(deckItem!.Flashcards, Is.Empty);
        });
    }

    [Test]
    public async Task GetDeckAsync_ReturnsDeckWithAllFlashcards()
    {
        // Arrange
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand insertDeckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name)
            VALUES ('Test Deck')
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

        ExportDeck.ExportDeckReader exportDeckReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);

        // Act
        ExportDeck.DeckItem? deckItem = await exportDeckReader.GetDeckAsync(deckId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deckItem, Is.Not.Null);
            Assert.That(deckItem!.Flashcards, Has.Count.EqualTo(3));
            Assert.That(deckItem.Flashcards[0].Question, Is.EqualTo("Question 1"));
            Assert.That(deckItem.Flashcards[0].Answer, Is.EqualTo("Answer 1"));
            Assert.That(deckItem.Flashcards[1].Question, Is.EqualTo("Question 2"));
            Assert.That(deckItem.Flashcards[1].Answer, Is.EqualTo("Answer 2"));
            Assert.That(deckItem.Flashcards[2].Question, Is.EqualTo("Question 3"));
            Assert.That(deckItem.Flashcards[2].Answer, Is.EqualTo("Answer 3"));
        });
    }
}
