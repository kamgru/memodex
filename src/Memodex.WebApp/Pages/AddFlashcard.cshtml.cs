using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Security.Claims;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class AddFlashcard : PageModel
{
    public class FormInput
    {
        [Required]
        public int DeckId { get; set; }

        [Required]
        public string Question { get; init; } = string.Empty;

        [Required]
        public string Answer { get; init; } = string.Empty;
    }

    public class AddFlashcardWriter
    {
        private readonly ClaimsPrincipal _claimsPrincipal;
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;

        public AddFlashcardWriter(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task<int> AddFlashcardAsync(
            int deckId,
            string question,
            string answer)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal, true);
            await connection.OpenAsync();
            await using DbTransaction transaction = await connection.BeginTransactionAsync();
            await using SqliteCommand insertFlashcardCommand = connection.CreateCommand(
                """
                INSERT INTO flashcards (deckId, question, answer, ordinalNumber)
                VALUES (@deckId, @question, @answer, (SELECT COUNT(*) FROM flashcards WHERE deckId = @deckId) + 1)
                RETURNING id;
                """);
            insertFlashcardCommand.Parameters.AddWithValue("@deckId", deckId);
            insertFlashcardCommand.Parameters.AddWithValue("@question", question);
            insertFlashcardCommand.Parameters.AddWithValue("@answer", answer);
            int flashcardId = Convert.ToInt32(await insertFlashcardCommand.ExecuteScalarAsync());

            await using SqliteCommand updateDeckCommand = connection.CreateCommand(
                """
                UPDATE decks
                SET flashcardCount = flashcardCount + 1
                WHERE id = @deckId
                RETURNING id;
                """);
            updateDeckCommand.Parameters.AddWithValue("@deckId", deckId);
            await updateDeckCommand.ExecuteNonQueryAsync();
            await transaction.CommitAsync();

            return flashcardId;
        }
    }

    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public AddFlashcard(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public IActionResult OnGet(
        int deckId)
    {
        Input.DeckId = deckId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        AddFlashcardWriter addFlashcardWriter = new(_sqliteConnectionFactory, User);
        await addFlashcardWriter.AddFlashcardAsync(
            Input.DeckId,
            Input.Question,
            Input.Answer);

        return RedirectToPage("EditFlashcards", new { deckId = Input.DeckId });
    }
}
