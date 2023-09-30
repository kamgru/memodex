using System.ComponentModel.DataAnnotations;
using System.Data.Common;
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

        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User, true);
        await connection.OpenAsync();
        await using DbTransaction transaction = await connection.BeginTransactionAsync();
        await using SqliteCommand insertFlashcardCommand = connection.CreateCommand(
            """
            INSERT INTO flashcards (deckId, question, answer)
            VALUES (@deckId, @question, @answer)
            """);
        insertFlashcardCommand.Parameters.AddWithValue("@deckId", Input.DeckId);
        insertFlashcardCommand.Parameters.AddWithValue("@question", Input.Question);
        insertFlashcardCommand.Parameters.AddWithValue("@answer", Input.Answer);
        await insertFlashcardCommand.ExecuteNonQueryAsync();

        await using SqliteCommand updateDeckCommand = connection.CreateCommand(
            """
            UPDATE decks
            SET flashcardCount = flashcardCount + 1
            WHERE id = @deckId
            """);
        updateDeckCommand.Parameters.AddWithValue("@deckId", Input.DeckId);
        await updateDeckCommand.ExecuteNonQueryAsync();

        await transaction.CommitAsync();

        return RedirectToPage("EditFlashcards", new { deckId = Input.DeckId });
    }
}