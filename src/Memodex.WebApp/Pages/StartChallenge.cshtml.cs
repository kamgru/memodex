using System.Data.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class StartChallenge : PageModel
{
    public IEnumerable<CategoryItem> Categories { get; set; } = new List<CategoryItem>();
    public IEnumerable<DeckItem> Decks { get; set; } = new List<DeckItem>();
    public int? SelectedCategoryId { get; set; }

    public async Task<IActionResult> OnPostAsync(
        int deckId)
    {
        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = "memodex_test.sqlite",
            ForeignKeys = true
        };
        await using SqliteConnection connection = new(connectionStringBuilder.ConnectionString);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        
        //check if deck exists
        const string sqlDeckExists =
            """
            SELECT EXISTS(SELECT 1 FROM decks WHERE id = @deckId);
            """;
        SqliteCommand deckExistsCommand = connection.CreateCommand();
        deckExistsCommand.CommandText = sqlDeckExists;
        deckExistsCommand.Parameters.AddWithValue("@deckId", deckId);
        bool deckExists = Convert.ToBoolean(await deckExistsCommand.ExecuteScalarAsync());
        if (!deckExists)
        {
            return NotFound();
        }

        //get flashcards
        const string sqlFlashcards = "SELECT id FROM flashcards WHERE deckId = @deckId;";
        SqliteCommand getFlashcardsCommand = connection.CreateCommand();
        getFlashcardsCommand.CommandText = sqlFlashcards;
        getFlashcardsCommand.Parameters.AddWithValue("@deckId", deckId);
        await using DbDataReader reader = await getFlashcardsCommand.ExecuteReaderAsync();
        List<int> flashcardIds = new();
        while (await reader.ReadAsync())
        {
            flashcardIds.Add(reader.GetInt32(0));
        }

        //create challenge
        const string sqlChallenge =
            """
            INSERT INTO main.challenges (`deckId`, `stepCount`) VALUES (@deckId, @stepCount);
            """;
        SqliteCommand createChallengeCommand = connection.CreateCommand();
        createChallengeCommand.CommandText = sqlChallenge;
        createChallengeCommand.Parameters.AddWithValue("@deckId", deckId);
        createChallengeCommand.Parameters.AddWithValue("@stepCount", flashcardIds.Count);
        await createChallengeCommand.ExecuteNonQueryAsync();
        
        //get last inserted challenge id
        const string sqlLastInsertedChallengeId =
            """
            SELECT last_insert_rowid();
            """;
        SqliteCommand getLastInsertedChallengeIdCommand = connection.CreateCommand();
        getLastInsertedChallengeIdCommand.CommandText = sqlLastInsertedChallengeId;
        int challengeId = Convert.ToInt32(await getLastInsertedChallengeIdCommand.ExecuteScalarAsync());
        
        //create challenge steps
        const string sqlChallengeSteps =
            """
            INSERT INTO main.steps (`challengeId`, `flashcardId`, `stepIndex`) 
            VALUES (@challengeId, @flashcardId, @stepIndex);
            """;
        SqliteCommand createChallengeStepsCommand = connection.CreateCommand();
        createChallengeStepsCommand.CommandText = sqlChallengeSteps;
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
}