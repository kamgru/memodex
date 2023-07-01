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
    public DbSet<Profile> Profiles => Set<Profile>();
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Flashcard>()
            .HasOne(flashcard => flashcard.Deck)
            .WithMany(category => category.Flashcards);

        modelBuilder.Entity<Deck>()
            .HasOne(deck => deck.Category)
            .WithMany(category => category.Decks);
    }
}