namespace Memodex.Tests.Integration.DataAccess;

public class BrowseDecksReaderTests : TestFixtureBase
{
    [Test]
    public async Task GetDecksAsync_ReturnsAllDecks()
    {
        // Arrange
        await DbFixture.CreateUserDb();
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO decks (name, description, flashcardCount)
            VALUES
                ('Test Deck 1', 'Test Description 1', 1),
                ('Test Deck 2', 'Test Description 2', 2),
                ('Test Deck 3', 'Test Description 3', 3);
            """);
        await command.ExecuteNonQueryAsync();

        // Act
        BrowseDecks.BrowseDecksReader browseDecksReader = new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        IEnumerable<BrowseDecks.DeckItem> decks = await browseDecksReader.GetDecksAsync();
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(decks.Count(), Is.EqualTo(3));
            Assert.That(decks.ElementAt(0).Id, Is.EqualTo(1));
            Assert.That(decks.ElementAt(0).Name, Is.EqualTo("Test Deck 1"));
            Assert.That(decks.ElementAt(0).Description, Is.EqualTo("Test Description 1"));
            Assert.That(decks.ElementAt(1).Id, Is.EqualTo(2));
            Assert.That(decks.ElementAt(1).Name, Is.EqualTo("Test Deck 2"));
            Assert.That(decks.ElementAt(1).Description, Is.EqualTo("Test Description 2"));
            Assert.That(decks.ElementAt(2).Id, Is.EqualTo(3));
            Assert.That(decks.ElementAt(2).Name, Is.EqualTo("Test Deck 3"));
            Assert.That(decks.ElementAt(2).Description, Is.EqualTo("Test Description 3"));
        });
    }
}
