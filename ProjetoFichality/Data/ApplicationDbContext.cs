using Microsoft.EntityFrameworkCore;
using ProjetoFichality.Models;

namespace ProjetoFichality.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Ficha> Fichas => Set<Ficha>();
    public DbSet<SistemaRpg> SistemasRpg { get; set; }
    public DbSet<ModeloFicha> ModelosFicha { get; set; }
    public DbSet<Destaque> Destaques { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>()
            .HasIndex(u => u.Email)
            .IsUnique();

        modelBuilder.Entity<Ficha>()
            .Property(f => f.EstruturaJson)
            .HasColumnType("nvarchar(max)");

        modelBuilder.Entity<Ficha>()
            .HasOne(f => f.Usuario)
            .WithMany(u => u.Fichas)
            .HasForeignKey(f => f.UsuarioId)
            .OnDelete(DeleteBehavior.Cascade);


        // Um sistema pertence ao usuário que o criou.
        modelBuilder.Entity<SistemaRpg>()
            .HasOne(s => s.CriadorUsuario)
            .WithMany()
            .HasForeignKey(s => s.CriadorUsuarioId)
            .OnDelete(DeleteBehavior.NoAction);

        // Uma cópia pode indicar o sistema de origem.
        modelBuilder.Entity<SistemaRpg>()
            .HasOne(s => s.SistemaOrigem)
            .WithMany(s => s.Copias)
            .HasForeignKey(s => s.SistemaOrigemId)
            .OnDelete(DeleteBehavior.NoAction);

        // Um sistema pode ter vários modelos de ficha.
        modelBuilder.Entity<ModeloFicha>()
            .HasOne(m => m.SistemaRpg)
            .WithMany(s => s.Modelos)
            .HasForeignKey(m => m.SistemaRpgId)
            .OnDelete(DeleteBehavior.Cascade);

        // As fichas podem ser associadas a um sistema.
        modelBuilder.Entity<Ficha>()
            .HasOne(f => f.Sistema)
            .WithMany(s => s.Fichas)
            .HasForeignKey(f => f.SistemaRpgId)
            .OnDelete(DeleteBehavior.NoAction);

        // As fichas podem ser criadas a partir de um modelo.
        modelBuilder.Entity<Ficha>()
            .HasOne(f => f.ModeloFicha)
            .WithMany(m => m.Fichas)
            .HasForeignKey(f => f.ModeloFichaId)
            .OnDelete(DeleteBehavior.NoAction);

        // A estrutura do modelo também é armazenada em JSON.
        modelBuilder.Entity<ModeloFicha>()
            .Property(m => m.EstruturaJson)
            .HasColumnType("nvarchar(max)");

        // Os destaques podem apontar para sistemas.
        modelBuilder.Entity<Destaque>()
            .HasOne(d => d.SistemaRpg)
            .WithMany()
            .HasForeignKey(d => d.SistemaRpgId)
            .OnDelete(DeleteBehavior.NoAction);

        // Ou para modelos específicos.
        modelBuilder.Entity<Destaque>()
            .HasOne(d => d.ModeloFicha)
            .WithMany()
            .HasForeignKey(d => d.ModeloFichaId)
            .OnDelete(DeleteBehavior.NoAction);

    }
}
