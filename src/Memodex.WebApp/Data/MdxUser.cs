namespace Memodex.WebApp.Data;

public class MdxUser
{
    public string UserId { get; init; } = string.Empty;
    public string Username { get; init; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
}
