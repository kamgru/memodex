using Memodex.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Memodex.Cli;

public static class MemodexContextFactory
{
    private static MemodexContext? _memodexContext;
    
    public static MemodexContext Create(string? connectionString = null)
    {
        if (_memodexContext is not null)
        {
            return _memodexContext;
        }
        
        connectionString ??= ConnectionStringManager.GetConnectionString().Value;
        
        DbContextOptionsBuilder<MemodexContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(connectionString);
            
        _memodexContext = new MemodexContext(optionsBuilder.Options);
        return _memodexContext;
    }
}