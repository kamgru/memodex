using System.Data.Common;
using System.Security.Claims;
using Memodex.WebApp.Common;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;

namespace Memodex.WebApp.Pages;

public class EditFlashcards : PageModel
{
    public record PageInfo(
        int DeckId,
        int ItemsPerPage,
        PagedData<FlashcardItem> Data);

    public record FlashcardItem(
        int Id,
        string Question,
        string Answer,
        int OrdinalNumber);

    public class EditFlashcardItem
    {
        public EditFlashcardItem(
            int id,
            int deckId,
            string question,
            string answer,
            int ordinalNumber)
        {
            Id = id;
            DeckId = deckId;
            Question = question;
            QuestionLineCount = question.Split(Environment.NewLine)
                .Length;
            Answer = answer;
            AnswerLineCount = answer.Split(Environment.NewLine)
                .Length;
            OrdinalNumber = ordinalNumber;
        }

        public int Id { get; init; }
        public int DeckId { get; init; }
        public string Question { get; init; }
        public int QuestionLineCount { get; init; }
        public string Answer { get; init; }
        public int AnswerLineCount { get; init; }
        public int OrdinalNumber { get; init; }
    }

    public record UpdateFlashcardRequest(
        int FlashcardId,
        string Question,
        string Answer);

    public record DeleteFlashcardRequest(
        int FlashcardId);

    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public EditFlashcards(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    public PageInfo? CurrentPageInfo { get; set; }

    public async Task<IActionResult> OnGetAsync(
        int deckId,
        int itemsPerPage = 25)
    {
        FlashcardReader flashcardReader = new(_sqliteConnectionFactory, User);
        PagedData<FlashcardItem> data = await flashcardReader.GetManyFlashcardsAsync(
            deckId,
            1,
            itemsPerPage);

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
        FlashcardReader flashcardReader = new(_sqliteConnectionFactory, User);
        PagedData<FlashcardItem> data = await flashcardReader.GetManyFlashcardsAsync(
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
        FlashcardReader flashcardReader = new(_sqliteConnectionFactory, User);
        FlashcardItem? flashcard = await flashcardReader.GetSingleFlashcard(flashcardId);

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
        EditFlashcardReader editFlashcardReader = new(_sqliteConnectionFactory, User);
        EditFlashcardItem flashcard = await editFlashcardReader.GetEditAsync(flashcardId);

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
        UpdateFlashcardWriter updateFlashcardWriter = new(_sqliteConnectionFactory, User);
        int ordinalNumber = await updateFlashcardWriter.UpdateFlashcardAsync(
            request.FlashcardId,
            request.Question,
            request.Answer);

        FlashcardItem flashcard = new(
            request.FlashcardId,
            request.Question,
            request.Answer,
            ordinalNumber);

        return new PartialViewResult
        {
            ViewData = new ViewDataDictionary(new EmptyModelMetadataProvider(), new ModelStateDictionary())
            {
                Model = flashcard
            },
            ViewName = "_FlashcardItemPartial"
        };
    }

    public class UpdateFlashcardWriter
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public UpdateFlashcardWriter(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task<int> UpdateFlashcardAsync(
            int flashcardId,
            string question,
            string answer)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal);
            await connection.OpenAsync();
            await using SqliteCommand command = connection.CreateCommand(
                """
                UPDATE flashcards
                SET question = @question, answer = @answer
                WHERE id = @id
                RETURNING ordinalNumber;
                """);
            command.Parameters.AddWithValue("@question", question);
            command.Parameters.AddWithValue("@answer", answer);
            command.Parameters.AddWithValue("@id", flashcardId);
            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
    }

    public async Task<IActionResult> OnPostDeleteFlashcardAsync(
        [FromQuery]
        DeleteFlashcardRequest request)
    {
        DeleteFlashcardWriter deleteFlashcardWriter = new(_sqliteConnectionFactory, User);
        await deleteFlashcardWriter.DeleteFlashcardAsync(request.FlashcardId);

        return new EmptyResult();
    }

    public class DeleteFlashcardWriter
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public DeleteFlashcardWriter(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task DeleteFlashcardAsync(
            int flashcardId)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal, true);
            await connection.OpenAsync();
            await using DbTransaction transaction = await connection.BeginTransactionAsync();
            await using SqliteCommand deleteFlashcardCmd = connection.CreateCommand(
                """
                DELETE FROM flashcards
                WHERE id = @id
                RETURNING deckId;
                """);
            deleteFlashcardCmd.Parameters.AddWithValue("@id", flashcardId);
            int deckId = Convert.ToInt32(await deleteFlashcardCmd.ExecuteScalarAsync());

            await using SqliteCommand deleteChallengeCmd = connection.CreateCommand(
                """
                DELETE FROM challenges
                WHERE deckId = @deckId;
                """);
            deleteChallengeCmd.Parameters.AddWithValue("@deckId", deckId);
            await deleteChallengeCmd.ExecuteNonQueryAsync();

            await using SqliteCommand updateDeckCmd = connection.CreateCommand(
                """
                UPDATE decks
                SET flashcardCount = flashcardCount - 1
                WHERE id = @deckId;
                """);
            updateDeckCmd.Parameters.AddWithValue("@deckId", deckId);
            await updateDeckCmd.ExecuteNonQueryAsync();
            await transaction.CommitAsync();
            await connection.CloseAsync();
        }
    }

    public class FlashcardReader
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public FlashcardReader(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task<PagedData<FlashcardItem>> GetManyFlashcardsAsync(
            int deckId,
            int pageNumber,
            int itemsPerPage)
        {
            pageNumber = Math.Max(1, pageNumber);
            itemsPerPage = Math.Max(10, itemsPerPage);

            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal);
            await connection.OpenAsync();
            await using SqliteCommand selectFlashcardsCmd = connection.CreateCommand(
                """
                SELECT id, question, answer, ordinalNumber
                FROM flashcards
                WHERE deckId = @deckId
                ORDER BY id
                LIMIT @limit
                OFFSET @offset;
                """);
            selectFlashcardsCmd.Parameters.AddWithValue("@deckId", deckId);
            selectFlashcardsCmd.Parameters.AddWithValue("@limit", itemsPerPage);
            selectFlashcardsCmd.Parameters.AddWithValue("@offset", (pageNumber - 1) * itemsPerPage);
            await using SqliteDataReader reader = await selectFlashcardsCmd.ExecuteReaderAsync();

            List<FlashcardItem> flashcards = new();
            while (await reader.ReadAsync())
                flashcards.Add(new FlashcardItem(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetInt32(3)));

            SqliteCommand countFlashcardsCmd = connection.CreateCommand(
                """
                SELECT COUNT(id)
                FROM flashcards
                WHERE deckId = @deckId;
                """);
            countFlashcardsCmd.Parameters.AddWithValue("@deckId", deckId);
            long totalFlashcardCount = Convert.ToInt64(await countFlashcardsCmd.ExecuteScalarAsync());
            int totalPageCount = (int)Math.Ceiling((double)totalFlashcardCount / itemsPerPage);

            return new PagedData<FlashcardItem>
            {
                ItemCount = totalFlashcardCount,
                Page = pageNumber,
                TotalPages = totalPageCount,
                Items = flashcards
            };
        }

        public async Task<FlashcardItem?> GetSingleFlashcard(
            int flashcardId)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal);
            await connection.OpenAsync();
            await using SqliteCommand command = connection.CreateCommand(
                """
                SELECT id, question, answer, ordinalNumber
                FROM flashcards
                WHERE id = @flashcardId
                LIMIT 1;
                """);
            command.Parameters.AddWithValue("@flashcardId", flashcardId);
            await using SqliteDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            FlashcardItem flashcard = new(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3));

            await connection.CloseAsync();
            return flashcard;
        }
    }

    public class EditFlashcardReader
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public EditFlashcardReader(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task<EditFlashcardItem> GetEditAsync(
            int flashcardId)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal);
            await connection.OpenAsync();
            await using SqliteCommand command = connection.CreateCommand(
                """
                SELECT id, deckId, question, answer, ordinalNumber
                FROM flashcards
                WHERE id = @flashcardId
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
                reader.GetString(3),
                reader.GetInt32(4));

            await connection.CloseAsync();
            return flashcard;
        }
    }
}
