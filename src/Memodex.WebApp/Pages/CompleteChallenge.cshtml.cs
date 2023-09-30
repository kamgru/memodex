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

    public ChallengeItem? CompletedChallenge { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int challengeId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User);
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
}