using Microsoft.EntityFrameworkCore;
public class AppDataContext : DbContext
{
    public DbSet<Participante> Participantes { get; set; }
    public DbSet<Modalidade> Modalidades { get; set; }
    public DbSet<Inscricao> Inscricoes {get;set;}
    public DbSet<Partida> Partidas {get;set;}
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

        modelBuilder.Entity<Partida>(partida =>
        {
            partida.HasOne(p => p.Participante1)
                .WithMany()
                .HasForeignKey(p => p.Participante1Id)
                .OnDelete(DeleteBehavior.Restrict);

            partida.HasOne(p => p.Participante2)
                .WithMany()
                .HasForeignKey(p => p.Participante2Id)
                .OnDelete(DeleteBehavior.Restrict);

            partida.HasOne(p => p.Vencedor)
                .WithMany()
                .HasForeignKey(p => p.VencedorId)
                .OnDelete(DeleteBehavior.Restrict);
});
    }
}