using System.ComponentModel.DataAnnotations;
using Memodex.WebApp.Common;
using Memodex.WebApp.Infrastructure;
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
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            SELECT id, categoryId, name, description
            FROM decks
            WHERE id = @deckId;
            """);
        command.Parameters.AddWithValue("@deckId", deckId);
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

        await using SqliteConnection connection = SqliteConnectionFactory.Create(User, true);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            UPDATE decks
            SET name = @name, description = @description
            WHERE id = @id
            """);
        command.Parameters.AddWithValue("@id", Deck.Id);
        command.Parameters.AddWithValue("@name", Deck.Name);
        command.Parameters.AddWithValue("@description", Deck.Description is null ? DBNull.Value : Deck.Description);
        await command.ExecuteNonQueryAsync();

        this.AddNotification(NotificationType.Success, $"Deck {Deck.Name} updated.");

        return RedirectToPage("EditDeck", new { deckId = Deck.Id });
    }

    public async Task<IActionResult> OnPostDeleteDeckAsync(
        [FromQuery]
        int deckId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.Create(User, true);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            DELETE FROM decks 
            WHERE id = @id;
            """);
        command.Parameters.AddWithValue("@id", deckId);
        await command.ExecuteNonQueryAsync();

        this.AddNotification(NotificationType.Success, $"Deck {Deck.Name} deleted.");

        return RedirectToPage("BrowseDecks", new { categoryId = Deck.CategoryId });
    }
}
