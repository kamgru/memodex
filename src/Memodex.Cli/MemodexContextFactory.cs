using Microsoft.Extensions.Configuration;

namespace Memodex.Cli;

public class MemodexContextFactory
{
    private readonly IConfiguration _configuration;

    public MemodexContextFactory(
        IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public MemodexContext Create()
    {
        string connectionString = _configuration.GetConnectionString("MemodexDb")
            ?? throw new InvalidOperationException("Missing configuration for MemodexDb.");
        
        DbContextOptionsBuilder<MemodexContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(connectionString);

        MemodexContext memodexContext = new(optionsBuilder.Options);
        return memodexContext;
    }
}