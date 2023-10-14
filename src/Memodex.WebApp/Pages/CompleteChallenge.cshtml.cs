using System.Security.Claims;
using Memodex.WebApp.Data;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class CompleteChallenge : PageModel
{
    public record ChallengeItem(
        int Id,
        string Title,
        ChallengeState State);

    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public CompleteChallenge(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    public ChallengeItem? CompletedChallenge { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int challengeId)
    {
        await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(User);
        await connection.OpenAsync();
        const string sql =
            """
            SELECT challenge.id, deck.name, challenge.state FROM challenges challenge
            JOIN main.decks deck on deck.id = challenge.deckId
            WHERE challenge.id = @id
            LIMIT 1;
            """;
        await using SqliteCommand command = connection.CreateCommand();
        command.CommandText = sql;
        command.Parameters.AddWithValue("@id", challengeId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException($"Challenge {challengeId} not found");
        }

        if (reader.GetInt32(2) == (int)ChallengeState.InProgress)
        {
            throw new InvalidOperationException($"Challenge {challengeId} is in progress");
        }

        CompletedChallenge = new ChallengeItem(
            challengeId,
            reader.GetString(1),
            (ChallengeState)reader.GetInt32(2));

        return Page();
    }

    public class ChallengeReader
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public ChallengeReader(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task<ChallengeItem?> ReadChallengeAsync(
            int challengeId)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal);
            await connection.OpenAsync();

            await using SqliteCommand command = connection.CreateCommand(
                """
                SELECT challenge.id, deck.name, challenge.state FROM challenges challenge
                JOIN main.decks deck on deck.id = challenge.deckId
                WHERE challenge.id = @id
                LIMIT 1;
                """);
            command.Parameters.AddWithValue("@id", challengeId);
            await using SqliteDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new ChallengeItem(
                challengeId,
                reader.GetString(1),
                (ChallengeState)reader.GetInt32(2));
        }
    }
}
