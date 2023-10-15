namespace Memodex.Tests.Integration.DataAccess;

public class EditDeckWriterTests : TestFixtureBase
{
    [Test]
    public async Task EditDeckAsync_WhenDeckExists_UpdatesDeck()
    {
        // Arrange
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
        EditDeck.EditDeckWriter editDeckWriter = new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        await editDeckWriter.EditDeckAsync(deckId, "New Name", "New Description");

        // Assert
        await connection.OpenAsync();
        await using SqliteCommand selectCommand = connection.CreateCommand(
            """
            SELECT name, description
            FROM decks
            WHERE id = @id;
            """);
        selectCommand.Parameters.AddWithValue("@id", deckId);
        await using SqliteDataReader reader = await selectCommand.ExecuteReaderAsync();
        await reader.ReadAsync();
        
        Assert.Multiple(() =>
        {
            Assert.That(reader.GetString(0), Is.EqualTo("New Name"));
            Assert.That(reader.GetString(1), Is.EqualTo("New Description"));
        });
    }
}
