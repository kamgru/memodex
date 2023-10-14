namespace Memodex.Tests.Integration.DataAccess;

public class DeleteFlashcardWriterTests : TestFixtureBase
{
    [Test]
    public async Task DeleteFlashcardAsync_DeletesFlashcard()
    {
        // Arrange
        await DbFixture.CreateUserDb();

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand deckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES ('Test Deck', 'Test Description')
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await deckCmd.ExecuteScalarAsync());

        await using SqliteCommand flashcardCmd = connection.CreateCommand(
            """
            INSERT INTO flashcards (question, answer, deckId)
            VALUES
                ('Question 1', 'Answer 1', @deckId),
                ('Question 2', 'Answer 2', @deckId),
                ('Question 3', 'Answer 3', @deckId)
            RETURNING id;
            """);
        flashcardCmd.Parameters.AddWithValue("@deckId", deckId);
        await using SqliteDataReader reader = await flashcardCmd.ExecuteReaderAsync();
        List<int> flashcardIds = new();
        while (await reader.ReadAsync())
        {
            flashcardIds.Add(reader.GetInt32(0));
        }

        await connection.CloseAsync();

        // Act
        EditFlashcards.DeleteFlashcardWriter writer = new(DbFixture.SqliteConnectionFactory,
            DbFixture.ClaimsPrincipal);
        await writer.DeleteFlashcardAsync(flashcardIds[1]);

        // Assert
        await connection.OpenAsync();
        await using SqliteCommand countCmd = connection.CreateCommand(
            "SELECT COUNT(*) FROM flashcards WHERE deckId = @deckId;");
        countCmd.Parameters.AddWithValue("@deckId", deckId);
        int count = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
        Assert.That(count, Is.EqualTo(2));
    }

    [Test]
    public async Task DeleteFlashcardAsync_WhenFlashcardDeleted_DeletesAssociatedChallenge()
    {
        // Arrange
        await DbFixture.CreateUserDb();

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand deckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES ('Test Deck', 'Test Description')
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await deckCmd.ExecuteScalarAsync());

        await using SqliteCommand flashcardCmd = connection.CreateCommand(
            """
            INSERT INTO flashcards (question, answer, deckId)
            VALUES
                ('Question 1', 'Answer 1', @deckId),
                ('Question 2', 'Answer 2', @deckId),
                ('Question 3', 'Answer 3', @deckId)
            RETURNING id;
            """);
        flashcardCmd.Parameters.AddWithValue("@deckId", deckId);
        await using SqliteDataReader reader = await flashcardCmd.ExecuteReaderAsync();
        List<int> flashcardIds = new();
        while (await reader.ReadAsync())
        {
            flashcardIds.Add(reader.GetInt32(0));
        }

        await using SqliteCommand challengeCmd = connection.CreateCommand(
            """
            INSERT INTO challenges (deckId)
            VALUES (@deckId)
            RETURNING id;
            """);
        challengeCmd.Parameters.AddWithValue("@deckId", deckId);
        int challengeId = Convert.ToInt32(await challengeCmd.ExecuteScalarAsync());

        // Act
        EditFlashcards.DeleteFlashcardWriter writer = new(DbFixture.SqliteConnectionFactory,
            DbFixture.ClaimsPrincipal);
        await writer.DeleteFlashcardAsync(flashcardIds[1]);

        // Assert
        await connection.OpenAsync();
        await using SqliteCommand countCmd = connection.CreateCommand(
            """
            SELECT COUNT(*)
            FROM challenges 
            WHERE id = @challengeId;
            """);
        countCmd.Parameters.AddWithValue("@challengeId", challengeId);
        int count = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
        Assert.That(count, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteFlashcardAsync_WhenFlashcardDeleted_UpdateDeckFlashcardCount()
    {
        // Arrange
        await DbFixture.CreateUserDb();

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand deckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description, flashcardCount)
            VALUES ('Test Deck', 'Test Description', 3)
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await deckCmd.ExecuteScalarAsync());

        await using SqliteCommand flashcardCmd = connection.CreateCommand(
            """
            INSERT INTO flashcards (question, answer, deckId)
            VALUES
                ('Question 1', 'Answer 1', @deckId),
                ('Question 2', 'Answer 2', @deckId),
                ('Question 3', 'Answer 3', @deckId)
            RETURNING id;
            """);
        flashcardCmd.Parameters.AddWithValue("@deckId", deckId);
        await using SqliteDataReader reader = await flashcardCmd.ExecuteReaderAsync();
        List<int> flashcardIds = new();
        while (await reader.ReadAsync())
        {
            flashcardIds.Add(reader.GetInt32(0));
        }

        await connection.CloseAsync();

        // Act
        EditFlashcards.DeleteFlashcardWriter writer = new(DbFixture.SqliteConnectionFactory,
            DbFixture.ClaimsPrincipal);
        await writer.DeleteFlashcardAsync(flashcardIds[1]);

        // Assert
        await connection.OpenAsync();
        await using SqliteCommand countCmd = connection.CreateCommand(
            """
            SELECT flashcardCount FROM decks WHERE id = @deckId;
            """);
        countCmd.Parameters.AddWithValue("@deckId", deckId);
        int count = Convert.ToInt32(await countCmd.ExecuteScalarAsync());
        Assert.That(count, Is.EqualTo(2));
    }
}
