namespace Memodex.Tests.E2e;

public static class RandomString
{
    private static readonly char[] _chars =
        "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789".ToCharArray();

    public static string Generate(
        int length = 10)
    {
        Random random = new();
        string result = new(
            Enumerable.Repeat(_chars, length)
                .Select(chars => chars[random.Next(chars.Length)])
                .ToArray());
        return result;
    }
}
