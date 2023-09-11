using System.ComponentModel.DataAnnotations;
using Memodex.WebApp.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Data.Sqlite;

namespace Memodex.WebApp.Pages;

public class EditDeck : PageModel
{
    public class DeckItem
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        [Required]
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
    }

    [BindProperty]
    public DeckItem Deck { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int deckId)
    {
        await using SqliteConnection connection = new($"Data Source=memodex_test.sqlite");
        const string sql = "SELECT `id`, `categoryId`, `name`, `description` FROM `decks` WHERE `id` = @deckId";
        SqliteCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@deckId", deckId);
        await connection.OpenAsync();
        SqliteDataReader reader = await command.ExecuteReaderAsync();
        if (!await reader.ReadAsync())
        {
            return RedirectToPage("Index");
        }

        Deck = new DeckItem
        {
            Id = reader.GetInt32(0),
            CategoryId = reader.GetInt32(1),
            Name = reader.GetString(2),
            Description = reader.GetValue(3) as string ?? string.Empty
        };
        
        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = "memodex_test.sqlite",
            ForeignKeys = true
        };
        await using SqliteConnection connection = new(connectionStringBuilder.ToString());
        const string sql = "UPDATE `decks` SET `name` = @name, `description` = @description WHERE `id` = @id";
        SqliteCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", Deck.Id);
        command.Parameters.AddWithValue("@name", Deck.Name);
        command.Parameters.AddWithValue("@description", Deck.Description is null ? DBNull.Value : Deck.Description);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();

        this.AddNotification(NotificationType.Success, $"Deck {Deck.Name} updated.");

        return RedirectToPage("EditDeck", new { deckId = Deck.Id });
    }

    public async Task<IActionResult> OnPostDeleteDeckAsync(
        [FromQuery]
        int deckId)
    {
        SqliteConnectionStringBuilder connectionStringBuilder = new()
        {
            DataSource = "memodex_test.sqlite",
            ForeignKeys = true
        };
        await using SqliteConnection connection = new(connectionStringBuilder.ToString());
        const string sql = "DELETE FROM `decks` WHERE `id` = @id";
        SqliteCommand command = new(sql, connection);
        command.Parameters.AddWithValue("@id", deckId);
        await connection.OpenAsync();
        await command.ExecuteNonQueryAsync();
        
        this.AddNotification(NotificationType.Success, $"Deck {Deck.Name} deleted.");

        return RedirectToPage("BrowseDecks", new { categoryId = Deck.CategoryId });
    }
}