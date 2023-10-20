using System.Data.Common;
using System.Security.Claims;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using NSubstitute;

namespace Memodex.Tests.E2e;

public record FakeDeck(
    string Name,
    IEnumerable<FakeFlashcard> Flashcards);

public record FakeFlashcard(
    string Question,
    string Answer);

public class DbFixture : IDisposable
{
    private readonly ClaimsPrincipal _claimsPrincipal;
    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

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
        _sqliteConnectionFactory = new SqliteConnectionFactory(mediaPhysicalPath);

        ClaimsIdentity claimsIdentity = new(new[]
        {
            new Claim(ClaimTypes.Name, username ?? Guid.NewGuid()
                .ToString())
        });
        _claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
    }

    public void Dispose()
    {
        if (File.Exists(_sqliteConnectionFactory.GetDatabaseNameForUser(_claimsPrincipal)))
        {
            File.Delete(_sqliteConnectionFactory.GetDatabaseNameForUser(_claimsPrincipal));
        }
    }

    public SqliteConnection CreateConnectionForUser()
    {
        return _sqliteConnectionFactory.CreateForUser(_claimsPrincipal, true, true);
    }

    public async Task CreateUserDb()
    {
        UserDatabase userDatabase = new(_sqliteConnectionFactory, Substitute.For<ILogger<UserDatabase>>());
        await userDatabase.CreateAsync(_claimsPrincipal);
    }

    public async Task EnsureUserExistsAsync(
        string username,
        string password)
    {
        MemodexDatabase memodexDatabase = new(_sqliteConnectionFactory);
        await memodexDatabase.EnsureExistsAsync();
        await memodexDatabase.AddUserAsync(username, password);
    }

    public async Task<FakeDeck> CreateFakeDeck(
        string deckName = "Fake Deck",
        int flashcardCount = 35)
    {
        await using SqliteConnection connection = CreateConnectionForUser();
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        await using SqliteCommand deckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES (@deckName, 'Test Deck Description')
            RETURNING id;
            """);
        deckCmd.Parameters.AddWithValue("@deckName", deckName);
        int deckId = Convert.ToInt32(await deckCmd.ExecuteScalarAsync());

        List<FakeFlashcard> fakeFlashcards = new();
        flashcardCount = Math.Clamp(flashcardCount, 1, 35);
        for (int i = 0; i < flashcardCount; i++)
        {
            string question = $"Question {i + 1}";
            string answer = $"Answer {i + 1}";
            int ordinal = i + 1;
            await using SqliteCommand flashcardCmd = connection.CreateCommand(
                """
                INSERT INTO flashcards (question, answer, ordinalNumber, deckId)
                VALUES
                    (@question, @answer, @ordinal, @deckId)
                """);
            flashcardCmd.Parameters.AddWithValue("@deckId", deckId);
            flashcardCmd.Parameters.AddWithValue("@question", question);
            flashcardCmd.Parameters.AddWithValue("@answer", answer);
            flashcardCmd.Parameters.AddWithValue("@ordinal", ordinal);
            await flashcardCmd.ExecuteNonQueryAsync();

            fakeFlashcards.Add(
                new FakeFlashcard(
                    question,
                    answer));
        }

        await transaction.CommitAsync();
        await connection.CloseAsync();

        return new FakeDeck(deckName, fakeFlashcards);
    }
}
