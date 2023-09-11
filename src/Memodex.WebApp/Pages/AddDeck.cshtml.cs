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
        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = "memodex_test.sqlite",
            ForeignKeys = true
        };
        await using SqliteConnection connection = new(connectionStringBuilder.ToString());
        const string sql = "INSERT INTO `decks` (`categoryId`, `name`) VALUES (@categoryId, @name)";
        SqliteCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@categoryId", Deck.CategoryId);
        command.Parameters.AddWithValue("@name", Deck.Name);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        const string getLastIdSql = "SELECT last_insert_rowid()";
        SqliteCommand getLastIdCommand = new(getLastIdSql, connection);
        object? lastId = await getLastIdCommand.ExecuteScalarAsync();

        if (lastId is null)
        {
            throw new InvalidOperationException("Unable to retrieve last insert row ID.");
        }

        return RedirectToPage("EditDeck", new { deckId = Convert.ToInt32(lastId) });
    }
}