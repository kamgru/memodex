namespace Memodex.Cli;

public record ConnectionString(string Value);

public static class ConnectionStringManager
{
    private static readonly string MemodexDirectory;
    private static readonly string ConfigFilename;
    
    private static string _connectionString = string.Empty;

    static ConnectionStringManager()
    {
        string homeDirectory = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile);
        MemodexDirectory = Path.Combine(homeDirectory, ".memodex");
        ConfigFilename = Path.Combine(MemodexDirectory, ".config");
    }

    public static void SetConnectionString(ConnectionString connectionString)
    {
        if (string.IsNullOrWhiteSpace(connectionString.Value))
        {
            throw new InvalidOperationException("Connection string cannot be empty.");
        }
        
        if (!Directory.Exists(MemodexDirectory))
        {
            Directory.CreateDirectory(MemodexDirectory);
        }

        using StreamWriter file = File.CreateText(ConfigFilename);
        file.WriteLine(connectionString.Value);
        
        _connectionString = connectionString.Value;
    }

    public static ConnectionString GetConnectionString()
    {
        if (!string.IsNullOrWhiteSpace(_connectionString))
        {
            return new ConnectionString(_connectionString);
        }
        
        if (!File.Exists(ConfigFilename))
        {
            return new ConnectionString(string.Empty);
        }

        using TextReader textReader = File.OpenText(ConfigFilename);
        string? connectionString = textReader.ReadLine();
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return new ConnectionString(string.Empty);
        }

        return new ConnectionString(connectionString);
    }
}