namespace Memodex.Tests.Integration.DataAccess;

[TestFixture]
public class MemodexDatabaseTests : TestFixtureBase
{
    [Test]
    public async Task AddUserAsync_WhenUserAdded_ReturnsSuccess()
    {
        // Arrange
        string username = RandomString.Generate();
        string password = Guid.NewGuid()
            .ToString();

        DbFixture dbFixture = new();
        MemodexDatabase memodexDatabase = new(dbFixture.SqliteConnectionFactory);
        await memodexDatabase.EnsureExistsAsync();

        // Act
        AddUserResult addUserResult = await memodexDatabase.AddUserAsync(username, password);

        // Assert
        Assert.That(addUserResult.IsSuccess, Is.True);

        // Cleanup
        await memodexDatabase.DeleteUserAsync(addUserResult.User!.UserId);
    }

    [Test]
    public async Task AddUserAsync_WhenUserAdded_ReturnsUser()
    {
        // Arrange
        string username = RandomString.Generate();
        string expectedUsername = username.ToLowerInvariant();
        string password = Guid.NewGuid()
            .ToString();

        DbFixture dbFixture = new();
        MemodexDatabase memodexDatabase = new(dbFixture.SqliteConnectionFactory);
        await memodexDatabase.EnsureExistsAsync();

        // Act
        AddUserResult addUserResult = await memodexDatabase.AddUserAsync(username, password);
        MdxUser? user = addUserResult.User;

        // Assert
        Assert.That(user, Is.Not.Null);
        Assert.That(user?.Username, Is.EqualTo(expectedUsername));

        // Cleanup
        await memodexDatabase.DeleteUserAsync(addUserResult.User!.UserId);
    }

    [Test]
    public async Task AddUserAsync_WhenUserExists_ReturnsFailure()
    {
        // Arrange
        string username = RandomString.Generate();
        string password = Guid.NewGuid()
            .ToString();

        DbFixture dbFixture = new();
        MemodexDatabase memodexDatabase = new(dbFixture.SqliteConnectionFactory);
        await memodexDatabase.EnsureExistsAsync();

        // Act
        AddUserResult initialAddUserResult = await memodexDatabase.AddUserAsync(username, password);
        AddUserResult addUserResult = await memodexDatabase.AddUserAsync(username, password);

        // Assert
        Assert.That(addUserResult.IsSuccess, Is.False);

        // Cleanup
        await memodexDatabase.DeleteUserAsync(initialAddUserResult.User!.UserId);
    }

    [Test]
    public async Task DeleteUserAsync_DeletesUser()
    {
        // Arrange
        string username = RandomString.Generate();
        string password = Guid.NewGuid()
            .ToString();

        DbFixture dbFixture = new();
        MemodexDatabase memodexDatabase = new(dbFixture.SqliteConnectionFactory);
        await memodexDatabase.EnsureExistsAsync();
        AddUserResult addUserResult = await memodexDatabase.AddUserAsync(username, password);

        // Act
        await memodexDatabase.DeleteUserAsync(addUserResult.User!.UserId);

        // Assert
        await using SqliteConnection connection = dbFixture.SqliteConnectionFactory.CreateForApp();
        await connection.OpenAsync();
        await using SqliteCommand command = connection.CreateCommand(
            """
            SELECT COUNT(*)
            FROM users
            WHERE userId = @userId;
            """);
        command.Parameters.AddWithValue("@userId", addUserResult.User!.UserId);
        long count = Convert.ToInt64(await command.ExecuteScalarAsync());
        await connection.CloseAsync();

        Assert.That(count, Is.EqualTo(0));
    }
}
