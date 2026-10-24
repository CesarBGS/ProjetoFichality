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
    }
}
