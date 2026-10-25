using Microsoft.EntityFrameworkCore;
public class AppDataContext : DbContext
{
    public DbSet<Participante> Participantes { get; set; }
    public DbSet<Modalidade> Modalidades { get; set; }
    public DbSet<Inscricao> Inscricoes {get;set;}

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

        modelBuilder.Entity<Inscricao>()
            .HasIndex(i => new { i.ParticipanteId, i.ModalidadeId })
            .IsUnique();

        modelBuilder.Entity<Modalidade>().HasData(
            new Modalidade { Id = 1, Nome = "8 Ball" },
            new Modalidade { Id = 2, Nome = "9 Ball" },
            new Modalidade { Id = 3, Nome = "Snooker 15 reds" }
        );
    }
}