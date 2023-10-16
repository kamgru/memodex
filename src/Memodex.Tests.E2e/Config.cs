namespace Memodex.Tests.E2e;

public static class Config
{
    public static string BaseUrl => TestContext.Parameters["MemodexBaseUrl"]
                                    ?? throw new InvalidOperationException(
                                        "Missing 'MemodexBaseUrl' parameter in .runsettings file");
}
