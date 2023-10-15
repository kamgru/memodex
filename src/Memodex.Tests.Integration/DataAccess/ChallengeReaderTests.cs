namespace Memodex.Tests.Integration.DataAccess;

public class ChallengeReaderTests : TestFixtureBase
{
    [Test]
    public async Task ReadChallengeAsync_WhenChallengeNotFound_ReturnsNull()
    {
        // Arrange
        CompleteChallenge.ChallengeReader challengeReader = new(
            DbFixture.SqliteConnectionFactory,
            DbFixture.ClaimsPrincipal);
        
        // Act
        CompleteChallenge.ChallengeItem? result = await challengeReader.ReadChallengeAsync(1);

        // Assert
        Assert.That(result, Is.Null);
    }

    [Theory]
    [TestCase("Test Deck 1", ChallengeState.InReview)]
    [TestCase("Test Deck 2", ChallengeState.InProgress)]
    [TestCase("Test Deck 3", ChallengeState.Complete)]
    public async Task ReadChallengeAsync_WhenChallengeExists_ReturnsCorrectChallenge(
        string deckName,
        ChallengeState challengeState)
    {
        // Arrange
        await using SqliteConnection connection =
            DbFixture.SqliteConnectionFactory.CreateForUser(DbFixture.ClaimsPrincipal);
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO decks (name)
            VALUES (@deckName);

            WITH inserted AS
                (SELECT last_insert_rowid() AS id)
                INSERT INTO challenges (deckId, state)
                VALUES (
                    (SELECT id from inserted LIMIT 1),
                    @challengeState)
                RETURNING id;
            """);
        command.Parameters.AddWithValue("@deckName", deckName);
        command.Parameters.AddWithValue("@challengeState", (int)challengeState);
        int challengeId = Convert.ToInt32(await command.ExecuteScalarAsync());
        await connection.CloseAsync();

        // Act
        CompleteChallenge.ChallengeReader challengeReader = new(
            DbFixture.SqliteConnectionFactory,
            DbFixture.ClaimsPrincipal);
        CompleteChallenge.ChallengeItem? result = await challengeReader.ReadChallengeAsync(challengeId);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Title, Is.EqualTo(deckName));
            Assert.That(result.State, Is.EqualTo(challengeState));
        });
    }
}
