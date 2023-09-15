using System.Data.Common;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class StartChallenge : PageModel
{
    public record CategoryItem(
        int Id,
        string Name,
        string Description,
        string Image);

    public record DeckItem(
        int Id,
        string Name,
        string Description,
        int ItemCount);

    public IEnumerable<CategoryItem> Categories { get; set; } = new List<CategoryItem>();
    public IEnumerable<DeckItem> Decks { get; set; } = new List<DeckItem>();
    public int? SelectedCategoryId { get; set; }

    public async Task<IActionResult> OnPostAsync(
        int deckId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User, true);
        await connection.OpenAsync();

        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        await using SqliteCommand existsCmd = connection.CreateCommand(
            """
            SELECT EXISTS(
                SELECT id
                FROM decks
                WHERE id = @deckId);
            """);
        existsCmd.Parameters.AddWithValue("@deckId", deckId);

        bool deckExists = Convert.ToBoolean(await existsCmd.ExecuteScalarAsync());
        if (!deckExists)
        {
            return NotFound();
        }

        await using SqliteCommand getFlashcardsCmd = connection.CreateCommand(
            """
            SELECT id
            FROM flashcards
            WHERE deckId = @deckId;
            """);
        getFlashcardsCmd.Parameters.AddWithValue("@deckId", deckId);

        await using DbDataReader reader = await getFlashcardsCmd.ExecuteReaderAsync();
        List<int> flashcardIds = new();
        while (await reader.ReadAsync())
        {
            flashcardIds.Add(reader.GetInt32(0));
        }

        await using SqliteCommand createChallengeCommand = connection.CreateCommand(
            """
            INSERT INTO challenges (deckId, stepCount)
            VALUES (@deckId, @stepCount)
            RETURNING id;
            """);
        createChallengeCommand.Parameters.AddWithValue("@deckId", deckId);
        createChallengeCommand.Parameters.AddWithValue("@stepCount", flashcardIds.Count);
        int challengeId = Convert.ToInt32(await createChallengeCommand.ExecuteScalarAsync());

        await using SqliteCommand createChallengeStepsCommand = connection.CreateCommand(
            """
            INSERT INTO steps (challengeId, flashcardId, stepIndex)
            VALUES (@challengeId, @flashcardId, @stepIndex);
            """);
        for (int i = 0; i < flashcardIds.Count; i++)
        {
            createChallengeStepsCommand.Parameters.Clear();
            createChallengeStepsCommand.Parameters.AddWithValue("@challengeId", challengeId);
            createChallengeStepsCommand.Parameters.AddWithValue("@flashcardId", flashcardIds[i]);
            createChallengeStepsCommand.Parameters.AddWithValue("@stepIndex", i);
            await createChallengeStepsCommand.ExecuteNonQueryAsync();
        }

        await transaction.CommitAsync();

        return RedirectToPage("Engage", new { challengeId });
    }
}
