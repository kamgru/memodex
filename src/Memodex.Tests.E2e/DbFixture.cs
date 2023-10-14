using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;
using NUnit.Framework;

namespace Memodex.Tests.E2e;

public record FakeDeck(
    int Id,
    string Name);

public class DbFixture : IDisposable
{
    public DbFixture(
        string? username = null)
    {
        IConfiguration configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new[]
            {
                new KeyValuePair<string, string?>("Media:Path", TestContext.CurrentContext.WorkDirectory)
            })
            .Build();

        MediaPhysicalPath mediaPhysicalPath = new(configuration);
        SqliteConnectionFactory = new SqliteConnectionFactory(mediaPhysicalPath);

        ClaimsIdentity claimsIdentity = new(new[]
        {
            new Claim(ClaimTypes.Name, username ?? Guid.NewGuid()
                .ToString())
        });
        ClaimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    }

    public SqliteConnectionFactory SqliteConnectionFactory { get; }

    public ClaimsPrincipal ClaimsPrincipal { get; }

    public SqliteConnection CreateConnectionForUser() =>
        SqliteConnectionFactory.CreateForUser(ClaimsPrincipal, true, true);

    public async Task CreateUserDb()
    {
        UserDatabase userDatabase = new(SqliteConnectionFactory, Substitute.For<ILogger<UserDatabase>>());
        await userDatabase.CreateAsync(ClaimsPrincipal);
    }

    public async Task EnsureUserExistsAsync(
        string username,
        string password)
    {
        MemodexDatabase memodexDatabase = new(SqliteConnectionFactory);
        await memodexDatabase.EnsureExistsAsync();
        await memodexDatabase.AddUserAsync(username, password);
    }

    public async Task<IEnumerable<FakeDeck>> SeedDecks(int deckCount = 10)
    {
        List<FakeDeck> fakeDecks = new();
        for (int i = 0; i < deckCount; i++)
        {
            string deckName = $"Test Deck {i}";
            fakeDecks.Add(
                new FakeDeck(
                    await SeedFlashcards(deckName),
                    deckName));
        }

        return fakeDecks;
    }

    public async Task<int> SeedFlashcards(string? deckName = null)
    {
        await using SqliteConnection connection = CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand deckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES (@deckName, 'Test Deck Description')
            RETURNING id;
            """);
        deckCmd.Parameters.AddWithValue("@deckName", deckName ?? Guid.NewGuid().ToString());
        int deckId = Convert.ToInt32(await deckCmd.ExecuteScalarAsync());

        await using SqliteCommand flashcardCmd = connection.CreateCommand(
            """
            INSERT INTO flashcards (question, answer, ordinalNumber, deckId)
            VALUES
                ('Question 1', 'Answer 1', 1, @deckId),
                ('Question 2', 'Answer 2', 2, @deckId),
                ('Question 3', 'Answer 3', 3, @deckId),
                ('Question 4', 'Answer 4', 4, @deckId),
                ('Question 5', 'Answer 5', 5, @deckId),
                ('Question 6', 'Answer 6', 6, @deckId),
                ('Question 7', 'Answer 7', 7, @deckId),
                ('Question 8', 'Answer 8', 8, @deckId),
                ('Question 9', 'Answer 9', 9, @deckId),
                ('Question 10', 'Answer 10', 10, @deckId),
                ('Question 11', 'Answer 11', 11, @deckId),
                ('Question 12', 'Answer 12', 12, @deckId),
                ('Question 13', 'Answer 13', 13, @deckId),
                ('Question 14', 'Answer 14', 14, @deckId),
                ('Question 15', 'Answer 15', 15, @deckId),
                ('Question 16', 'Answer 16', 16, @deckId),
                ('Question 17', 'Answer 17', 17, @deckId),
                ('Question 18', 'Answer 18', 18, @deckId),
                ('Question 19', 'Answer 19', 19, @deckId),
                ('Question 20', 'Answer 20', 20, @deckId),
                ('Question 21', 'Answer 21', 21, @deckId),
                ('Question 22', 'Answer 22', 22, @deckId),
                ('Question 23', 'Answer 23', 23, @deckId),
                ('Question 24', 'Answer 24', 24, @deckId),
                ('Question 25', 'Answer 25', 25, @deckId),
                ('Question 26', 'Answer 26', 26, @deckId),
                ('Question 27', 'Answer 27', 27, @deckId),
                ('Question 28', 'Answer 28', 28, @deckId),
                ('Question 29', 'Answer 29', 29, @deckId),
                ('Question 30', 'Answer 30', 30, @deckId),
                ('Question 31', 'Answer 31', 31, @deckId),
                ('Question 32', 'Answer 32', 32, @deckId),
                ('Question 33', 'Answer 33', 33, @deckId),
                ('Question 34', 'Answer 34', 34, @deckId),
                ('Question 35', 'Answer 35', 35, @deckId)
            """);
        flashcardCmd.Parameters.AddWithValue("@deckId", deckId);
        await flashcardCmd.ExecuteNonQueryAsync();
        await connection.CloseAsync();

        return deckId;
    }

    public void Dispose()
    {
        if (File.Exists(SqliteConnectionFactory.GetDatabaseNameForUser(ClaimsPrincipal)))
        {
            File.Delete(SqliteConnectionFactory.GetDatabaseNameForUser(ClaimsPrincipal));
        }
    }
}
