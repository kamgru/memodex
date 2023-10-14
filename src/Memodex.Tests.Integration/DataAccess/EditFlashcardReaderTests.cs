namespace Memodex.Tests.Integration.DataAccess;

public class GetEditReaderTests : TestFixtureBase
{
    [Test]
    public async Task GetEditAsync_ReturnsCorrectFlashcard()
    {
        // Arrange
        await DbFixture.CreateUserDb();

        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand deckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES ('Test Deck', 'Test Description')
            RETURNING id;
            """);

        int deckId = Convert.ToInt32(await deckCmd.ExecuteScalarAsync());

        await using SqliteCommand flashcardCmd = connection.CreateCommand(
            """
            INSERT INTO flashcards (question, answer, deckId)
            VALUES
                ('Question 1', 'Answer 1', @deckId),
                ('Question 2', 'Answer 2', @deckId),
                ('Question 3', 'Answer 3', @deckId)
            RETURNING id;
            """);
        flashcardCmd.Parameters.AddWithValue("@deckId", deckId);
        await using SqliteDataReader reader = await flashcardCmd.ExecuteReaderAsync();
        List<int> flashcardIds = new();
        while (await reader.ReadAsync())
        {
            flashcardIds.Add(reader.GetInt32(0));
        }

        await connection.CloseAsync();

        // Act
        EditFlashcards.EditFlashcardReader editFlashcardReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        EditFlashcards.EditFlashcardItem editFlashcardItem =
            await editFlashcardReader.GetEditAsync(flashcardIds[1]);
        
        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(editFlashcardItem.Question, Is.EqualTo("Question 2"));
            Assert.That(editFlashcardItem.Answer, Is.EqualTo("Answer 2"));
            Assert.That(editFlashcardItem.DeckId, Is.EqualTo(deckId));
        });
    }
}
