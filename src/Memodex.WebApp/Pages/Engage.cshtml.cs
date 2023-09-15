using System.Data.Common;
using Memodex.WebApp.Data;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class Engage : PageModel
{
    public record StepInput(
        int ChallengeId,
        int FlashcardId,
        bool NeedsReview);

    public record FlashcardItem(
        int Id,
        string Question,
        string Answer,
        string DeckTitle,
        int DeckItemCount,
        int CurrentStep);

    public record FlashcardStep(
        FlashcardItem Flashcard,
        bool IsLast);

    public FlashcardItem? CurrentFlashcard { get; set; }

    [BindProperty]
    public StepInput? Input { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int challengeId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User);
        await connection.OpenAsync();

        SqliteCommand getChallengeCmd = connection.CreateCommand(
            """
            SELECT challenges.id, state, currentStepIndex, stepCount, name
            FROM challenges
            JOIN decks ON decks.id = deckId
            WHERE challenges.id = @id;
            """);
        getChallengeCmd.Parameters.AddWithValue("@id", challengeId);

        await using SqliteDataReader getChallengeReader = await getChallengeCmd.ExecuteReaderAsync();
        if (!await getChallengeReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find challenge");
        }

        ChallengeState challengeState = (ChallengeState)getChallengeReader.GetInt32(1);
        if (challengeState != ChallengeState.InProgress)
        {
            throw new InvalidOperationException("Challenge is not in progress");
        }

        int? currentStepIndex = getChallengeReader.IsDBNull(2)
            ? null
            : getChallengeReader.GetInt32(2);
        if (currentStepIndex is null)
        {
            throw new InvalidOperationException("Challenge has no current step");
        }

        int stepCount = getChallengeReader.GetInt32(3);
        string deckName = getChallengeReader.GetString(4);

        //get current step
        SqliteCommand getStepCmd = connection.CreateCommand(
            """
            SELECT flashcardId from steps
            WHERE challengeId = @challengeId AND stepIndex = @stepIndex
            LIMIT 1;
            """);
        getStepCmd.Parameters.AddWithValue("@challengeId", challengeId);
        getStepCmd.Parameters.AddWithValue("@stepIndex", currentStepIndex);
        object? scalar = await getStepCmd.ExecuteScalarAsync();
        if (scalar is not long flashcardId)
        {
            throw new InvalidOperationException("Could not find current step");
        }

        //get flashcard
        SqliteCommand getFlashcardCmd = connection.CreateCommand(
            """
            SELECT id, question, answer, deckId
            FROM flashcards
            WHERE id = @id
            LIMIT 1;
            """);
        getFlashcardCmd.Parameters.AddWithValue("@id", flashcardId);

        await using SqliteDataReader getFlashcardReader = await getFlashcardCmd.ExecuteReaderAsync();
        if (!await getFlashcardReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find flashcard for challenge");
        }

        CurrentFlashcard = new FlashcardItem(
            getFlashcardReader.GetInt32(0),
            getFlashcardReader.GetString(1),
            getFlashcardReader.GetString(2),
            deckName,
            stepCount,
            currentStepIndex.Value + 1);

        Input = new StepInput(
            challengeId,
            CurrentFlashcard.Id,
            false);

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input is null)
        {
            throw new InvalidOperationException("Input is null");
        }

        await using SqliteConnection connection = SqliteConnectionFactory.Create(User, true);
        await connection.OpenAsync();

        await using DbTransaction transaction = await connection.BeginTransactionAsync();

        await using SqliteCommand getChallengeCmd = connection.CreateCommand(
            """
            SELECT state, currentStepIndex, stepCount
            FROM challenges
            WHERE id = @id;
            """);
        getChallengeCmd.Parameters.AddWithValue("@id", Input.ChallengeId);

        await using SqliteDataReader getChallengeReader = await getChallengeCmd.ExecuteReaderAsync();
        if (!await getChallengeReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find challenge");
        }

        ChallengeState challengeState = (ChallengeState)getChallengeReader.GetInt64(0);
        if (challengeState != ChallengeState.InProgress)
        {
            throw new InvalidOperationException("Challenge is not in progress");
        }

        int? currentStepIndex = getChallengeReader.IsDBNull(1)
            ? null
            : getChallengeReader.GetInt32(1);
        if (currentStepIndex is null)
        {
            throw new InvalidOperationException("Challenge has no current step");
        }

        int stepCount = getChallengeReader.GetInt32(2);

        if (Input.NeedsReview)
        {
            const string updateStepSql =
                "UPDATE steps SET needsReview = 1 WHERE challengeId = @challengeId AND stepIndex = @stepIndex;";
            await using SqliteCommand updateStepCmd = connection.CreateCommand(
                """
                UPDATE steps
                SET needsReview = 2
                WHERE challengeId = @challengeId AND stepIndex = @stepIndex;
                """);
            updateStepCmd.CommandText = updateStepSql;
            updateStepCmd.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
            updateStepCmd.Parameters.AddWithValue("@stepIndex", currentStepIndex);
            await updateStepCmd.ExecuteNonQueryAsync();
        }

        if (currentStepIndex == stepCount - 1)
        {
            await using SqliteCommand needsReviewCmd = connection.CreateCommand(
                """
                SELECT EXISTS(
                    SELECT 1
                    FROM steps
                    WHERE challengeId = @challengeId AND needsReview = 1);
                """);
            needsReviewCmd.Parameters.AddWithValue("@challengeId", Input.ChallengeId);

            bool needsReview = Convert.ToBoolean(await needsReviewCmd.ExecuteScalarAsync());
            if (needsReview)
            {
                challengeState = ChallengeState.InReview;

                await using SqliteCommand setChallengeStateCommand = connection.CreateCommand(
                    """
                    UPDATE challenges
                    SET state = @state
                    WHERE id = @id;
                    """);
                setChallengeStateCommand.Parameters.AddWithValue("@state", ChallengeState.InReview);
                setChallengeStateCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
                await setChallengeStateCommand.ExecuteNonQueryAsync();

                await using SqliteCommand setCurrentStepIndexCommand = connection.CreateCommand(
                    """
                    UPDATE challenges
                    SET currentStepIndex = (
                        SELECT stepIndex
                        FROM steps
                        WHERE challengeId = @challengeId AND needsReview = 1
                        ORDER BY stepIndex
                        LIMIT 1)
                    WHERE id = @id;
                    """);
                setCurrentStepIndexCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
                setCurrentStepIndexCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
                await setCurrentStepIndexCommand.ExecuteNonQueryAsync();
            }
            else
            {
                challengeState = ChallengeState.Complete;

                await using SqliteCommand setChallengeStateCommand = connection.CreateCommand(
                    """
                    UPDATE challenges
                    SET state = @state
                    WHERE id = @id;
                    """);
                setChallengeStateCommand.Parameters.AddWithValue("@state", ChallengeState.Complete);
                setChallengeStateCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
                await setChallengeStateCommand.ExecuteNonQueryAsync();
            }
        }
        else
        {
            await using SqliteCommand incrementCurrentStepIndexCommand = connection.CreateCommand(
                """
                UPDATE challenges
                SET currentStepIndex = currentStepIndex + 1
                WHERE id = @id;
                """);
            incrementCurrentStepIndexCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
            await incrementCurrentStepIndexCommand.ExecuteNonQueryAsync();
        }

        await using SqliteCommand updateUpdatedAtCommand = connection.CreateCommand(
            """
            UPDATE challenges
            SET updatedAt = @updatedAt
            WHERE id = @id;
            """);
        updateUpdatedAtCommand.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        updateUpdatedAtCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
        await updateUpdatedAtCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();

        return challengeState != ChallengeState.InProgress
            ? RedirectToPage("CompleteChallenge", new { challengeId = Input.ChallengeId })
            : RedirectToPage("Engage", new { challengeId = Input.ChallengeId });
    }
}
