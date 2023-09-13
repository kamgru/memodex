using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class AddFlashcard : PageModel
{
    public class FormInput
    {
        public int DeckId { get; init; }
        public required string Question { get; set; }
        public required string Answer { get; set; }
    }

    [BindProperty]
    public FormInput? Input { get; set; }

    public IActionResult OnGet(
        int deckId)
    {
        Input = new FormInput
        {
            DeckId = deckId,
            Question = string.Empty,
            Answer = string.Empty
        };

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await using SqliteConnection connection = SqliteConnectionFactory.Create("memodex_test.sqlite", true);
        await connection.OpenAsync();
        SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO `flashcards` (`deckId`, `question`, `answer`) 
            VALUES (@deckId, @question, @answer)
            """);
        command.Parameters.AddWithValue("@deckId", Input!.DeckId);
        command.Parameters.AddWithValue("@question", Input!.Question);
        command.Parameters.AddWithValue("@answer", Input!.Answer);
        await command.ExecuteNonQueryAsync();
        
        return RedirectToPage("EditFlashcards", new { deckId = Input!.DeckId });
    }
}