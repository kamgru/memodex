namespace Memodex.Tests.Integration.DataAccess;

public class EditDeckReaderTests : TestFixtureBase
{
    [Test]
    public async Task GetDeckAsync_WhenDeckNotFound_ReturnsNull()
    {
        // Arrange
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT MAX(id) FROM decks;
            """);
        object? scalar = await command.ExecuteScalarAsync();
        int maxDeckId = scalar is DBNull ? 1 : Convert.ToInt32(scalar);
        await connection.CloseAsync();

        // Act
        EditDeck.EditDeckReader reader = new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        EditDeck.FormInput? deck = await reader.GetDeckAsync(maxDeckId + 1);

        // Assert
        Assert.That(deck, Is.Null);
    }

    [Test]
    public async Task GetDeckAsync_WhenDeckExists_ReturnsCorrectDeck()
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
        EditDeck.EditDeckReader reader = new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        EditDeck.FormInput? deck = await reader.GetDeckAsync(deckId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(deck, Is.Not.Null);
            Assert.That(deck!.Id, Is.EqualTo(deckId));
            Assert.That(deck.Name, Is.EqualTo("Test Deck"));
            Assert.That(deck.Description, Is.EqualTo("Test Description"));
        });
    }
}
