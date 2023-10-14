namespace Memodex.Tests.Integration.DataAccess;

public class DeleteDeckWriterTests : TestFixtureBase
{
    [Test]
    public async Task DeleteDeckAsync_WhenDeckExists_DeletesDeck()
    {
        // Arrange
        await DbFixture.CreateUserDb();

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES ('Test Deck', 'Test Description')
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await command.ExecuteScalarAsync());
        await connection.CloseAsync();

        // Act
        EditDeck.DeleteDeckWriter deleteDeckWriter =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        await deleteDeckWriter.DeleteDeckAsync(deckId);

        // Assert
        await connection.OpenAsync();
        await using SqliteCommand selectCommand = connection.CreateCommand(
            """
            SELECT COUNT(*)
            FROM decks
            WHERE id = @id;
            """);
        selectCommand.Parameters.AddWithValue("@id", deckId);
        int deckCount = Convert.ToInt32(await selectCommand.ExecuteScalarAsync());
        await connection.CloseAsync();

        Assert.That(deckCount, Is.EqualTo(0));
    }

    [Test]
    public async Task DeleteDeckAsync_WhenDeckDeleted_DeletesFlashcards()
    {
        // Arrange
        await DbFixture.CreateUserDb();

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES ('Test Deck', 'Test Description')
            RETURNING id;
            
            WITH inserted AS (
                SELECT last_insert_rowid() AS id)
                INSERT INTO flashcards (question, answer, deckId) 
                VALUES 
                    ('Test Question 1', 'Test Answer 1', (SELECT id FROM inserted LIMIT 1)),
                    ('Test Question 2', 'Test Answer 2', (SELECT id FROM inserted LIMIT 1)),
                    ('Test Question 3', 'Test Answer 3', (SELECT id FROM inserted LIMIT 1));
            """);
        int deckId = Convert.ToInt32(await command.ExecuteScalarAsync());
        await connection.CloseAsync();

        // Act
        EditDeck.DeleteDeckWriter deleteDeckWriter =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        await deleteDeckWriter.DeleteDeckAsync(deckId);

        // Assert
        await connection.OpenAsync();
        await using SqliteCommand selectCommand = connection.CreateCommand(
            """
            SELECT COUNT(*)
            FROM flashcards
            WHERE deckId = @deckId;
            """);
        selectCommand.Parameters.AddWithValue("@deckId", deckId);
        int flashcardCount = Convert.ToInt32(await selectCommand.ExecuteScalarAsync());
        await connection.CloseAsync();

        Assert.That(flashcardCount, Is.EqualTo(0));
    }
}
