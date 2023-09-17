using System.ComponentModel.DataAnnotations;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class AddDeck : PageModel
{
    public class FormInput
    {
        [Required]
        [MaxLength(256)]
        public string Name { get; init; } = string.Empty;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User, true);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            INSERT INTO decks (name) 
            VALUES (@name)
            RETURNING id
            """);
        
        command.Parameters.AddWithValue("@name", Input.Name);

        int deckId = Convert.ToInt32(await command.ExecuteScalarAsync());
        
        return RedirectToPage("EditDeck", new { deckId });
    }
}
