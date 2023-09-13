using System.Data.Common;
using Memodex.WebApp.Common;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class EditFlashcards : PageModel
{
    private async Task<PagedData<FlashcardItem>> GetFlashcardsAsync(
        int deckId,
        int pageNumber,
        int itemsPerPage)
    {
        pageNumber = Math.Max(1, pageNumber);
        itemsPerPage = Math.Max(10, itemsPerPage);

        await using SqliteConnection connection = SqliteConnectionFactory.Create("memodex_test.sqlite");
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT flashcard.`id`, flashcard.`question`, flashcard.`answer` FROM `flashcards` flashcard
            WHERE flashcard.`deckId` = @deckId
            ORDER BY flashcard.`id`
            LIMIT @limit
            OFFSET @offset;
            """);
        command.Parameters.AddWithValue("@deckId", deckId);
        command.Parameters.AddWithValue("@limit", itemsPerPage);
        command.Parameters.AddWithValue("@offset", (pageNumber - 1) * itemsPerPage);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();

        List<FlashcardItem> items = new();
        while (await reader.ReadAsync())
        {
            items.Add(new FlashcardItem(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2)));
        }

        SqliteCommand countSqliteCommand = connection.CreateCommand(
            "SELECT COUNT(`id`) FROM `flashcards` WHERE `deckId` = @deckId;");
        countSqliteCommand.Parameters.AddWithValue("@deckId", deckId);
        long totalCount = Convert.ToInt64(await countSqliteCommand.ExecuteScalarAsync());
        int totalPages = (int)Math.Ceiling((double)totalCount / itemsPerPage);

        return new PagedData<FlashcardItem>
        {
            ItemCount = totalCount,
            Page = pageNumber,
            TotalPages = totalPages,
            Items = items
        };
    }

    public async Task<IActionResult> OnGetAsync(
        int deckId,
        int itemsPerPage = 25)
    {
        PagedData<FlashcardItem> data = await GetFlashcardsAsync(deckId, 1, itemsPerPage);

        CurrentPageInfo = new PageInfo(
            deckId,
            itemsPerPage,
            data);

        return Page();
    }

    public async Task<IActionResult> OnGetFlashcardsAsync(
        int deckId,
        int pageNumber,
        int itemsPerPage)
    {
        PagedData<FlashcardItem> data = await GetFlashcardsAsync(
            deckId,
            pageNumber,
            itemsPerPage);

        return data.Items.Any()
            ? new PartialViewResult
            {
                ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
                {
                    Model = new PageInfo(
                        deckId,
                        itemsPerPage,
                        data)
                },
                ViewName = "_FlashcardsListPartial"
            }
            : new EmptyResult();
    }

    public async Task<IActionResult> OnGetSingleFlashcardAsync(
        int flashcardId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create("memodex_test.sqlite");
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT flashcard.`id`, flashcard.`question`, flashcard.`answer` FROM `flashcards` flashcard
            WHERE flashcard.`id` = @flashcardId
            LIMIT 1;
            """);
        command.Parameters.AddWithValue("@flashcardId", flashcardId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException($"Flashcard {flashcardId} not found");
        }

        FlashcardItem flashcard = new(
            reader.GetInt32(0),
            reader.GetString(1),
            reader.GetString(2));

        return new PartialViewResult
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = flashcard
            },
            ViewName = "_FlashcardItemPartial"
        };
    }

    public async Task<IActionResult> OnGetEditAsync(
        int flashcardId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create("memodex_test.sqlite");
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT flashcard.`id`, flashcard.`deckId`, flashcard.`question`, flashcard.`answer`
            FROM `flashcards` flashcard
            WHERE flashcard.`id` = @flashcardId
            LIMIT 1;
            """);

        command.Parameters.AddWithValue("@flashcardId", flashcardId);
        await using SqliteDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            throw new InvalidOperationException($"Flashcard {flashcardId} not found");
        }

        EditFlashcardItem flashcard = new(
            reader.GetInt32(0),
            reader.GetInt32(1),
            reader.GetString(2),
            reader.GetString(3));

        return new PartialViewResult
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = flashcard
            },
            ViewName = "_EditFlashcardPartial"
        };
    }

    public async Task<IActionResult> OnPostUpdateFlashcardAsync(
        [FromForm]
        UpdateFlashcardRequest request)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create("memodex_test.sqlite");
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            "UPDATE `flashcards` SET `question` = @question, `answer` = @answer WHERE `id` = @id;");
        command.Parameters.AddWithValue("@question", request.Question);
        command.Parameters.AddWithValue("@answer", request.Answer);
        command.Parameters.AddWithValue("@id", request.FlashcardId);
        await command.ExecuteNonQueryAsync();

        FlashcardItem flashcard = new(request.FlashcardId, request.Question, request.Answer);

        return new PartialViewResult
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = flashcard
            },
            ViewName = "_FlashcardItemPartial"
        };
    }

    public async Task<IActionResult> OnPostDeleteFlashcardAsync(
        [FromQuery]
        DeleteFlashcardRequest request)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create("memodex_test.sqlite", true);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        await using SqliteCommand command = connection.CreateCommand(
            "DELETE FROM `flashcards` WHERE `id` = @id RETURNING `deckId`;");
        command.Parameters.AddWithValue("@id", request.FlashcardId);
        int deckId = Convert.ToInt32(await command.ExecuteNonQueryAsync());

        //reduce deck flashcard count
        SqliteCommand updateDeckCommand = connection.CreateCommand(
            "UPDATE `decks` SET `flashcardCount` = `flashcardCount` - 1 WHERE `id` = @id;");
        updateDeckCommand.Parameters.AddWithValue("@id", deckId);
        await updateDeckCommand.ExecuteNonQueryAsync();

        //delete challenges based on this deck
        SqliteCommand deleteChallengesCommand = connection.CreateCommand(
            "DELETE FROM `challenges` WHERE `deckId` = @id;");
        deleteChallengesCommand.Parameters.AddWithValue("@id", deckId);
        await deleteChallengesCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();

        return new EmptyResult();
    }

    public record PageInfo(
        int DeckId,
        int ItemsPerPage,
        PagedData<FlashcardItem> Data);

    public PageInfo? CurrentPageInfo { get; set; }

    public record FlashcardItem(
        int Id,
        string Question,
        string Answer);

    public record EditFlashcardItem(
        int Id,
        int DeckId,
        string Question,
        string Answer);

    public record UpdateFlashcardRequest(
        int FlashcardId,
        string Question,
        string Answer);

    public record DeleteFlashcardRequest(
        int FlashcardId);
}
