using Microsoft.EntityFrameworkCore;
public class AppDataContext : DbContext
{
    public DbSet<Participante> Participantes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder.UseSqlite("Data Source=Participantes.db");
    }
}