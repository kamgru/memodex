using Microsoft.EntityFrameworkCore;

namespace Memodex.WebApp.Data;

public class MemodexContext : DbContext
{
    public MemodexContext(
        DbContextOptions<MemodexContext> options)
        : base(options)
    {
    }

    public DbSet<Flashcard> Flashcards => Set<Flashcard>();
    public DbSet<Category> Categories => Set<Category>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flashcard>()
            .HasOne(flashcard => flashcard.Category)
            .WithMany(category => category.Flashcards);
    }
}