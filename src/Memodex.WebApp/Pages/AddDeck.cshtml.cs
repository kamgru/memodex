using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class AddDeck : PageModel
{
    public class DeckItem
    {
        public int CategoryId { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    [BindProperty]
    public DeckItem Deck { get; set; } = new();

    public IActionResult OnGetAsync(
        int categoryId)
    {
        Deck.CategoryId = categoryId;
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User, true);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO `decks` (`categoryId`, `name`) 
            VALUES (@categoryId, @name)
            RETURNING `id`
            """);
        
        command.Parameters.AddWithValue("@categoryId", Deck.CategoryId);
        command.Parameters.AddWithValue("@name", Deck.Name);

        object scalar = await command.ExecuteScalarAsync()
                        ?? throw new InvalidOperationException("Scalar result was null.");
        int deckId = Convert.ToInt32(scalar);
        
        return RedirectToPage("EditDeck", new { deckId });
    }
}
