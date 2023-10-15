namespace Memodex.Tests.Integration.DataAccess;

public class FlashcardReaderTests : TestFixtureBase
{
    [Test]
    public async Task GetSingleFlashcardAsync_ReturnsCorrectFlashcard()
    {
        // Arrange
        await using SqliteConnection connection = DbFixture.CreateConnectionForUser();
        await connection.OpenAsync();
        await using SqliteCommand insertDeckCmd = connection.CreateCommand(
            """
            INSERT INTO decks (name, description)
            VALUES ('Test Deck', 'Test Description')
            RETURNING id;
            """);
        int deckId = Convert.ToInt32(await insertDeckCmd.ExecuteScalarAsync());

        await using SqliteCommand insertFlashcardsCmd = connection.CreateCommand(
            """
            INSERT INTO flashcards (question, answer, deckId)
            VALUES
                ('Question 1', 'Answer 1', @deckId),
                ('Question 2', 'Answer 2', @deckId),
                ('Question 3', 'Answer 3', @deckId)
            RETURNING id;
            """);
        insertFlashcardsCmd.Parameters.AddWithValue("@deckId", deckId);
        await using SqliteDataReader reader = await insertFlashcardsCmd.ExecuteReaderAsync();
        List<int> flashcardIds = new();
        while (await reader.ReadAsync())
        {
            flashcardIds.Add(reader.GetInt32(0));
        }

        await connection.CloseAsync();

        // Act
        EditFlashcards.FlashcardReader flashcardReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        EditFlashcards.FlashcardItem? flashcardItem =
            await flashcardReader.GetSingleFlashcard(flashcardIds[1]);

        // Assert
        Assert.That(flashcardItem?.Question, Is.EqualTo("Question 2"));
    }

    [Test]
    public async Task GetSingleFlashcardAsync_WhenFlashcardNotFound_ReturnsNull()
    {
        // Arrange
        EditFlashcards.FlashcardReader flashcardReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        
        // Act
        EditFlashcards.FlashcardItem? flashcardItem = await flashcardReader.GetSingleFlashcard(1);

        // Assert
        Assert.That(flashcardItem, Is.Null);
    }

    [Test]
    public async Task GetManyFlashcardsAsync_ReturnsCorrectFlashcards()
    {
        // Arrange
        int deckId = await DbFixture.SeedFlashcards();

        // Act
        EditFlashcards.FlashcardReader flashcardReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        PagedData<EditFlashcards.FlashcardItem> flashcardItems =
            await flashcardReader.GetManyFlashcardsAsync(deckId, 1, 10);

        // Assert
        Assert.Multiple(() =>
        {
            Assert.That(flashcardItems.Items, Has.Count.EqualTo(10));
            Assert.That(flashcardItems.Items[0].Question, Is.EqualTo("Question 1"));
            Assert.That(flashcardItems.Items[9].Question, Is.EqualTo("Question 10"));
        });
    }

    [Test]
    public async Task GetManyFlashcardsAsync_ReturnsCorrectTotalItemCount()
    {
        // Arrange
        int deckId = await DbFixture.SeedFlashcards();

        // Act
        EditFlashcards.FlashcardReader flashcardReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        PagedData<EditFlashcards.FlashcardItem> flashcardItems =
            await flashcardReader.GetManyFlashcardsAsync(deckId, 1, 3);

        // Assert
        Assert.That(flashcardItems.ItemCount, Is.EqualTo(35));
    }

    [Test]
    public async Task GetManyFlashcardsAsync_WhenPageNumberIsTooHigh_ReturnsEmptyList()
    {
        // Arrange
        int deckId = await DbFixture.SeedFlashcards();

        // Act
        EditFlashcards.FlashcardReader flashcardReader =
            new(DbFixture.SqliteConnectionFactory, DbFixture.ClaimsPrincipal);
        PagedData<EditFlashcards.FlashcardItem> flashcardItems =
            await flashcardReader.GetManyFlashcardsAsync(deckId, 10, 10);

        // Assert
        CollectionAssert.IsEmpty(flashcardItems.Items);
    }
}
