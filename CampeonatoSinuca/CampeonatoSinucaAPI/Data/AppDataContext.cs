using Microsoft.EntityFrameworkCore;
public class AppDataContext : DbContext
{
    public DbSet<Participante> Participantes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Data/Participantes.db");
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Participante>()
            .HasIndex(p => p.Cpf)
            .IsUnique();
    }
}