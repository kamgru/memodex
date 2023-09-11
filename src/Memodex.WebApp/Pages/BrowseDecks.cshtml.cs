using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class BrowseDecks : PageModel
{
    public async Task<IActionResult> OnGetAsync(
        int categoryId)
    {
        SqliteConnectionStringBuilder builder = new()
        {
            DataSource = "memodex_test.sqlite"
        };

        await using SqliteConnection connection = new(builder.ConnectionString);
        const string sql =
            "SELECT `id`, `name`, `description`, `flashcardCount` FROM `decks` WHERE `categoryId` = @categoryId";
        SqliteCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@categoryId", categoryId);
        await connection.OpenAsync();
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

    public int CurrentCategoryId { get; set; }

    public List<DeckItem> Decks { get; set; } = new();

    public record DeckItem(
        int Id,
        string Name,
        string Description,
        int ItemCount);
}