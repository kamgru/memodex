using Memodex.DataAccess;
using Microsoft.EntityFrameworkCore;

namespace Memodex.Cli;

public static class MemodexContextFactory
{
    private static MemodexContext? _memodexContext;
    
    public static MemodexContext Create()
    {
        if (_memodexContext is not null)
        {
            return _memodexContext;
        }
        DbContextOptionsBuilder<MemodexContext> optionsBuilder = new();
        optionsBuilder.UseSqlServer(ConnectionStringManager.GetConnectionString().Value);
            
        _memodexContext = new MemodexContext(optionsBuilder.Options);
        return _memodexContext;
    }
}