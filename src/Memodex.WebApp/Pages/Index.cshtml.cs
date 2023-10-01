using System.Data.Common;
using Memodex.WebApp.Data;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class IndexModel : PageModel
{
    public record PastChallenges(
        List<UnfinishedChallenge> UnfinishedChallenges,
        List<InReviewChallenge> InReviewChallenges);

    public record UnfinishedChallenge(
        int Id,
        DateTime CreatedAt,
        DateTime UpdatedAt,
        string DeckName,
        int CurrentStep,
        int TotalSteps);

    public record InReviewChallenge(
        int Id,
        DateTime CreatedOn,
        string DeckName,
        int StepsToReview);

    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public IndexModel(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    public PastChallenges? Challenges { get; set; }

    public async Task<IActionResult> OnGet()
    {
        await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(User);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        await using SqliteCommand getUnfinishedChallengeStepsCommand = connection.CreateCommand(
            """
            SELECT challenges.id, createdAt, updatedAt, name, currentStepIndex, flashcardCount
            FROM challenges
            JOIN decks ON decks.id = challenges.deckId
            JOIN steps ON steps.challengeId = challenges.id
            WHERE challenges.state = @state
            GROUP BY challenges.id, createdAt, updatedAt, name, currentStepIndex, flashcardCount;
            """);
        getUnfinishedChallengeStepsCommand.Parameters.AddWithValue("@state", ChallengeState.InProgress);
        await using SqliteDataReader reader = await getUnfinishedChallengeStepsCommand.ExecuteReaderAsync();
        List<UnfinishedChallenge> unfinishedChallenges = new();
        while (await reader.ReadAsync())
            unfinishedChallenges.Add(new UnfinishedChallenge(
                reader.GetInt32(0),
                reader.GetDateTime(1),
                reader.GetDateTime(2),
                reader.GetString(3),
                reader.GetInt32(4) + 1,
                reader.GetInt32(5)));

        await using SqliteCommand getInReviewChallenges = connection.CreateCommand(
            """
            SELECT challenges.id, createdAt, name, COUNT(steps.id)
            FROM challenges
            JOIN decks ON decks.id = challenges.deckId
            JOIN steps ON steps.challengeId = challenges.id
            WHERE challenges.state = 1
            AND steps.needsReview = 1
            GROUP BY challenges.id, createdAt, name
            HAVING COUNT(steps.id) > 0;
            """);
        getInReviewChallenges.Parameters.AddWithValue("@state", ChallengeState.InReview);
        await using SqliteDataReader needsReviewReader = await getInReviewChallenges.ExecuteReaderAsync();

        List<InReviewChallenge> inReviewChallenges = new();
        while (await needsReviewReader.ReadAsync())
            inReviewChallenges.Add(new InReviewChallenge(
                needsReviewReader.GetInt32(0),
                needsReviewReader.GetDateTime(1),
                needsReviewReader.GetString(2),
                needsReviewReader.GetInt32(3)));

        await transaction.CommitAsync();

        Challenges = new PastChallenges(unfinishedChallenges, inReviewChallenges);
        return Page();
    }
}
