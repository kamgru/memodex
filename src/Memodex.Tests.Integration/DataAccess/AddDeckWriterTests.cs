namespace Memodex.Tests.Integration.DataAccess;

[TestFixture]
public class AddDeckWriterTests : TestFixtureBase
{
    [Test]
    public async Task AddDeck_AddsDeckToDb()
    {
        await DbFixture.CreateUserDb();

        AddDeck.AddDeckWriter addDeckWriter = new(DbFixture.SqliteConnectionFactory);
        int deckId = await addDeckWriter.AddDeckAsync("Test Deck", DbFixture.ClaimsPrincipal);

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT name
            FROM decks
            WHERE id = @id
            """);
        command.Parameters.AddWithValue("@id", deckId);

        string? name = Convert.ToString(await command.ExecuteScalarAsync());
        Assert.That(name, Is.EqualTo("Test Deck"));
    }
}
