namespace Memodex.Tests.Integration.DataAccess;

public class UpdateFlashcardWriterTests : TestFixtureBase
{
    [Test]
    public async Task UpdateFlashcardAsync_UpdatesQuestionAndAnswer()
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
        EditFlashcards.UpdateFlashcardWriter writer = new(DbFixture.SqliteConnectionFactory,
            DbFixture.ClaimsPrincipal);
        await writer.UpdateFlashcardAsync(flashcardIds[1], "New Question", "New Answer");

        // Assert
        await connection.OpenAsync();
        await using SqliteCommand flashcardCmd2 = connection.CreateCommand(
            """
            SELECT question, answer
            FROM flashcards
            WHERE id = @id;
            """);
        flashcardCmd2.Parameters.AddWithValue("@id", flashcardIds[1]);
        await using SqliteDataReader assertionReader = await flashcardCmd2.ExecuteReaderAsync();
        await assertionReader.ReadAsync();
        
        Assert.Multiple(() =>
        {
            Assert.That(assertionReader.GetString(0), Is.EqualTo("New Question"));
            Assert.That(assertionReader.GetString(1), Is.EqualTo("New Answer"));
        });
    }

    [Test]
    public async Task UpdateFlashcardAsync_WhenFlashcardUpdated_ReturnsOrdinalNumber()
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
            INSERT INTO flashcards (question, answer, deckId, ordinalNumber)
            VALUES
                ('Question 1', 'Answer 1', @deckId, 1),
                ('Question 2', 'Answer 2', @deckId, 2),
                ('Question 3', 'Answer 3', @deckId, 3)
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
        EditFlashcards.UpdateFlashcardWriter writer = new(DbFixture.SqliteConnectionFactory,
            DbFixture.ClaimsPrincipal);
        int ordinalNumber = await writer.UpdateFlashcardAsync(flashcardIds[1], "New Question", "New Answer");

        // Assert
        Assert.That(ordinalNumber, Is.EqualTo(2));
    }
}
