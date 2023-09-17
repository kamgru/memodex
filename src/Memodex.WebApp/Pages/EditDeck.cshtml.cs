using System.ComponentModel.DataAnnotations;
using Memodex.WebApp.Common;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

public class EditDeck : PageModel
{
    public class FormInput
    {
        public int Id { get; init; }

        [Required]
        public string Name { get; init; } = string.Empty;

        public string? Description { get; init; }
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int deckId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            SELECT name, description
            FROM decks
            WHERE id = @deckId;
            """);
        command.Parameters.AddWithValue("@deckId", deckId);
        SqliteDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            return RedirectToPage("Index");
        }

        Input = new FormInput
        {
            Id = deckId,
            Name = reader.GetString(0),
            Description = reader.GetValue(1) as string ?? string.Empty
        };

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

        SqliteCommand command = connection.CreateCommand(
            """
            UPDATE decks
            SET name = @name, description = @description
            WHERE id = @id
            """);
        command.Parameters.AddWithValue("@id", Input.Id);
        command.Parameters.AddWithValue("@name", Input.Name);
        command.Parameters.AddWithValue("@description", Input.Description is null ? DBNull.Value : Input.Description);
        await command.ExecuteNonQueryAsync();

        this.AddNotification(NotificationType.Success, $"Deck {Input.Name} updated.");

        return RedirectToPage("EditDeck", new { deckId = Input.Id });
    }

    public async Task<IActionResult> OnPostDeleteDeckAsync(
        [FromQuery]
        int deckId)
    {
        await using SqliteConnection connection = SqliteConnectionFactory.CreateForUser(User, true);
        await connection.OpenAsync();

        SqliteCommand command = connection.CreateCommand(
            """
            DELETE FROM decks 
            WHERE id = @id;
            """);
        command.Parameters.AddWithValue("@id", deckId);
        await command.ExecuteNonQueryAsync();

        this.AddNotification(NotificationType.Success, $"Deck {Input.Name} deleted.");

        return RedirectToPage("BrowseDecks");
    }
}
