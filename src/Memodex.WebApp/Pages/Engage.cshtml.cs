using System.Data;
using System.Data.Common;
using Memodex.WebApp.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class Engage : PageModel
{
    public FlashcardItem? CurrentFlashcard { get; set; }

    [BindProperty]
    public StepInput? Input { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int challengeId)
    {
        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = "memodex_test.sqlite"
        };
        await using SqliteConnection connection = new(connectionStringBuilder.ToString());
        await connection.OpenAsync();
        const string getChallengeSql = "SELECT `id`, `state`, `currentStepIndex`, `stepCount` FROM `challenges` WHERE `id` = @id";
        SqliteCommand getChallengeCommand = connection.CreateCommand();
        getChallengeCommand.CommandText = getChallengeSql;
        getChallengeCommand.Parameters.AddWithValue("@id", challengeId);
        await using SqliteDataReader getChallengeReader = await getChallengeCommand.ExecuteReaderAsync();

        if (!await getChallengeReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find challenge");
        }

        ChallengeState challengeState = (ChallengeState)getChallengeReader.GetInt32(1);
        int? currentStepIndex = getChallengeReader.IsDBNull(2)
            ? null
            : getChallengeReader.GetInt32(2);
        int stepCount = getChallengeReader.GetInt32(3);

        if (challengeState != ChallengeState.InProgress)
        {
            throw new InvalidOperationException("Challenge is not in progress");
        }

        if (currentStepIndex is null)
        {
            throw new InvalidOperationException("Challenge has no current step");
        }

        //get current step
        const string getCurrentStepSql =
            "SELECT `flashcardId` from `steps` WHERE `challengeId` = @challengeId AND `stepIndex` = @stepIndex LIMIT 1;";
        SqliteCommand getCurrentFlashcardCommand = connection.CreateCommand();
        getCurrentFlashcardCommand.CommandText = getCurrentStepSql;
        getCurrentFlashcardCommand.Parameters.AddWithValue("@challengeId", challengeId);
        getCurrentFlashcardCommand.Parameters.AddWithValue("@stepIndex", currentStepIndex);
        object? scalar = await getCurrentFlashcardCommand.ExecuteScalarAsync();
        if (scalar is not long flashcardId)
        {
            throw new InvalidOperationException("Could not find current step");
        }

        //get flashcard
        const string getFlashcardSql =
            "SELECT `id`, `question`, `answer`, `deckId` FROM `flashcards` WHERE `id` = @id LIMIT 1;";
        SqliteCommand getFlashcardCommand = connection.CreateCommand();
        getFlashcardCommand.CommandText = getFlashcardSql;
        getFlashcardCommand.Parameters.AddWithValue("@id", flashcardId);
        await using SqliteDataReader getFlashcardReader = await getFlashcardCommand.ExecuteReaderAsync();
        if (!await getFlashcardReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find flashcard for challenge");
        }

        FlashcardItem flashcard = new(
            getFlashcardReader.GetInt32(0),
            getFlashcardReader.GetString(1),
            getFlashcardReader.GetString(2),
            "Deck Title",
            stepCount,
            currentStepIndex.Value + 1);
        CurrentFlashcard = flashcard;
        Input = new StepInput(
            challengeId,
            flashcard.Id,
            false);


        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (Input is null)
        {
            throw new InvalidOperationException("Input is null");
        }

        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = "memodex_test.sqlite",
            ForeignKeys = true
        };
        await using SqliteConnection connection = new(connectionStringBuilder.ToString());
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync(IsolationLevel.RepeatableRead);

        //get challenge details
        const string getChallengeSql =
            "SELECT `state`, `currentStepIndex`, `stepCount` FROM `challenges` WHERE `id` = @id";
        SqliteCommand getChallengeCommand = connection.CreateCommand();
        getChallengeCommand.CommandText = getChallengeSql;
        getChallengeCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
        await using SqliteDataReader getChallengeReader = await getChallengeCommand.ExecuteReaderAsync();
        if (!await getChallengeReader.ReadAsync())
        {
            throw new InvalidOperationException("Could not find challenge");
        }

        ChallengeState challengeState = (ChallengeState)getChallengeReader.GetInt64(0);

        int? currentStepIndex = getChallengeReader.IsDBNull(1)
            ? null
            : getChallengeReader.GetInt32(1);

        int stepCount = getChallengeReader.GetInt32(2);

        if (challengeState != ChallengeState.InProgress)
        {
            throw new InvalidOperationException("Challenge is not in progress");
        }

        if (currentStepIndex is null)
        {
            throw new InvalidOperationException("Challenge has no current step");
        }

        if (Input.NeedsReview)
        {
            //update step
            const string updateStepSql =
                "UPDATE `steps` SET `needsReview` = 1 WHERE `challengeId` = @challengeId AND `stepIndex` = @stepIndex;";
            SqliteCommand updateStepCommand = connection.CreateCommand();
            updateStepCommand.CommandText = updateStepSql;
            updateStepCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
            updateStepCommand.Parameters.AddWithValue("@stepIndex", currentStepIndex);
            await updateStepCommand.ExecuteNonQueryAsync();
        }

        if (currentStepIndex == stepCount - 1)
        {
            //check if any steps have needsReview set to true
            const string checkNeedsReviewSql =
                "SELECT EXISTS(SELECT 1 FROM `steps` WHERE `challengeId` = @challengeId AND `needsReview` = 1);";
            SqliteCommand checkNeedsReviewCommand = connection.CreateCommand();
            checkNeedsReviewCommand.CommandText = checkNeedsReviewSql;
            checkNeedsReviewCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
            bool needsReview = Convert.ToBoolean(await checkNeedsReviewCommand.ExecuteScalarAsync());
            if (needsReview)
            {
                //set challenge state to in review
                challengeState = ChallengeState.InReview;
                const string setChallengeStateSql =
                    "UPDATE `challenges` SET `state` = @state WHERE `id` = @id;";
                SqliteCommand setChallengeStateCommand = connection.CreateCommand();
                setChallengeStateCommand.CommandText = setChallengeStateSql;
                setChallengeStateCommand.Parameters.AddWithValue("@state", ChallengeState.InReview);
                setChallengeStateCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
                await setChallengeStateCommand.ExecuteNonQueryAsync();

                //set current step index to first step with needsReview set to true
                const string setCurrentStepIndexSql =
                    """
                    UPDATE `challenges` SET `currentStepIndex` = (
                        SELECT `stepIndex` FROM `steps` 
                        WHERE `challengeId` = @challengeId AND `needsReview` = 1 ORDER BY `stepIndex` LIMIT 1) 
                    WHERE `id` = @id;
                    """;
                SqliteCommand setCurrentStepIndexCommand = connection.CreateCommand();
                setCurrentStepIndexCommand.CommandText = setCurrentStepIndexSql;
                setCurrentStepIndexCommand.Parameters.AddWithValue("@challengeId", Input.ChallengeId);
                setCurrentStepIndexCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
                await setCurrentStepIndexCommand.ExecuteNonQueryAsync();
            }
            else
            {
                //set challenge state to complete
                challengeState = ChallengeState.Complete;
                const string setChallengeStateSql =
                    "UPDATE `challenges` SET `state` = @state WHERE `id` = @id;";
                SqliteCommand setChallengeStateCommand = connection.CreateCommand();
                setChallengeStateCommand.CommandText = setChallengeStateSql;
                setChallengeStateCommand.Parameters.AddWithValue("@state", ChallengeState.Complete);
                setChallengeStateCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
                await setChallengeStateCommand.ExecuteNonQueryAsync();
            }
        }
        else
        {
            //increment current step index
            const string incrementCurrentStepIndexSql =
                "UPDATE `challenges` SET `currentStepIndex` = `currentStepIndex` + 1 WHERE `id` = @id;";
            SqliteCommand incrementCurrentStepIndexCommand = connection.CreateCommand();
            incrementCurrentStepIndexCommand.CommandText = incrementCurrentStepIndexSql;
            incrementCurrentStepIndexCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
            await incrementCurrentStepIndexCommand.ExecuteNonQueryAsync();
        }
        //update updatedAt
        const string updateUpdatedAtSql =
            "UPDATE `challenges` SET `updatedAt` = @updatedAt WHERE `id` = @id;";
        SqliteCommand updateUpdatedAtCommand = connection.CreateCommand();
        updateUpdatedAtCommand.CommandText = updateUpdatedAtSql;
        updateUpdatedAtCommand.Parameters.AddWithValue("@updatedAt", DateTime.UtcNow);
        updateUpdatedAtCommand.Parameters.AddWithValue("@id", Input.ChallengeId);
        await updateUpdatedAtCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();

        return challengeState != ChallengeState.InProgress
            ? RedirectToPage("CompleteChallenge", new { challengeId = Input.ChallengeId })
            : RedirectToPage("Engage", new { challengeId = Input.ChallengeId });
    }

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

}
