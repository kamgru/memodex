using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
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

    private readonly SqliteConnectionFactory _sqliteConnectionFactory;

    public EditDeck(
        SqliteConnectionFactory sqliteConnectionFactory)
    {
        _sqliteConnectionFactory = sqliteConnectionFactory;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(
        int deckId)
    {
        EditDeckReader editDeckReader = new(_sqliteConnectionFactory, User);
        FormInput? formInput = await editDeckReader.GetDeckAsync(deckId);

        if (formInput is null)
        {
            return RedirectToPage("Index");
        }

        Input = formInput;

        return Page();
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        EditDeckWriter editDeckWriter = new(_sqliteConnectionFactory, User);
        await editDeckWriter.EditDeckAsync(
            Input.Id,
            Input.Name,
            Input.Description);

        this.AddNotification(NotificationType.Success, $"Deck {Input.Name} updated.");

        return RedirectToPage("EditDeck", new { deckId = Input.Id });
    }

    public async Task<IActionResult> OnPostDeleteDeckAsync(
        [FromQuery]
        int deckId)
    {
        await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(User, true);
        await connection.OpenAsync();

        DeleteDeckWriter deleteDeckWriter = new(_sqliteConnectionFactory, User);
        await deleteDeckWriter.DeleteDeckAsync(deckId);

        this.AddNotification(NotificationType.Success, $"Deck {Input.Name} deleted.");

        return RedirectToPage("BrowseDecks");
    }

    public class EditDeckWriter
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public EditDeckWriter(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task EditDeckAsync(
            int deckId,
            string name,
            string? description)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal, true);
            await connection.OpenAsync();

            SqliteCommand command = connection.CreateCommand(
                """
                UPDATE decks
                SET name = @name, description = @description
                WHERE id = @id
                """);
            command.Parameters.AddWithValue("@id", deckId);
            command.Parameters.AddWithValue("@name", name);
            command.Parameters.AddWithValue("@description", description is null ? DBNull.Value : description);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
    }

    public class EditDeckReader
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public EditDeckReader(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task<FormInput?> GetDeckAsync(
            int deckId)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal);
            await connection.OpenAsync();
            await using SqliteCommand command = connection.CreateCommand(
                """
                SELECT name, description
                FROM decks
                WHERE id = @deckId;
                """);
            command.Parameters.AddWithValue("@deckId", deckId);
            SqliteDataReader reader = await command.ExecuteReaderAsync();

            if (!await reader.ReadAsync())
            {
                return null;
            }

            return new FormInput
            {
                Id = deckId,
                Name = reader.GetString(0),
                Description = reader.GetValue(1) as string ?? string.Empty
            };
        }
    }

    public class DeleteDeckWriter
    {
        private readonly SqliteConnectionFactory _sqliteConnectionFactory;
        private readonly ClaimsPrincipal _claimsPrincipal;

        public DeleteDeckWriter(
            SqliteConnectionFactory sqliteConnectionFactory,
            ClaimsPrincipal claimsPrincipal)
        {
            _sqliteConnectionFactory = sqliteConnectionFactory;
            _claimsPrincipal = claimsPrincipal;
        }

        public async Task DeleteDeckAsync(
            int deckId)
        {
            await using SqliteConnection connection = _sqliteConnectionFactory.CreateForUser(_claimsPrincipal, true);
            await connection.OpenAsync();

            SqliteCommand command = connection.CreateCommand(
                """
                DELETE FROM decks
                WHERE id = @id;
                """);
            command.Parameters.AddWithValue("@id", deckId);
            await command.ExecuteNonQueryAsync();
            await connection.CloseAsync();
        }
    }
}
