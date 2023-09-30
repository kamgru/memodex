using System.ComponentModel.DataAnnotations;
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
public class Login : PageModel
{
    public class FormInput
    {
        [Required]
        public string Username { get; init; } = string.Empty;

        [Required]
        [DataType(DataType.Password)]
        public string Password { get; init; } = string.Empty;

        public bool RememberMe { get; init; }
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

        SqliteCommand command = mdxDbConnection.CreateCommand(
            """
            SELECT UserId, Username, PasswordHash
            FROM users
            WHERE username = @username
            LIMIT 1;
            """);
        string usernameNormalized = Input.Username.ToLowerInvariant();
        command.Parameters.AddWithValue("@username", usernameNormalized);

        await using SqliteDataReader reader = await command.ExecuteReaderAsync();

        if (!await reader.ReadAsync())
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        MdxUser user = new()
        {
            UserId = reader.GetString(0),
            Username = reader.GetString(1),
            PasswordHash = reader.GetString(2)
        };
        await mdxDbConnection.CloseAsync();

        PasswordHasher<MdxUser> hasher = new();
        if (hasher.VerifyHashedPassword(user, user.PasswordHash, Input.Password) == PasswordVerificationResult.Failed)
        {
            ModelState.AddModelError(string.Empty, "Invalid login attempt.");
            return Page();
        }

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, usernameNormalized),
            new Claim(ClaimTypes.NameIdentifier, user.UserId)
        };

        ClaimsIdentity identity = new(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        ClaimsPrincipal principal = new(identity);

        AuthenticationProperties authProps = new()
        {
            IsPersistent = Input.RememberMe,
            AllowRefresh = true,
            ExpiresUtc = DateTimeOffset.UtcNow.AddMinutes(60)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            principal,
            authProps);

        return RedirectToPage("/Index");
    }
}