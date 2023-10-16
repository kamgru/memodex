namespace Memodex.WebApp.Data;

public class UserDatabaseName
{
    private readonly string _userDatabaseName;

    public UserDatabaseName(
        string username)
    {
        if (string.IsNullOrWhiteSpace(username))
        {
            throw new ArgumentException("Value cannot be null or whitespace.", nameof(username));
        }

        _userDatabaseName = $"mdx.{username.ToLowerInvariant()}.db";
    }

    public override string ToString()
    {
        return _userDatabaseName;
    }
}
