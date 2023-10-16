using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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

    public class AddDeckWriter
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;

        public AddDeckWriter(
            SqliteConnectionFactory sqliteConnectionFactory)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
        }

        public async Task<int> AddDeckAsync(
            string name,
            ClaimsPrincipal user)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(user, true);
            await connection.OpenAsync();
            SqliteCommand command = connection.CreateCommand(
                """
                INSERT INTO decks (name)
                VALUES (@name)
                RETURNING id
                """);

            command.Parameters.AddWithValue("@name", name);

            return Convert.ToInt32(await command.ExecuteScalarAsync());
        }
    }

    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public AddDeck(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public IActionResult OnGet()
    {
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        int deckId = await new AddDeckWriter(_sqliteConnectionFactory).AddDeckAsync(
            Input.Name,
            User);

        return RedirectToPage("EditDeck", new { deckId });
    }
}
