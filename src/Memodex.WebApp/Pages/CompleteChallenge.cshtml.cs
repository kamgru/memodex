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

    public class ChallengeReader
    {
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;

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

            ChallengeItem? challengeItem;

            if (!await reader.ReadAsync())
            {
                challengeItem = null;
            }
            else
            {
                challengeItem = new ChallengeItem(
                    challengeId,
                    reader.GetString(1),
                    (ChallengeState)reader.GetInt32(2));
            }

            await connection.CloseAsync();
            return challengeItem;
        }
    }

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
        ChallengeReader challengeReader = new(_sqliteConnectionFactory, User);
        ChallengeItem? challengeItem = await challengeReader.ReadChallengeAsync(challengeId);

        if (challengeItem is null or { State: ChallengeState.InProgress })
        {
            return RedirectToPage("Index");
        }

        CompletedChallenge = challengeItem;

        return Page();
    }
}
