namespace Memodex.Tests.Integration.DataAccess;

public abstract class TestFixtureBase
{
    protected readonly DbFixture DbFixture = new();

    [OneTimeSetUp]
    public async Task OneTimeSetUp() =>
        await DbFixture.CreateUserDb();

    [OneTimeTearDown]
    public void OneTimeTearDown() =>
        DbFixture.Dispose();
}
