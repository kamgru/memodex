using System.Data.Common;
using Memodex.WebApp.Data;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class Review : PageModel
{
    public FlashcardItem? CurrentFlashcard { get; set; }

    [BindProperty]
    public StepInput? Input { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int challengeId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        await using SqliteCommand getChallengeCommand = connection.CreateCommand(
            """
            SELECT currentStepIndex, state
            FROM challenges
            WHERE id = @challengeId
            LIMIT 1;
            """);

        getChallengeCommand.Parameters.AddWithValue("@challengeId", challengeId);
        await using SqliteDataReader dataReader = await getChallengeCommand.ExecuteReaderAsync();
        if (!await dataReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find challenge");
        }

        int? currentStepIndex = dataReader.GetInt32(0);
        ChallengeState state = (ChallengeState)dataReader.GetInt32(1);

        if (currentStepIndex is null)
        {
            throw new InvalidOperationException("Challenge has no current step");
        }

        if (state != ChallengeState.InReview)
        {
            throw new InvalidOperationException("Challenge is not in review");
        }

        await using SqliteCommand getCurrentStepCommand = connection.CreateCommand(
            """
            SELECT flashcardId, needsReview
            FROM steps
            WHERE challengeId = @challengeId
            AND stepIndex = @currentStepIndex
            LIMIT 1;
            """);

        getCurrentStepCommand.Parameters.AddWithValue("@challengeId", challengeId);
        getCurrentStepCommand.Parameters.AddWithValue("@currentStepIndex", currentStepIndex);
        await using SqliteDataReader currentStepDataReader = await getCurrentStepCommand.ExecuteReaderAsync();
        if (!await currentStepDataReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find current step");
        }

        int flashcardId = currentStepDataReader.GetInt32(0);
        bool needsReview = currentStepDataReader.GetBoolean(1);

        if (!needsReview)
        {
            throw new InvalidOperationException("Current step does not need review");
        }

        await using SqliteCommand getFlashcardCommand = connection.CreateCommand(
            """
            SELECT question, answer, name, flashcardCount
            FROM flashcards
            INNER JOIN decks ON deckId = decks.id
            WHERE flashcards.id = @flashcardId
            LIMIT 1;
            """);

        getFlashcardCommand.Parameters.AddWithValue("@flashcardId", flashcardId);
        await using SqliteDataReader flashcardDataReader = await getFlashcardCommand.ExecuteReaderAsync();
        if (!await flashcardDataReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find flashcard for challenge");
        }

        string question = flashcardDataReader.GetString(0);
        string answer = flashcardDataReader.GetString(1);
        string deckName = flashcardDataReader.GetString(2);
        int deckItemCount = flashcardDataReader.GetInt32(3);

        await transaction.CommitAsync();

        CurrentFlashcard = new FlashcardItem(
            flashcardId,
            question,
            answer,
            deckName,
            deckItemCount,
            currentStepIndex.Value + 1);

        Input = new StepInput(
            challengeId,
            CurrentFlashcard.Id);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input is null)
        {
            throw new InvalidOperationException("Input is null");
        }

        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User, true);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        await using SqliteCommand getChallengeCommand = connection.CreateCommand(
            """
            SELECT currentStepIndex, state
            FROM challenges
            WHERE id = @challengeId
            LIMIT 1;
            """);

        getChallengeCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
        await using SqliteDataReader dataReader = await getChallengeCommand.ExecuteReaderAsync();

        if (!await dataReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find challenge");
        }

        int? currentStepIndex = dataReader.GetInt32(0);
        ChallengeState state = (ChallengeState)dataReader.GetInt32(1);

        if (currentStepIndex is null)
        {
            throw new InvalidOperationException("Challenge has no current step");
        }

        if (state != ChallengeState.InReview)
        {
            throw new InvalidOperationException("Challenge is not in review");
        }

        await using SqliteCommand getNextStepCommand = connection.CreateCommand(
            """
            SELECT stepIndex
            FROM steps
            WHERE challengeId = @challengeId
            AND needsReview = 1
            AND stepIndex > @currentStepIndex
            ORDER BY stepIndex
            LIMIT 1;
            """);
        getNextStepCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
        getNextStepCommand.Parameters.AddWithValue("@currentStepIndex", currentStepIndex);
        await using SqliteDataReader nextStepDataReader = await getNextStepCommand.ExecuteReaderAsync();

        if (!await nextStepDataReader.ReadAsync())
        {
            state = ChallengeState.Complete;
            await using SqliteCommand completeChallengeCommand = connection.CreateCommand(
                """
                UPDATE challenges
                SET state = @state
                WHERE id = @challengeId;
                """);
            completeChallengeCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
            completeChallengeCommand.Parameters.AddWithValue("@state", (int)state);
            await completeChallengeCommand.ExecuteNonQueryAsync();
        }
        else
        {
            int nextStepIndex = nextStepDataReader.GetInt32(0);
            await using SqliteCommand updateNextStepCommand = connection.CreateCommand(
                """
                UPDATE challenges
                SET currentStepIndex = @nextStepIndex
                WHERE id = @challengeId;
                """);
            updateNextStepCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
            updateNextStepCommand.Parameters.AddWithValue("@nextStepIndex", nextStepIndex);
            await updateNextStepCommand.ExecuteNonQueryAsync();
        }

        //update challenge updatedAt column
        await using SqliteCommand updateChallengeCommand = connection.CreateCommand(
            """
            UPDATE challenges
            SET updatedAt = @updatedAt
            WHERE id = @challengeId;
            """);
        updateChallengeCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
        updateChallengeCommand.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        await updateChallengeCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();

        return state == ChallengeState.Complete
            ? RedirectToPage("CompleteChallenge", new { challengeId = Input.ChallengeId })
            : RedirectToPage("Review", new { challengeId = Input.ChallengeId });
    }

    public record FlashcardItem(
        int Id,
        string Question,
        string Answer,
        string DeckTitle,
        int DeckItemCount,
        int CurrentStep);

    public record StepInput(
        int ChallengeId,
        int FlashcardId);
}
