using System.ComponentModel.DataAnnotations;
using System.Data.Common;
using System.Security.Claims;
using Memodex.WebApp.Data;
using Memodex.WebApp.Infrastructure;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace Memodex.WebApp.Pages;

[AllowAnonymous]
public class Register : PageModel
{
    public class FormInput
    {
        [Required]
        [RegularExpression("^[a-zA-Z|0-9|_]*$",
            ErrorMessage = "Letters, numbers, underscore. No more than 32 character")]
        [MaxLength(32)]
        public string Username { get; init; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; init; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        [Compare("Password")]
        public string ConfirmPassword { get; init; } = string.Empty;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        await using SqliteConnection mdxDbConnection = SqliteConnectionFactory.CreateForApp();
        await mdxDbConnection.OpenAsync();
        await using DbTransaction mdxDbTransaction = await mdxDbConnection.BeginTransactionAsync();

        await using SqliteCommand userExistsCmd = mdxDbConnection.CreateCommand(
            """
            SELECT EXISTS (
                SELECT 1
                FROM users
                WHERE username = @username
            );
            """);
        string usernameNormalized = Input.Username.ToLowerInvariant();
        userExistsCmd.Parameters.AddWithValue("@username", usernameNormalized);
        bool userExists = Convert.ToBoolean(await userExistsCmd.ExecuteScalarAsync());
        if (userExists)
        {
            ModelState.AddModelError("", $"User {usernameNormalized} already exists.");
            return Page();
        }

        MdxUser user = new()
        {
            Username = usernameNormalized,
            UserId = Guid.NewGuid()
                .ToString()
        };
        PasswordHasher<MdxUser> hasher = new();
        user.PasswordHash = hasher.HashPassword(user, Input.Password);

        await using SqliteCommand addUserCmd = mdxDbConnection.CreateCommand(
            """
            INSERT INTO users (userId, username, passwordHash)
            VALUES (@userId, @username, @passwordHash);
            """);
        addUserCmd.Parameters.AddWithValue("@userId", user.UserId);
        addUserCmd.Parameters.AddWithValue("@username", user.Username);
        addUserCmd.Parameters.AddWithValue("@passwordHash", user.PasswordHash);
        await addUserCmd.ExecuteNonQueryAsync();

        await mdxDbTransaction.CommitAsync();

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, usernameNormalized),
            new Claim(ClaimTypes.NameIdentifier, user.UserId)
        };

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        AuthenticationProperties authProps = new()
        {
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProps);

        await using SqliteConnection userDbConnection = SqliteConnectionFactory.CreateForUser(
            principal,
            createIfNotExists: true);
        await userDbConnection.OpenAsync();
        await using DbTransaction userDbTransaction = await userDbConnection.BeginTransactionAsync();

        await using SqliteCommand createDbCmd = userDbConnection.CreateCommand(
            """
            create table if not exists main.decks
            (
                id             integer not null
                    constraint decks_pk primary key autoincrement,
                name           text    not null,
                description    text,
                flashcardCount integer not null default 0
            );

            create table if not exists main.flashcards
            (
                id          integer not null
                        constraint flashcards_pk
                            primary key autoincrement,
                ordinalNumber integer not null default 1,
                question    text    not null,
                answer      text    not null,
                deckId      integer not null
                        constraint flashcards_decks_id_fk references decks (id) on delete cascade
            );

            create table if not exists main.challenges
            (
                id integer not null
                    constraint challenge_pk
                        primary key autoincrement,
                deckId integer not null
                    constraint challenge_decks_id_fk references decks(id) on delete cascade,
                state integer not null default 0,
                currentStepIndex integer not null default 0,
                stepCount integer not null default 0,
                createdAt text not null default CURRENT_TIMESTAMP,
                updatedAt text not null default CURRENT_TIMESTAMP
            );

            create table if not exists main.steps
            (
                id integer not null
                    constraint step_pk
                        primary key autoincrement,
                stepIndex integer not null,
                needsReview integer not null default 0,
                flashcardId integer not null
                    constraint step_flashcards_id_fk references flashcards(id) on delete cascade,
                challengeId integer not null
                    constraint step_challenges_id_fk references challenges(id) on delete cascade
            );

            create table if not exists main.preferences
            (
                key text not null
                    constraint preferences_pk
                        primary key,
                value text not null
            );
            """);
        await createDbCmd.ExecuteNonQueryAsync();

        await using SqliteCommand addPrefsCmd = userDbConnection.CreateCommand(
            """
            INSERT INTO main.preferences (key, value)
            VALUES
                ('preferredTheme', 'light'),
                ('name', @username),
                ('avatar', 'default.png');
            """);
        addPrefsCmd.Parameters.AddWithValue("@username", user.Username);
        await addPrefsCmd.ExecuteNonQueryAsync();

        await userDbTransaction.CommitAsync();
        await userDbConnection.CloseAsync();

        return RedirectToPage("/Index");
    }
}