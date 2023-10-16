namespace Memodex.Tests.Integration.DataAccess;

public class AddFlashcardWriterTests : TestFixtureBase
{
    [Test]
    public async Task AddFlashcard_AddsFlashcardToDb()
    {
        // Arrange
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO decks (name, flashcardCount)
            VALUES ('Test Deck', 10)
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await command.ExecuteScalarAsync());
        await connection.CloseAsync();

        AddFlashcard.AddFlashcardWriter addFlashcardWriter =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);

        // Act
        int flashcardId = await addFlashcardWriter.AddFlashcardAsync(deckId, "Test Question", "Test Answer");

        // Assert
        await connection.OpenAsync();

        await using SqliteCommand assertCommand = connection.CreateCommand(
            """
            SELECT deckId, question, answer
            FROM flashcards
            WHERE id = @id
            LIMIT 1;
            """);
        assertCommand.Parameters.AddWithValue("@id", flashcardId);

        await using SqliteDataReader reader = await assertCommand.ExecuteReaderAsync();
        await reader.ReadAsync();

        Assert.Multiple(() =>
        {
            Assert.That(reader.GetInt32(0), Is.EqualTo(flashcardId));
            Assert.That(reader.GetString(1), Is.EqualTo("Test Question"));
            Assert.That(reader.GetString(2), Is.EqualTo("Test Answer"));
        });
    }

    [Test]
    public async Task AddFlashcard_IncrementsDeckItemCount()
    {
        // Arrange
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO decks (name, flashcardCount)
            VALUES ('Test Deck', 10)
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await command.ExecuteScalarAsync());
        await connection.CloseAsync();

        AddFlashcard.AddFlashcardWriter addFlashcardWriter =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);

        // Act
        await addFlashcardWriter.AddFlashcardAsync(deckId, "Test Question", "Test Answer");

        await connection.OpenAsync();
        await using SqliteCommand assertCommand = connection.CreateCommand(
            """
            SELECT flashcardCount
            FROM decks
            WHERE id = @id;
            """);
        assertCommand.Parameters.AddWithValue("@id", deckId);

        int flashcardCount = Convert.ToInt32(await assertCommand.ExecuteScalarAsync());

        // Assert
        Assert.That(flashcardCount, Is.EqualTo(11));
    }

    [Test]
    public async Task AddFlashcardAsync_IncrementsOrdinalNumber()
    {
        // Arrange
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO decks (name, flashcardCount)
            VALUES ('Test Deck', 10)
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await command.ExecuteScalarAsync());
        await connection.CloseAsync();

        // Act
        AddFlashcard.AddFlashcardWriter addFlashcardWriter =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);

        for (int i = 0; i < 10; i++)
        {
            await addFlashcardWriter.AddFlashcardAsync(deckId, "Test Question", "Test Answer");
        }

        // Assert
        await connection.OpenAsync();
        await using SqliteCommand assertCommand = connection.CreateCommand(
            """
            SELECT ordinalNumber
            FROM flashcards
            WHERE deckId = @deckId;
            """);
        assertCommand.Parameters.AddWithValue("@deckId", deckId);
        await using SqliteDataReader reader = await assertCommand.ExecuteReaderAsync();

        for (int i = 0; i < 10; i++)
        {
            await reader.ReadAsync();
            Assert.That(reader.GetInt32(0), Is.EqualTo(i + 1));
        }
    }
}
