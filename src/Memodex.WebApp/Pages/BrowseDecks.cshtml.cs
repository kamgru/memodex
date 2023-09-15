using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class BrowseDecks : PageModel
{
    public record DeckItem(
        int Id,
        string Name,
        string Description,
        int ItemCount);

    public int CurrentCategoryId { get; set; }
    public List<DeckItem> Decks { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int categoryId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User);
        await connection.OpenAsync();

        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT id, name, description, flashcardCount
            FROM decks
            WHERE categoryId = @categoryId;
            """);
        command.Parameters.AddWithValue("@categoryId", categoryId);

        SqliteDataReader reader = await command.ExecuteReaderAsync();
        while (await reader.ReadAsync())
        {
            Decks.Add(new DeckItem(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetValue(2) as string ?? string.Empty,
                reader.GetInt32(3)));
        }

        CurrentCategoryId = categoryId;
        return Page();
    }
}
