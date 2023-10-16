using System.ComponentModel.DataAnnotations;
using System.Security.Claims;
using Memodex.WebApp.Data;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
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

    private readonly MemodexDatabase _memodexDatabase;
    private readonly UserDatabase _userDatabase;

    public Register(
        UserDatabase userDatabase,
        MemodexDatabase memodexDatabase)
    {
        _userDatabase = userDatabase;
        _memodexDatabase = memodexDatabase;
    }

    [BindProperty]
    public FormInput Input { get; set; } = new();

    public async Task<IActionResult> OnPost()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        AddUserResult result = await _memodexDatabase.AddUserAsync(Input.Username, Input.Password);
        if (!result.IsSuccess)
        {
            ModelState.AddModelError("", $"User {Input.Username} already exists.");
            return Page();
        }

        List<Claim> claims = new()
        {
            new Claim(ClaimTypes.Name, result.User!.Username),
            new Claim(ClaimTypes.NameIdentifier, result.User.UserId)
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

        await _userDatabase.CreateAsync(principal);

        return RedirectToPage("/Index");
    }
}
