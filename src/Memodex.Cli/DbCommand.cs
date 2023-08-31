namespace Memodex.Cli;

public class DbCommand : Command
{
    public DbCommand()
        : base("db", "Manage database")
    {
        AddCommand(new DbWipeCommand());
    }
}

public class DbWipeCommand : Command
{
    public DbWipeCommand()
        : base("wipe", "Wipe database")
    {
        this.SetHandler(Handle);
    }
    
    private void Handle(
        InvocationContext invocationContext)
    {
        MemodexContext memodexContext = invocationContext.GetRequiredService<MemodexContextFactory>()
            .Create();

        memodexContext.Database.EnsureDeleted();
        memodexContext.Database.EnsureCreated();
    }
}
