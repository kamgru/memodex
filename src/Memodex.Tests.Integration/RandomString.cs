namespace Memodex.Tests.Integration;

public static class RandomString
{
    private static readonly char[] Chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    public static string Generate(
        int length = 10)
    {
        Random random = new();
        string result = new(
            Enumerable.Repeat(Chars, length)
                .Select(chars => chars[random.Next(chars.Length)])
                .ToArray());
        return result;
    }
}
