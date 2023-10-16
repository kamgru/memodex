namespace Memodex.Tests.Integration.DataAccess;

public abstract class TestFixtureBase
{
    protected DbFixture DbFixture = new();

    [SetUp]
    public async Task SetUp()
    {
        DbFixture = new DbFixture();
        await DbFixture.CreateUserDb();
    }

    [TearDown]
    public void TearDown()
    {
        DbFixture?.Dispose();
    }
}
